using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bintainer.WebApp.Models;

public partial class BintainerContext : DbContext
{
    public BintainerContext()
    {
    }

    public BintainerContext(DbContextOptions<BintainerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Bin> Bins { get; set; }

    public virtual DbSet<BinSubspace> BinSubspaces { get; set; }

    public virtual DbSet<Cabin> Cabins { get; set; }

    public virtual DbSet<CabinLabel> CabinLabels { get; set; }

    public virtual DbSet<Component> Components { get; set; }

    public virtual DbSet<ComponentAttribute> ComponentAttributes { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventorySection> InventorySections { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<PartAttribute> PartAttributes { get; set; }

    public virtual DbSet<PartCategory> PartCategories { get; set; }

    public virtual DbSet<PartFootprint> PartFootprints { get; set; }

    public virtual DbSet<PartGroup> PartGroups { get; set; }

    public virtual DbSet<PartLabel> PartLabels { get; set; }

    public virtual DbSet<PartNumber> PartNumbers { get; set; }

    public virtual DbSet<PartPackage> PartPackages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-FL9KCPH;Initial Catalog=Bintainer;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.Property(e => e.RoleId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Bin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bin__3214EC075A327EE3");

            entity.ToTable("Bin", tb => tb.HasTrigger("CheckBinCoordinates"));

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Section).WithMany(p => p.Bins)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_Bin_InventorySection");
        });

        modelBuilder.Entity<BinSubspace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BinSubsp__3214EC0771DE7263");

            entity.ToTable("BinSubspace");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Capacity).HasDefaultValueSql("((0))");
            entity.Property(e => e.Label)
                .HasMaxLength(100)
                .IsFixedLength();

            entity.HasOne(d => d.Bin).WithMany(p => p.BinSubspaces)
                .HasForeignKey(d => d.BinId)
                .HasConstraintName("FK_BinSubspace_Bin");
        });

        modelBuilder.Entity<Cabin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cabin__3214EC078D91E193");

            entity.ToTable("Cabin", tb => tb.HasTrigger("CheckCabinCoordinates"));

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Section).WithMany(p => p.Cabins)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_Cabin_InventorySection");
        });

        modelBuilder.Entity<CabinLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CabinLab__3214EC07E852C530");

            entity.ToTable("CabinLabel");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Value)
                .HasMaxLength(50)
                .IsFixedLength();

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.CabinLabel)
                .HasForeignKey<CabinLabel>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabinLabel_Cabin");
        });

        modelBuilder.Entity<Component>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Componen__3214EC07322F7F61");

            entity.ToTable("Component");

            entity.Property(e => e.CategoryId)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.Description)
                .HasMaxLength(150)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .IsFixedLength();

            entity.HasOne(d => d.Inventory).WithMany(p => p.Components)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Component_Inventory");

            entity.HasMany(d => d.Cabins).WithMany(p => p.Components)
                .UsingEntity<Dictionary<string, object>>(
                    "ComponentCabin",
                    r => r.HasOne<Cabin>().WithMany()
                        .HasForeignKey("CabinId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ComponentCabin_Cabin"),
                    l => l.HasOne<Component>().WithMany()
                        .HasForeignKey("ComponentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ComponentCabin_Component"),
                    j =>
                    {
                        j.HasKey("ComponentId", "CabinId");
                        j.ToTable("ComponentCabin");
                    });
        });

        modelBuilder.Entity<ComponentAttribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Componen__3214EC0736CA1A04");

            entity.ToTable("ComponentAttribute");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Value)
                .HasMaxLength(150)
                .IsFixedLength();

            entity.HasOne(d => d.Component).WithMany(p => p.ComponentAttributes)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComponentAttribute_Component");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC0767ED7499");

            entity.ToTable("Inventory");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<InventorySection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC073113E3F7");

            entity.ToTable("InventorySection");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.SectionName)
                .HasMaxLength(150)
                .IsFixedLength();

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventorySections)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventorySection_Inventory");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC07374CB885");

            entity.ToTable("Order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.Number)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Qunatity).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Part__3214EC07D8FA538C");

            entity.ToTable("Part");

            entity.Property(e => e.DatasheetUri)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.Description)
                .HasMaxLength(150)
                .IsFixedLength();
            entity.Property(e => e.ImageUri)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .IsFixedLength();

            entity.HasOne(d => d.FootPrintNavigation).WithMany(p => p.Parts)
                .HasForeignKey(d => d.FootPrint)
                .HasConstraintName("FK_Part_PartFootprint");

            entity.HasOne(d => d.PackageNavigation).WithMany(p => p.Parts)
                .HasForeignKey(d => d.Package)
                .HasConstraintName("FK_Part_PartPackage");

            entity.HasMany(d => d.Bins).WithMany(p => p.Parts)
                .UsingEntity<Dictionary<string, object>>(
                    "PartBin",
                    r => r.HasOne<Bin>().WithMany()
                        .HasForeignKey("BinId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PartBin_Bin"),
                    l => l.HasOne<Part>().WithMany()
                        .HasForeignKey("PartId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PartBin_Component"),
                    j =>
                    {
                        j.HasKey("PartId", "BinId");
                        j.ToTable("PartBin");
                    });
        });

        modelBuilder.Entity<PartAttribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartAttr__3214EC074C5195A5");

            entity.ToTable("PartAttribute");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Value)
                .HasMaxLength(150)
                .IsFixedLength();

            entity.HasOne(d => d.Part).WithMany(p => p.PartAttributes)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartAttribute_Part");
        });

        modelBuilder.Entity<PartCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartCate__3214EC07CA329B2B");

            entity.ToTable("PartCategory");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .IsFixedLength();
        });

        modelBuilder.Entity<PartFootprint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartFoot__3214EC0762A00FE3");

            entity.ToTable("PartFootprint");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength();
        });

        modelBuilder.Entity<PartGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartGrou__3214EC07E4E273CD");

            entity.ToTable("PartGroup");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.Part).WithMany(p => p.PartGroups)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartGroup_Part");
        });

        modelBuilder.Entity<PartLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartLabe__3214EC074BE25CCC");

            entity.ToTable("PartLabel");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Value)
                .HasMaxLength(50)
                .IsFixedLength();

            entity.HasOne(d => d.Part).WithMany(p => p.PartLabels)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartLabel_Part");
        });

        modelBuilder.Entity<PartNumber>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartNumb__3214EC07C0AAF8BD");

            entity.ToTable("PartNumber");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Number)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.Supplier)
                .HasMaxLength(100)
                .IsFixedLength();

            entity.HasOne(d => d.Part).WithMany(p => p.PartNumbers)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartNumber_Part");
        });

        modelBuilder.Entity<PartPackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartPack__3214EC07420202FB");

            entity.ToTable("PartPackage");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
