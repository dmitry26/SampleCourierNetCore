using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using Microsoft.Extensions.Options;
using SampleCourier.Contracts;
using SampleCourier.WebApi.Config;

namespace SampleCourier.WebApi.Controllers
{
	public class RoutingSlipPublisher
	{
		public RoutingSlipPublisher(IBusControl bus,IOptions<MqActivityOptions> actCfg)
		{
			_bus = bus;
			_activityConfig = actCfg.Value;
		}

		private readonly IBus _bus;
		private readonly MqActivityOptions _activityConfig;

		public async Task<Guid> Publish(Uri reqUri)
		{
			var builder = new RoutingSlipBuilder(NewId.NextGuid());

			builder.AddActivity("Validate",new Uri(_activityConfig.ValidateAddress));
			builder.AddActivity("Retrieve",new Uri(_activityConfig.RetrieveAddress));

			builder.SetVariables(new
			{
				RequestId = NewId.NextGuid(),
				Address = reqUri,
			});

			var routingSlip = builder.Build();


			await _bus.Publish<RoutingSlipCreated>(new
			{
				routingSlip.TrackingNumber,
				Timestamp = routingSlip.CreateTimestamp,
			});

			await _bus.Execute(routingSlip);

			return routingSlip.TrackingNumber;
		}
	}
}
