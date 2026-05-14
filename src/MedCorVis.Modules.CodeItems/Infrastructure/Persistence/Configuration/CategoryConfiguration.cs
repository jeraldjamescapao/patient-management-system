namespace MedCorVis.Modules.CodeItems.Infrastructure.Persistence.Configuration;

using MedCorVis.Modules.CodeItems.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(Category.CodeMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(Category.DescriptionMaxLength);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(x => x.IsSystemDefined)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsEditable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsDeletable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DeletedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.Property(x => x.DeletedBy)
            .HasMaxLength(100);

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

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_Categories_Code");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");
        
        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_Categories_IsDeleted");
    }
}