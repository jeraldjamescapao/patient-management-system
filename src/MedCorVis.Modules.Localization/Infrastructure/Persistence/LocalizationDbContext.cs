namespace MedCorVis.Modules.Localization.Infrastructure.Persistence;

using MedCorVis.Modules.Localization.Domain;
using Microsoft.EntityFrameworkCore;

internal sealed class LocalizationDbContext : DbContext
{
    public DbSet<Translation> Translations => Set<Translation>();

    public LocalizationDbContext(DbContextOptions<LocalizationDbContext> options)
        : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("Localization");

        builder.Entity<Translation>(entity =>
        {
            entity.ToTable("Translations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .UseIdentityColumn();

            entity.Property(x => x.Culture)
                .IsRequired()
                .HasMaxLength(Translation.CultureMaxLength);

            entity.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(Translation.KeyMaxLength);

            entity.Property(x => x.Value)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.Description)
                .HasMaxLength(Translation.DescriptionMaxLength);

            entity.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.Property(x => x.IsSystemDefined)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            entity.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.ModifiedAtUtc)
                .HasColumnType("datetimeoffset");

            entity.Property(x => x.ModifiedBy)
                .HasMaxLength(100);

            entity.HasIndex(x => x.Culture)
                .HasDatabaseName("IX_Translations_Culture");

            entity.HasIndex(x => new { x.Culture, x.Key })
                .IsUnique()
                .HasDatabaseName("IX_Translations_Culture_Key");

            entity.HasIndex(x => x.IsActive)
                .HasDatabaseName("IX_Translations_IsActive");
        });
    }
}