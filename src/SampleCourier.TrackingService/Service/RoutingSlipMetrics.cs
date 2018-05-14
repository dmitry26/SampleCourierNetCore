using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SampleCourier.TrackingService.Service
{
	public class RoutingSlipMetrics
	{
		readonly ConcurrentBag<TimeSpan> _durations;
		long _completedCount;
		readonly string _description;
		readonly ILogger _logger;

		public RoutingSlipMetrics(string description,ILogger<RoutingSlipMetrics> logger)
		{
			_description = description;
			_logger = logger;
			_completedCount = 0;
			_durations = new ConcurrentBag<TimeSpan>();
		}

		public void AddComplete(TimeSpan duration)
		{
			var count = Interlocked.Increment(ref _completedCount);
			_durations.Add(duration);

			if (count % 10 == 0)
				Snapshot();
		}

		public void Snapshot()
		{
			var snapshot = _durations.ToArray();
			var averageDuration = snapshot.Average(x => x.TotalMilliseconds);

			_logger.LogInformation($"{snapshot.Length} {_description} Completed, {averageDuration:F0}ms (average)");
		}
	}
}