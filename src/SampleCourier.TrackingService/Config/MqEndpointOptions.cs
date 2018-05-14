using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.TrackingService.Config
{
	internal class MqEndpointOptions
	{
		public EndpointConfig Metrics { get; set; }

		public ActivityEndpointConfig ActivityMetrics { get; set; }

		public EndpointConfig Saga { get; set; }
	}

	internal class EndpointConfig
	{
		public ushort PrefetchCount { get; set; }
		public string QueueName { get; set; }
		public int RetryLimit { get; set; }
		public int ConcurrencyLimit { get; set; }
	}

	internal class ActivityEndpointConfig : EndpointConfig
	{
		public string ActivityName { get; set; }
	}
}
