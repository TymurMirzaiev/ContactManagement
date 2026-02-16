using ContractManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Data;

public class ContactManagementDbContext : DbContext
{
    public ContactManagementDbContext(DbContextOptions<ContactManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<CustomField> CustomFields { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Contact configuration
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Phone)
                .HasMaxLength(50);

            entity.Property(e => e.CustomFields)
                .IsRequired()
                .HasDefaultValue("{}");

            // Unique index on Email
            entity.HasIndex(e => e.Email)
                .IsUnique();

            // Index on CreatedAt for sorting
            entity.HasIndex(e => e.CreatedAt);
        });

        // CustomField configuration
        modelBuilder.Entity<CustomField>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.DataType)
                .IsRequired()
                .HasMaxLength(20);
        });
    }
}
