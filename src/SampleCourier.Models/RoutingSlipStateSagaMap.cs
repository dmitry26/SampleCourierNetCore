using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleCourier.Models
{
	public class RoutingSlipStateSagaMap : IEntityTypeConfiguration<RoutingSlipState>
	{
		public void Configure(EntityTypeBuilder<RoutingSlipState> builder)
		{
			builder.Property(x => x.State);
			builder.Property(x => x.CreateTime);
			builder.Property(x => x.StartTime);
			builder.Property(x => x.EndTime);
			builder.Property(x => x.Duration);

			builder.ToTable("EfCoreRoutingSlipState");
		}
	}
}