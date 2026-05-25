using Microsoft.EntityFrameworkCore;
using PHP.QARAdjustmentTool.API.DTOs;
using PHP.QARAdjustmentTool.API.Models;

namespace PHP.QARAdjustmentTool.API.Test;

public partial class QarContext : DbContext
{
    public QarContext(DbContextOptions<QarContext> options)
        : base(options)
    {
    }

    public virtual DbSet<QaradjustmentToolAvailityUser> QaradjustmentToolAvailityUsers { get; set; }

    public virtual DbSet<QaradjustmentToolPermissionFolder> QaradjustmentToolPermissionFolders { get; set; }

    public virtual DbSet<QaradjustmentToolProviderGroup> QaradjustmentToolProviderGroups { get; set; }

    public virtual DbSet<QaradjustmentToolRole> QaradjustmentToolRoles { get; set; }

    public virtual DbSet<QaradjustmentToolUserRole> QaradjustmentToolUserRoles { get; set; }

    public virtual DbSet<UserWithRolesDto> UserWithRolesDto { get; set; }

    public virtual DbSet<RoleDto> RoleDto { get; set; }

    public DbSet<QARUserProviderGroupMapping> QARUserProviderGroupMapping { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DTO CONFIGURATION

        modelBuilder.Entity<UserWithRolesDto>()
            .HasNoKey()
            .ToView(null);

        modelBuilder.Entity<RoleDto>()
            .HasNoKey()
            .ToView(null);


        // =========================================================
        // QARAdjustmentToolAvailityUsers
        // =========================================================

        modelBuilder.Entity<QaradjustmentToolAvailityUser>(entity =>
        {
            entity.HasKey(e => e.QaradjustmentToolAvailityUsersUserId)
                .HasName("PK__QARAdjus__59D77F515FDFFC88");

            entity.ToTable("QARAdjustmentToolAvailityUsers");

            entity.HasIndex(e => e.AvailityUserId, "UQ_AvilityUserId")
                .IsUnique();

            entity.Property(e => e.QaradjustmentToolAvailityUsersUserId)
                .HasColumnName("QARAdjustmentToolAvailityUsersUserId");

            entity.Property(e => e.AvailityUserId)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime");

            entity.Property(e => e.OrganizationNpi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("OrganizationNPI");

            entity.Property(e => e.OrganizationTaxId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("OrganizationTaxID");

            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.UserFirstName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.UserLastName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });


        // =========================================================
        // QARAdjustmentToolPermissionFolders
        // =========================================================

        modelBuilder.Entity<QaradjustmentToolPermissionFolder>(entity =>
        {
            entity.HasKey(e => e.QaradjustmentToolPermissionFoldersId)
                .HasName("PK__QARAdjus__FE12E9BBA2A662FC");

            entity.ToTable("QARAdjustmentToolPermissionFolders");

            entity.Property(e => e.QaradjustmentToolPermissionFoldersId)
                .HasColumnName("QARAdjustmentToolPermissionFoldersId");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.FolderPath)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.QaradjustmentToolPermissionFolders)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_QARAdjustmentToolPermissionFolders_Roles_RoleId");
        });


        // =========================================================
        // QARAdjustmentToolProviderGroups
        // =========================================================

        modelBuilder.Entity<QaradjustmentToolProviderGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId)
                .HasName("PK__QARAdjus__149AF36AA33A0D05");

            entity.ToTable("QARAdjustmentToolProviderGroups");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.GroupName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime");

            entity.Property(e => e.Tin)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TIN");
        });


        // =========================================================
        // QARAdjustmentToolRoles
        // =========================================================

        modelBuilder.Entity<QaradjustmentToolRole>(entity =>
        {
            entity.HasKey(e => e.RoleId)
                .HasName("PK__QARAdjus__8AFACE1AC00AC8F1");

            entity.ToTable("QARAdjustmentToolRoles");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });


        // =========================================================
        // QARAdjustmentToolUserRoles
        // =========================================================

        modelBuilder.Entity<QaradjustmentToolUserRole>(entity =>
        {
            entity.HasKey(e => e.QaradjustmentToolUserRolesId)
                .HasName("PK__QARAdjus__440FC336ABE87076");

            entity.ToTable("QARAdjustmentToolUserRoles");

            entity.Property(e => e.QaradjustmentToolUserRolesId)
                .HasColumnName("QARAdjustmentToolUserRolesId");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime");

            entity.Property(e => e.QaradjustmentToolAvailityUsersUserId)
                .HasColumnName("QARAdjustmentToolAvailityUsersUserId");

            entity.HasOne(d => d.QaradjustmentToolAvailityUsersUser)
                .WithMany(p => p.QaradjustmentToolUserRoles)
                .HasForeignKey(d => d.QaradjustmentToolAvailityUsersUserId)
                .HasConstraintName("FK_QARAdjustmentToolUserRoles_Users_QARAdjustmentToolAvailityUsersUserId");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.QaradjustmentToolUserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_QARAdjustmentToolUserRoles_Roles_RoleId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}