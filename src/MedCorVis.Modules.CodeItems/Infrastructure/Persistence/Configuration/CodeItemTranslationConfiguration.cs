namespace MedCorVis.Modules.CodeItems.Infrastructure.Persistence.Configuration;

using MedCorVis.Modules.CodeItems.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CodeItemTranslationConfiguration : IEntityTypeConfiguration<CodeItemTranslation>
{
    public void Configure(EntityTypeBuilder<CodeItemTranslation> builder)
    {
        builder.ToTable("Translations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(CodeItemTranslation.EntityTypeMaxLength);

        builder.Property(x => x.EntityId)
            .IsRequired();

        builder.Property(x => x.Culture)
            .IsRequired()
            .HasMaxLength(CodeItemTranslation.CultureMaxLength);

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(CodeItemTranslation.LabelMaxLength);
        
        builder.Property(x => x.Description)
            .HasMaxLength(CodeItemTranslation.DescriptionMaxLength);

        builder.Property(x => x.IsSystemDefined)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DeletedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.Property(x => x.DeletedBy)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ModifiedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.Culture })
            .IsUnique()
            .HasDatabaseName("IX_Translations_EntityType_EntityId_Culture");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Translations_IsActive");
        
        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_Translations_IsDeleted");
    }
}