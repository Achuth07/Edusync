using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Edusync.Data;

public partial class SchoolManagementDbContext : DbContext
{
    public SchoolManagementDbContext(DbContextOptions<SchoolManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Courses__3214EC073EB1098F");

            entity.HasIndex(e => e.Code, "UQ__Courses__A25C5AA718DC24F6").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(5);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Students__3214EC07F7D53E01");

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Teachers__3214EC07872E2880");

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
