using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Dmo.MassTransit
{
	public static class AppBuilderExts
	{
		public static void UseMassTransit(this IApplicationBuilder app)
		{
			// start/stop the bus with the web application
			var appLifetime = (app ?? throw new ArgumentNullException(nameof(app)))
				.ApplicationServices.GetService<IApplicationLifetime>();

			var bus = app.ApplicationServices.GetService<IBusControl>();
			appLifetime.ApplicationStarted.Register(() => bus.Start());
			appLifetime.ApplicationStopped.Register(bus.Stop);
		}
	}
}
