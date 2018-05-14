// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SampleCourier.Models;

namespace SampleCourier.TrackingService.Config
{
	public static class AppBuilderExts
	{
		public static void UseEntityFrameworkCore(this IApplicationBuilder app)
		{
			var svcProv = app.ApplicationServices;

			using (var svcScope = svcProv.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var ctxFactory = svcProv.GetService<RoutingSlipDbContextFactory>();
				var dbCtx = ctxFactory.CreateDbContext(Array.Empty<string>());
				dbCtx.Database.EnsureCreated();
			}
		}
	}
}
