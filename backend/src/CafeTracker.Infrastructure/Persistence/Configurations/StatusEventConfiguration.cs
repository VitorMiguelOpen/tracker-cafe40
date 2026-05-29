using CafeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeTracker.Infrastructure.Persistence.Configurations;

/// <summary>Mapeia <see cref="StatusEvent"/> para a tabela `status_event`.</summary>
public sealed class StatusEventConfiguration : IEntityTypeConfiguration<StatusEvent>
{
    public void Configure(EntityTypeBuilder<StatusEvent> builder)
    {
        builder.ToTable("status_event");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.MachineCode)
            .HasColumnName("machine_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.Tag).HasColumnName("tag").IsRequired();

        builder.Property(e => e.Value).HasColumnName("value").IsRequired();

        builder.Property(e => e.EventTime)
            .HasColumnName("event_time")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.RawPayload)
            .HasColumnName("raw_payload")
            .HasMaxLength(255);

        // `State` é derivado de `Value` — não é coluna no banco.
        builder.Ignore(e => e.State);

        // Acelera "último evento da máquina" e consultas por período.
        builder.HasIndex(e => new { e.MachineCode, e.EventTime })
            .HasDatabaseName("ix_status_event_machine_time");
    }
}
