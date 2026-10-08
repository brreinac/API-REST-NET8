using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Osigu.MedicalOrders.Domain.Entities;

namespace Osigu.MedicalOrders.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.PatientId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(order => order.PatientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(order => order.ServiceCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(order => order.ServiceDescription)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(order => order.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.CreatedAt)
            .IsRequired();

        builder.HasIndex(order => order.PatientId);
        builder.HasIndex(order => order.Status);
        builder.HasIndex(order => order.CreatedAt);
    }
}
