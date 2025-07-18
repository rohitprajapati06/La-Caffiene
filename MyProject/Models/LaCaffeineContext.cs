using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Models;

public partial class LaCaffeineContext : DbContext
{
    public LaCaffeineContext()
    {
    }

    public LaCaffeineContext(DbContextOptions<LaCaffeineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Product> Products { get; set; }

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

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.OrderItemId);

            entity.ToTable("Cart");

            entity.Property(e => e.OrderItemId).ValueGeneratedNever();
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.ProductName).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.Carts)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cart_Product");
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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFCE9805B0");

            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__UserId__76619304");
        });

<<<<<<< HEAD
=======
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06817BB5C408");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Order__793DFFAF");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Produ__7A3223E8");
        });

>>>>>>> cart-order-backup
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

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.EmailId)
                .HasMaxLength(50)
                .HasColumnName("Email_id");
            entity.Property(e => e.FirstName).HasMaxLength(25);
            entity.Property(e => e.LastName).HasMaxLength(25);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.ProfilePhoto).HasMaxLength(500);
            entity.Property(e => e.Providers).HasMaxLength(50);
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
