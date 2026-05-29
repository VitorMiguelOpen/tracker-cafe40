using CafeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeTracker.Infrastructure.Persistence.Configurations;

/// <summary>Mapeia <see cref="CoffeeSession"/> para a tabela `coffee_session`.</summary>
public sealed class CoffeeSessionConfiguration : IEntityTypeConfiguration<CoffeeSession>
{
    public void Configure(EntityTypeBuilder<CoffeeSession> builder)
    {
        builder.ToTable("coffee_session");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(s => s.MachineCode)
            .HasColumnName("machine_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(s => s.EndedAt)
            .HasColumnName("ended_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(s => s.DurationSeconds).HasColumnName("duration_seconds");

        builder.Property(s => s.IsOpen).HasColumnName("is_open").IsRequired();

        // Acelera contagem/agrupamento de sessões por período (US-03..US-07).
        builder.HasIndex(s => new { s.MachineCode, s.StartedAt })
            .HasDatabaseName("ix_coffee_session_machine_start");

        // Acelera achar a sessão aberta (no máximo uma por máquina).
        builder.HasIndex(s => new { s.MachineCode, s.IsOpen })
            .HasDatabaseName("ix_coffee_session_open");
    }
}
