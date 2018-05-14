using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;

namespace SampleCourier.TrackingService.Service
{
	public class RoutingSlipActivityConsumer : IConsumer<RoutingSlipActivityCompleted>
	{
		readonly string _activityName;
		readonly RoutingSlipMetrics _metrics;

		public RoutingSlipActivityConsumer(RoutingSlipMetrics metrics,string activityName)
		{
			_metrics = metrics;
			_activityName = activityName;
		}

		public Task Consume(ConsumeContext<RoutingSlipActivityCompleted> context)
		{
			if (context.Message.ActivityName.Equals(_activityName,StringComparison.OrdinalIgnoreCase))
				_metrics.AddComplete(context.Message.Duration);

			return Task.CompletedTask;
		}
	}
}