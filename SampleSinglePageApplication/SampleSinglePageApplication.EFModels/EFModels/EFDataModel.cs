﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SampleSinglePageApplication.EFModels.EFModels;

public partial class EFDataModel : DbContext
{
    public EFDataModel()
    {
    }

    public EFDataModel(DbContextOptions<EFDataModel> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentGroup> DepartmentGroups { get; set; }

    public virtual DbSet<FileStorage> FileStorages { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<UDFLabel> UDFLabels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<UserInGroup> UserInGroups { get; set; }

    // this is required for doing the transcriber. Can be removed before production
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(local);Database=sspa_main;Trusted_Connection=True;TrustServerCertificate=True");

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
    // => optionsBuilder.UseSqlServer("Data Source=(local);Initial Catalog=SampleSinglePageApplication;Persist Security Info=True;User ID=sa;Password=saPassword;MultipleActiveResultSets=True;TrustServerCertificate=True;");
    // => optionsBuilder.UseMySQL("Server=localhost;Database=SampleSinglePageApplication;User=sa;Password=saPassword");
    // => optionsBuilder.UseNpgsql("Host=localhost;Database=SampleSinglePageApplication;Username=postgres;Password=saPassword");
    // => optionsBuilder.UseSqlite("Data Source=C:\\Working\\Development\\SampleSinglePageApplication.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.DepartmentId).ValueGeneratedNever();
            entity.Property(e => e.ActiveDirectoryNames).HasMaxLength(100);
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
        });

        modelBuilder.Entity<DepartmentGroup>(entity =>
        {
            entity.Property(e => e.DepartmentGroupId).ValueGeneratedNever();
            entity.Property(e => e.DepartmentGroupName).HasMaxLength(200);
        });

        modelBuilder.Entity<FileStorage>(entity =>
        {
            entity.HasKey(e => e.FileId);

            entity.ToTable("FileStorage");

            entity.Property(e => e.FileId).ValueGeneratedNever();
            entity.Property(e => e.Extension).HasMaxLength(15);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.SourceFileId).HasMaxLength(100);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.FileStorages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("IX_FileStorage_UserId");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK_Settings_1");

            entity.Property(e => e.SettingName).HasMaxLength(100);
            entity.Property(e => e.SettingType).HasMaxLength(100);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(e => e.TenantId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.TenantCode).HasMaxLength(50);
        });

        modelBuilder.Entity<UDFLabel>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Module).HasMaxLength(20);
            entity.Property(e => e.UDF).HasMaxLength(10);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastLockoutDate).HasColumnType("datetime");
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UDF01).HasMaxLength(500);
            entity.Property(e => e.UDF02).HasMaxLength(500);
            entity.Property(e => e.UDF03).HasMaxLength(500);
            entity.Property(e => e.UDF04).HasMaxLength(500);
            entity.Property(e => e.UDF05).HasMaxLength(500);
            entity.Property(e => e.UDF06).HasMaxLength(500);
            entity.Property(e => e.UDF07).HasMaxLength(500);
            entity.Property(e => e.UDF08).HasMaxLength(500);
            entity.Property(e => e.UDF09).HasMaxLength(500);
            entity.Property(e => e.UDF10).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("IX_Users_DepartmentId");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("IX_Users_TenantId");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId);

            entity.Property(e => e.GroupId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<UserInGroup>(entity =>
        {
            entity.Property(e => e.UserInGroupId).ValueGeneratedNever();

            entity.HasOne(d => d.Group).WithMany(p => p.UserInGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserInGroups_UserGroups");

            entity.HasOne(d => d.User).WithMany(p => p.UserInGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserInGroups_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
