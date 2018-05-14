using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace SampleCourier.Models
{
	public class RoutingSlipSagaDbContext : SagaDbContext<RoutingSlipState,RoutingSlipStateSagaMap>
	{
		public RoutingSlipSagaDbContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}
