using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HelloWorld.EFModels
{
    public partial class HelloWorldContext : DbContext
    {
        public HelloWorldContext()
        {
        }

        public HelloWorldContext(DbContextOptions<HelloWorldContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Source> Sources { get; set; } = null!;
        public virtual DbSet<Tenant> Tenants { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer(GlobalSettings.DatabaseConnection);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Source>(entity =>
            {
                entity.Property(e => e.SourceId).ValueGeneratedNever();

                entity.Property(e => e.SourceCategory).HasMaxLength(200);

                entity.Property(e => e.SourceName).HasMaxLength(200);

                entity.Property(e => e.SourceType).HasMaxLength(200);

                entity.HasOne(d => d.Tenant)
                    .WithMany(p => p.Sources)
                    .HasForeignKey(d => d.TenantId)
                    .HasConstraintName("FK_Sources_Tenants");
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.Property(e => e.TenantId).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.TenantCode).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
