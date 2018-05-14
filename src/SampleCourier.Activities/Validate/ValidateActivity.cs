using System;
using System.Threading.Tasks;
using MassTransit.Courier;
using MassTransit.Logging;
using SampleCourier.Contracts;

namespace SampleCourier.Activities.Validate
{
	/// <summary>
	/// Validates the address of the request to ensure it isn't an unacceptable domain
	/// </summary>
	public class ValidateActivity : ExecuteActivity<ValidateArguments>
	{
		static readonly ILog _logger = Logger.Get<ValidateActivity>();

		public async Task<ExecutionResult> Execute(ExecuteContext<ValidateArguments> context)
		{
			try
			{
				var address = context.Arguments.Address;

				if (address == null)
				{
					await context.Publish<RequestRejected>(new
					{
						context.Arguments.RequestId,
						context.TrackingNumber,
						Timestamp = DateTime.UtcNow,
						ReasonCode = 500,
						ReasonText = "The address was not specified",
					}).ConfigureAwait(false);

					return context.Terminate();
				}

				if (address.Host.EndsWith("microsoft.com",StringComparison.OrdinalIgnoreCase))
				{
					await context.Publish<RequestRejected>(new
					{
						context.Arguments.RequestId,
						context.TrackingNumber,
						Timestamp = DateTime.UtcNow,
						ReasonCode = 403,
						ReasonText = "The host specified is forbidden: " + address.Host,
					}).ConfigureAwait(false);

					return context.Terminate();
				}

				if (_logger.IsInfoEnabled)
				{
					_logger.InfoFormat("Validated Request {0} for address {1}",context.Arguments.RequestId,
						context.Arguments.Address);
				}

				return context.Completed();
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				throw;
			}
		}
	}
}