// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Dmo.MassTransit
{
	public static class MassTransitExts
	{
		public static IRabbitMqHost CreateHost(this IRabbitMqBusFactoryConfigurator cfg,RabbitMqHostOptions opts)
		{
			string host = opts.Host ?? "[::1]";
			ushort port = (ushort)(opts.Port ?? 5672);
			var vhost = string.IsNullOrWhiteSpace(opts.VirtualHost) ? "/" : opts.VirtualHost.Trim('/');

			return cfg.Host(host,port,vhost,null,h =>
			{
				h.Username(opts.Username ?? "guest");
				h.Password(opts.Password ?? "guest");
				h.Heartbeat(opts.Heartbeat ?? 0);

				if (!string.IsNullOrEmpty(opts.ClusterNodeHostname))
				{
					h.UseCluster(cc =>
					{
						cc.Node(opts.ClusterNodeHostname);
						cc.ClusterMembers = opts.ClusterMembers?.Split(',') ?? new string[0];
					});
				}
			});
		}
	}
}
