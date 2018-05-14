using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;

namespace SampleCourier.TrackingService.Service
{
	public class RoutingSlipMetricsConsumer : IConsumer<RoutingSlipCompleted>
	{
		readonly RoutingSlipMetrics _metrics;

		public RoutingSlipMetricsConsumer(RoutingSlipMetrics metrics) => _metrics = metrics;

		public Task Consume(ConsumeContext<RoutingSlipCompleted> context)
		{
			_metrics.AddComplete(context.Message.Duration);
			return Task.CompletedTask;
		}
	}
}