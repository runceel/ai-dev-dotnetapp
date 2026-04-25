using EventRegistration.Registrations.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Registrations.Infrastructure.Persistence;

/// <summary>
/// Registrations モジュールの DbContext。
/// </summary>
public sealed class RegistrationsDbContext(DbContextOptions<RegistrationsDbContext> options) : DbContext(options)
{
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.EventId).IsRequired();
            entity.Property(r => r.ParticipantName).IsRequired();
            entity.Property(r => r.Email).IsRequired();
            entity.Property(r => r.Status).IsRequired();
            entity.Property(r => r.RegisteredAt).IsRequired();

            // Cancelled 以外の有効登録に対する EventId + Email のユニーク制約。
            // フィルター条件の "2" は RegistrationStatus.Cancelled の enum 値。
            // NOTE: EF Core InMemory プロバイダーではフィルター付きインデックスは実効しないため、
            //       アプリケーション層 (RegisterParticipantUseCase.HasActiveRegistrationAsync) の
            //       事前チェックが主要な防御ライン。本定義は SQL Server 等の本番 DB 移行時の保護層。
            entity.HasIndex(r => new { r.EventId, r.Email })
                .HasFilter("[Status] <> 2")
                .IsUnique();
        });
    }
}
