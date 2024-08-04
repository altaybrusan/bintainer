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

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventorySection> InventorySections { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<PartAttribute> PartAttributes { get; set; }

    public virtual DbSet<PartAttributeTemplate> PartAttributeTemplates { get; set; }

    public virtual DbSet<PartCategory> PartCategories { get; set; }

    public virtual DbSet<PartFootprint> PartFootprints { get; set; }

    public virtual DbSet<PartGroup> PartGroups { get; set; }

    public virtual DbSet<PartLabel> PartLabels { get; set; }

    public virtual DbSet<PartPackage> PartPackages { get; set; }

    public virtual DbSet<PartTemplate> PartTemplates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-FL9KCPH;Initial Catalog=Bintainer;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

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
            entity.HasKey(e => e.Id).HasName("PK__Bin__3214EC07B0D1E1DB");

            entity.ToTable("Bin", tb => tb.HasTrigger("CheckBinCoordinates"));

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Section).WithMany(p => p.Bins)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_Bin_InventorySection");
        });

        modelBuilder.Entity<BinSubspace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BinSubsp__3214EC076D580789");

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

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("Inventory");

            entity.Property(e => e.Admin).HasMaxLength(256);
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsFixedLength();
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventory_AspNetUsers");
        });

        modelBuilder.Entity<InventorySection>(entity =>
        {
            entity.ToTable("InventorySection");

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
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC07AC2FBBF7");

            entity.ToTable("Order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.Number)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Qunatity).HasDefaultValueSql("((0))");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_AspNetUsers");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Part__3214EC07C7C41940");

            entity.ToTable("Part");

            entity.HasIndex(e => e.FootPrint, "IX_Part_FootPrintId").IsUnique();

            entity.Property(e => e.Description)
                .HasMaxLength(150)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .IsFixedLength();
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Category).WithMany(p => p.Parts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Part_PartCategory");

            entity.HasOne(d => d.FootPrintNavigation).WithOne(p => p.Part)
                .HasForeignKey<Part>(d => d.FootPrint)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Part_PartFootprint");

            entity.HasOne(d => d.User).WithMany(p => p.Parts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Part_AspNetUsers");

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

            entity.HasMany(d => d.Groups).WithMany(p => p.Parts)
                .UsingEntity<Dictionary<string, object>>(
                    "PartGroupAssociation",
                    r => r.HasOne<PartGroup>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Part_PartGroup_PartGroup"),
                    l => l.HasOne<Part>().WithMany()
                        .HasForeignKey("PartId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Part_PartGroup_Part"),
                    j =>
                    {
                        j.HasKey("PartId", "GroupId").HasName("PK_Part_PartGroup");
                        j.ToTable("PartGroupAssociation");
                    });

            entity.HasMany(d => d.Templates).WithMany(p => p.Parts)
                .UsingEntity<Dictionary<string, object>>(
                    "PartTemplateAssignment",
                    r => r.HasOne<PartTemplate>().WithMany()
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PartTempl__Templ__02FC7413"),
                    l => l.HasOne<Part>().WithMany()
                        .HasForeignKey("PartId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PartTempl__PartI__02084FDA"),
                    j =>
                    {
                        j.HasKey("PartId", "TemplateId").HasName("PK__PartTemp__13B8A0823629CDE5");
                        j.ToTable("PartTemplateAssignment");
                    });
        });

        modelBuilder.Entity<PartAttribute>(entity =>
        {
            entity.ToTable("PartAttribute");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Value)
                .HasMaxLength(150)
                .IsFixedLength();

            entity.HasOne(d => d.Template).WithMany(p => p.PartAttributes)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartAttribute_PartAttributeTemplate");
        });

        modelBuilder.Entity<PartAttributeTemplate>(entity =>
        {
            entity.ToTable("PartAttributeTemplate");

            entity.Property(e => e.TemplateName)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.PartAttributeTemplates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartAttributeTemplate_AspNetUsers");
        });

        modelBuilder.Entity<PartCategory>(entity =>
        {
            entity.ToTable("PartCategory");

            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .IsFixedLength();
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_PartCategory_ParentCategory");

            entity.HasOne(d => d.User).WithMany(p => p.PartCategories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartCategory_AspNetUsers");
        });

        modelBuilder.Entity<PartFootprint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC0797482D85");

            entity.ToTable("PartFootprint");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FullFileName).HasMaxLength(250);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.Url).HasMaxLength(250);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.PartFootprints)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartFootprint_AspNetUsers");
        });

        modelBuilder.Entity<PartGroup>(entity =>
        {
            entity.ToTable("PartGroup");

            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsFixedLength();
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.PartGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartGroup_AspNetUsers");
        });

        modelBuilder.Entity<PartLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartLabe__3214EC071C479CF5");

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

        modelBuilder.Entity<PartPackage>(entity =>
        {
            entity.ToTable("PartPackage");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength();
        });

        modelBuilder.Entity<PartTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartTemp__3214EC073F3831C2");

            entity.ToTable("PartTemplate");

            entity.Property(e => e.DatasheetUri)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.ImageUri)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.PartNumber)
                .HasMaxLength(150)
                .IsFixedLength();
            entity.Property(e => e.Supplier)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.PartTemplates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartTemplate_AspNetUsers");

            entity.HasMany(d => d.AttributeTemplates).WithMany(p => p.Templates)
                .UsingEntity<Dictionary<string, object>>(
                    "PartTemplateAttributeAssociation",
                    r => r.HasOne<PartAttributeTemplate>().WithMany()
                        .HasForeignKey("AttributeTemplateId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PartTemplateAttributeAssociation_PartAttributeTemplate"),
                    l => l.HasOne<PartTemplate>().WithMany()
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PartTemplateAttributeAssociation_PartTemplate"),
                    j =>
                    {
                        j.HasKey("TemplateId", "AttributeTemplateId").HasName("PK__PartTemp__B9F28FD89AAD0904");
                        j.ToTable("PartTemplateAttributeAssociation");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
