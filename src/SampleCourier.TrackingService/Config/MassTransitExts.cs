// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Dmo.MassTransit;
using GreenPipes;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Saga;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleCourier.Models;
using SampleCourier.TrackingService.Service;

namespace SampleCourier.TrackingService.Config
{
	class ValidateActivityMatrics : RoutingSlipMetrics
	{
		public ValidateActivityMatrics(ILogger<ValidateActivityMatrics> logger) : base("Validate Activity",logger) { }
	}

	public static class MassTransitExts
	{
		public static void AddMassTransitWithRabbitMq(this IServiceCollection services,IConfiguration appConfig)
		{
			if (services == null)
				throw new ArgumentNullException("services");

			if (appConfig == null)
				throw new ArgumentNullException("appConfig");

			var cfgSection = appConfig.GetSection("RabbitMqHost");

			if (!cfgSection.Exists())
				throw new InvalidOperationException("Appsettings: 'RabbitMqHost' section is not found");

			services.Configure<RabbitMqHostOptions>(cfgSection);

			var epSection = appConfig.GetSection("MqEndpoints");

			if (!epSection.Exists())
				throw new InvalidOperationException("Appsettings: 'MqEndpoints' section was not found");

			services.Configure<MqEndpointOptions>(epSection);

			services.AddMassTransit(cfg =>
			{
				cfg.AddConsumer<RoutingSlipMetricsConsumer>();
				cfg.AddConsumer<RoutingSlipActivityConsumer>();
				cfg.AddSaga<RoutingSlipState>();
			});

			services.AddSingleton<RoutingSlipStateMachine>();

			services.AddSingleton(svcProv =>
			{
				var fact = svcProv.GetService<ILoggerFactory>();
				return new RoutingSlipMetrics("Routing Slip",fact.CreateLogger<RoutingSlipMetrics>());
			});

			services.AddSingleton<ValidateActivityMatrics>();

			services.AddScoped<RoutingSlipMetricsConsumer>(svcProv =>
			{
				var metrics = svcProv.GetService<RoutingSlipMetrics>();
				return new RoutingSlipMetricsConsumer(metrics);
			});

			services.AddScoped(svcProv =>
			{
				var metrics = svcProv.GetService<ValidateActivityMatrics>();
				var epOpts = svcProv.GetService<IOptions<MqEndpointOptions>>().Value;
				return new RoutingSlipActivityConsumer(metrics,epOpts.ActivityMetrics.ActivityName);
			});

			services.AddSingleton(svcProv =>
			{
				var config = svcProv.GetService<IConfiguration>();
				var conStr = config.GetConnectionString("EfCoreRoutingSlip");
				return new RoutingSlipDbContextFactory(conStr);
			});

			services.AddSingleton<ISagaRepository<RoutingSlipState>,EntityFrameworkSagaRepository<RoutingSlipState>>(svcProv =>
			{
				var ctxFactory = svcProv.GetService<RoutingSlipDbContextFactory>();
				return new EntityFrameworkSagaRepository<RoutingSlipState>(() => ctxFactory.CreateDbContext(Array.Empty<string>()));
			});

			services.AddSingleton(svcProv =>
			{
				var hostOpts = svcProv.GetService<IOptions<RabbitMqHostOptions>>().Value;
				var epOpts = svcProv.GetService<IOptions<MqEndpointOptions>>().Value;
				var machine = svcProv.GetService<RoutingSlipStateMachine>();
				var repository = svcProv.GetService<ISagaRepository<RoutingSlipState>>();

				return Bus.Factory.CreateUsingRabbitMq(cfg =>
				{
					var host = cfg.CreateHost(hostOpts);

					cfg.ReceiveEndpoint(host,epOpts.Metrics.QueueName,e =>
					{
						e.PrefetchCount = epOpts.Metrics.PrefetchCount;
						e.UseRetry(r => r.None());
						//e.LoadFrom(svcProv);
						e.Consumer<RoutingSlipMetricsConsumer>(svcProv);
						e.Consumer<RoutingSlipActivityConsumer>(svcProv);
					});

					cfg.ReceiveEndpoint(host,epOpts.ActivityMetrics.QueueName,e =>
					{
						e.PrefetchCount = epOpts.ActivityMetrics.PrefetchCount;
						e.UseRetry(r => r.None());
					});

					cfg.ReceiveEndpoint(host,epOpts.Saga.QueueName,e =>
					{
						e.UseInMemoryOutbox();
						e.PrefetchCount = 1;
						e.UseConcurrencyLimit(1);
						e.StateMachineSaga(machine,repository);
					});

					cfg.UseSerilog();
				});
			});
		}
	}
}
