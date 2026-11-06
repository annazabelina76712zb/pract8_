using Microsoft.EntityFrameworkCore;
using pract_8.Models;

namespace pract_8.Models;

public partial class Pract8Context : DbContext
{
    public Pract8Context() { }

    public Pract8Context(DbContextOptions<Pract8Context> options)
        : base(options) { }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Task> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=sql.ects;Database=pract_8;User Id=student_00;Password=student_00;TrustServerCertificate=True;Encrypt=False");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Users");
            entity.HasIndex(e => e.Login, "UQ_Users_Login").IsUnique();
            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Tasks");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.OwnerLogin).HasMaxLength(100);
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}