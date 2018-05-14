using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SampleCourier.Models
{
	public class RoutingSlipDbContextFactory : IDesignTimeDbContextFactory<RoutingSlipSagaDbContext>
	{
		public RoutingSlipDbContextFactory() { }

		public RoutingSlipDbContextFactory(string conStr)
		{
			_conStr = conStr;
		}

		private string _conStr = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EfCoreRoutingSlip;";

		public RoutingSlipSagaDbContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<RoutingSlipSagaDbContext>();
			builder.UseSqlServer(_conStr);
			return new RoutingSlipSagaDbContext(builder.Options);
		}
	}
}