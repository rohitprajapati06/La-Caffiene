using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Models;

public partial class LaCaffeineContext : DbContext
{

    public LaCaffeineContext(DbContextOptions<LaCaffeineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Booking");

            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.Mail).HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NoOfPerson).HasColumnName("No of Person");
            entity.Property(e => e.UserId).HasColumnName("User_id");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupon");

            entity.Property(e => e.CouponCode).HasMaxLength(10);
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.ItemName)
                .HasMaxLength(50)
                .HasColumnName("Item_name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.ProductName)
                .HasMaxLength(50)
                .HasColumnName("Product_Name");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Review");

            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.Review1)
                .HasMaxLength(100)
                .HasColumnName("Review");
            entity.Property(e => e.UserId).HasColumnName("User_id");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.EmailId)
                .HasMaxLength(50)
                .HasColumnName("Email_id");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.ProfilePhoto).HasMaxLength(500);
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
