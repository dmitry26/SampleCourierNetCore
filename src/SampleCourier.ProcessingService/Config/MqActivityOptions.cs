using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.ProcessingService.Config
{
	internal class MqActivityOptions
	{
		public ActivityConfig ValidateActivity { get; set; }

		public ActivityConfig RetrieveActivity { get; set; }

		public ActivityConfig CompensateRetrieveActivity { get; set; }
	}

	internal class ActivityConfig
	{
		public int PrefetchCount { get; set; }
		public string QueueName { get; set; }
		public int RetryLimit { get; set; }
	}
}
