using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Logging;
using SampleCourier.Contracts;

namespace SampleCourier.Activities.Retrieve
{
	public class RetrieveActivity : Activity<RetrieveArguments,RetrieveLog>
	{
		static readonly ILog _logger = Logger.Get<RetrieveActivity>();

		public async Task<ExecutionResult> Execute(ExecuteContext<RetrieveArguments> context)
		{
			var sourceAddress = context.Arguments.Address;

			_logger.InfoFormat("Retrieve Content: {0}",sourceAddress);

			try
			{
				using (var client = new HttpClient())
				{
					var response = await client.GetAsync(sourceAddress);

					if (response.IsSuccessStatusCode)
					{
						var localFileName = GetLocalFileName(response,sourceAddress);

						_logger.InfoFormat("Success, copying to local file: {0}",GetRelativePath(Environment.CurrentDirectory,localFileName));

						using (var stream = File.Create(localFileName,4096,FileOptions.Asynchronous))
						{
							await response.Content.CopyToAsync(stream).ConfigureAwait(false);
						}

						var fileInfo = new FileInfo(localFileName);
						var localAddress = new Uri(fileInfo.FullName);

						_logger.InfoFormat("Completed, length = {0}",fileInfo.Length);

						await context.Publish<ContentRetrieved>(new
						{
							Timestamp = DateTime.UtcNow,
							Address = sourceAddress,
							LocalPath = fileInfo.FullName,
							LocalAddress = localAddress,
							fileInfo.Length,
							ContentType = response.Content.Headers.ContentType.ToString(),
						}).ConfigureAwait(false);

						return context.Completed<RetrieveLog>(new Log(fileInfo.FullName));
					}

					string message = string.Format("Server returned a response status code: {0} ({1})",
						(int)response.StatusCode,response.StatusCode);

					_logger.ErrorFormat("Failed to retrieve image: {0}",message);

					await context.Publish<ContentNotFound>(new
					{
						Timestamp = DateTime.UtcNow,
						Address = sourceAddress,
						Reason = message,
					}).ConfigureAwait(false);

					return context.Completed();
				}
			}
			catch (HttpRequestException exception)
			{
				_logger.Error("Exception from HttpClient",exception.InnerException);
				throw;
			}
		}

		static string GetLocalFileName(HttpResponseMessage response,Uri sourceAddress)
		{
			var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"content");

			Directory.CreateDirectory(localFilePath);

			var localFileName = Path.GetFullPath(Path.Combine(localFilePath,NewId.NextGuid().ToString()));
			Uri contentLocation = response.Content.Headers.ContentLocation ?? sourceAddress;

			if (response.Content.Headers.ContentDisposition != null
				&& Path.HasExtension(response.Content.Headers.ContentDisposition.FileName))
				localFileName += Path.GetExtension(response.Content.Headers.ContentDisposition.FileName);
			else if (Path.HasExtension(contentLocation.AbsoluteUri))
				localFileName += Path.GetExtension(contentLocation.AbsoluteUri);

			return localFileName;
		}

		public static string GetRelativePath(string basePath,string fullPath)
		{
			// Require trailing backslash for path
			if (!basePath.EndsWith("\\"))
				basePath += "\\";

			var baseUri = new Uri(basePath);
			var fullUri = new Uri(fullPath);

			var relUri = baseUri.MakeRelativeUri(fullUri);

			// Uri's use forward slashes so convert back to backward slashes
			return relUri.ToString().Replace("/","\\");
		}

		public Task<CompensationResult> Compensate(CompensateContext<RetrieveLog> context)
		{
			if (_logger.IsErrorEnabled) _logger.ErrorFormat("Removing local file: {0}",context.Log.LocalFilePath);

			return Task.FromResult(context.Compensated());
		}

		class Log : RetrieveLog
		{
			public Log(string localFilePath) => LocalFilePath = localFilePath;

			public string LocalFilePath { get; private set; }
		}
	}
}