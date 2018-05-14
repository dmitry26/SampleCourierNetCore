// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Dmo.MassTransit
{
	public class RabbitMqHostOptions
	{
		public string Username { get; set; }

		public string Password { get; set; }

		public ushort? Heartbeat { get; set; }

		public string Host { get; set; }

		public int? Port { get; set; }

		public string VirtualHost { get; set; }

		public string ClusterMembers { get; set; }

		public string ClusterNodeHostname { get; set; }
	}
}
