using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dmo.MassTransit;
using GreenPipes;
using MassTransit;
using MassTransit.Courier.Factories;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SampleCourier.Activities.Retrieve;
using SampleCourier.Activities.Validate;

namespace SampleCourier.ProcessingService.Config
{
	public static class MassTransitExts
	{
		public static void AddMassTransitWithRabbitMq(this IServiceCollection services,IConfiguration appConfig)
		{
			if (services == null)
				throw new ArgumentNullException("services");

			if (appConfig == null)
				throw new ArgumentNullException("appConfig");

			var cfgSection = appConfig.GetSection("RabbitMq");

			if (!cfgSection.Exists())
				throw new InvalidOperationException("Appsettings: 'RabbitMq' section was not found");

			services.Configure<RabbitMqHostOptions>(cfgSection);

			var actSection = appConfig.GetSection("Activities");

			if (!actSection.Exists())
				throw new InvalidOperationException("Appsettings: 'Activities' section was not found");

			services.Configure<MqActivityOptions>(actSection);

			services.AddMassTransit();

			services.AddSingleton(svcProv =>
			{
				var hostOpts = svcProv.GetService<IOptions<RabbitMqHostOptions>>().Value;
				var actOpts = svcProv.GetService<IOptions<MqActivityOptions>>().Value;

				return Bus.Factory.CreateUsingRabbitMq(cfg =>
				{
					var host = cfg.CreateHost(hostOpts);
					var validateOpts = actOpts.ValidateActivity;

					cfg.ReceiveEndpoint(host,validateOpts.QueueName,e =>
					{
						e.PrefetchCount = (ushort)validateOpts.PrefetchCount;
						e.ExecuteActivityHost(
							DefaultConstructorExecuteActivityFactory<ValidateActivity,ValidateArguments>.ExecuteFactory,
							c => c.UseRetry(r => r.Immediate(validateOpts.RetryLimit))
						);
					});

					var retrieveOpts = actOpts.RetrieveActivity;
					var compRetrieveOpts = actOpts.CompensateRetrieveActivity;

					var compAddress = new Uri(host.Address,compRetrieveOpts.QueueName);

					cfg.ReceiveEndpoint(host,retrieveOpts.QueueName,e =>
					{
						e.PrefetchCount = (ushort)retrieveOpts.PrefetchCount;
						e.ExecuteActivityHost<RetrieveActivity,RetrieveArguments>(compAddress,
							c => c.UseRetry(r => r.Immediate(retrieveOpts.RetryLimit)));
					});

					cfg.ReceiveEndpoint(host,compRetrieveOpts.QueueName,
						e => e.CompensateActivityHost<RetrieveActivity,RetrieveLog>(c =>
							c.UseRetry(r => r.Immediate(compRetrieveOpts.RetryLimit))));

					cfg.UseSerilog();
				});
			});
		}
	}
}
