using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence.Configuration
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ComponentFamily> ComponentFamilies => Set<ComponentFamily>();
        public DbSet<Component> Components => Set<Component>();
        public DbSet<FormType> FormTypes => Set<FormType>();
        public DbSet<ParameterDefinition> ParameterDefinitions => Set<ParameterDefinition>();
        public DbSet<ParameterValue> ParameterValues => Set<ParameterValue>();
        public DbSet<AppUser> Users => Set<AppUser>();

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureAppUser(modelBuilder);
            ConfigureComponentFamily(modelBuilder);
            ConfigureComponent(modelBuilder);
            ConfigureFormType(modelBuilder);
            ConfigureParameterDefinition(modelBuilder);
            ConfigureParameterValue(modelBuilder);
            ConfigureRemarks(modelBuilder);
        }

        private void ConfigureRemarks(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Remark>()
                .ToTable("Remarks");

            modelBuilder.Entity<Remark>()
                .HasIndex(x => x.Text)
                .IsUnique();

            // ParameterValueRemark (M:M)
            modelBuilder.Entity<ParameterValueRemark>()
                .ToTable("ParameterValueRemarks");

            modelBuilder.Entity<ParameterValueRemark>()
                .HasKey(x => new {x.ParameterValueId, x.RemarkId});

            modelBuilder.Entity<ParameterValueRemark>()
                .HasOne(x => x.ParameterValue)
                .WithMany(pv => pv.Remarks)
                .HasForeignKey(x => x.ParameterValueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ParameterValueRemark>()
                .HasOne(x => x.Remark)
                .WithMany(r => r.ParameterValues)
                .HasForeignKey(x => x.RemarkId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureAppUser(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<AppUser>();

            b.ToTable("Users");

            b.Property(x => x.WindowsIdentity)
                .IsRequired()
                .HasMaxLength(256);

            b.HasIndex(x => x.WindowsIdentity)
                .IsUnique();

            b.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.Role)
                .IsRequired();

            b.Property(x => x.IsActive)
                .IsRequired();
        }

        private void ConfigureComponentFamily(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<ComponentFamily>();

            b.ToTable("ComponentFamilies");

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            b.HasOne(x => x.FamilyFormType)
                .WithMany()
                .HasForeignKey(x => x.FamilyFormTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.VerificationStatus)
                .HasConversion<int>()
                .IsRequired();

            b.Property(x => x.CreatedAt).IsRequired();

            b.Property(x =>x.Version)
                .IsRequired().HasDefaultValue(1);

            b.HasIndex(x => x.VerificationStatus);
            b.HasIndex(x => x.Name);
        }

        private void ConfigureComponent(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<Component>();

            b.ToTable("Components");

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Свзяь с семейством
            b.HasOne(x => x.ComponentFamily)
                .WithMany(f => f.Components)
                .HasForeignKey(x => x.ComponentFamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            //Связь с фомой компонента
            b.HasOne(x => x.FormType)
                .WithMany()
                .HasForeignKey(x => x.FormTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            //Verification
            b.Property(x => x.VerificationStatus)
                .HasConversion<int>()
                .IsRequired();

            b.Property(x => x.CreatedAt).IsRequired();

            b.Property(x => x.VerificationNote).HasMaxLength(500);

            b.Property(x => x.Version)
                .IsRequired().HasDefaultValue(1);

            b.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.VerifiedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

            // Индексы (без NormalizedName)
            b.HasIndex(x => x.ComponentFamilyId);       // быстро выводить компоненты семейства
            b.HasIndex(x => x.VerificationStatus);      // быстро выводить непроверенные
            b.HasIndex(x => x.FormTypeId);              // если часто фильтруешь по форме

        }

        private void ConfigureFormType(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<FormType>();

            b.ToTable("FormTypes");

            b.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            b.HasIndex(x => x.Code).IsUnique();

            b.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(200);
        }

        private void ConfigureParameterDefinition(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<ParameterDefinition>();

            b.ToTable("ParameterDefinitions");

            b.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.Unit)
                .HasMaxLength(50);

            b.Property(x => x.ValueKind)
                .HasConversion<int>()
                .IsRequired();

            b.Property(x => x.Order)
                .IsRequired();

            //--- Валидация (все nullable) ------

            b.Property(x => x.DataType)
                .HasConversion<int?>()
                .IsRequired(false);

            b.Property(x => x.MinValue)
                .HasColumnType("decimal(18,6)")
                .IsRequired(false);

            b.Property(x => x.MaxValue)
                .HasColumnType("decimal(18,6)")
                .IsRequired(false);

            b.Property(x => x.ValidationPattern)
                .HasMaxLength(500)
                .IsRequired(false);

            b.Property(x => x.ValidationMessage)
                .HasMaxLength(500)
                .IsRequired(false);

            b.Property(x => x.IsRequired)
                .HasDefaultValue(false)
                .IsRequired();

            // ---- Связи ----

            b.HasOne(x => x.FormType)
                .WithMany(f => f.Parameters)
                .HasForeignKey(x => x.FormTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Индексы ----

            b.HasIndex(x => new { x.FormTypeId, x.Code })
                .IsUnique();

            // ---- Быстрый поиск параметров по форме
            b.HasIndex(x => x.FormTypeId);
        }

        private void ConfigureParameterValue(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<ParameterValue>();

            b.ToTable("ParameterValues");

            // ---- ПОЛЯ ----

            b.Property(x => x.StringValue)
                .HasMaxLength(500)
                .IsRequired(false); // Nulable

            b.Property(x => x.IntValue)
                .IsRequired(false); // Nulable

            b.Property(x => x.DoubleValue)
                .HasColumnType("double precision")
                .IsRequired(false); // Nulable

            b.Property(x => x.Pins)
                .HasMaxLength(200)
                .IsRequired(false);

            // ---- Аудит ----

            b.Property(x => x.UpdatedAt)
                .IsRequired(false);

            b.Property(x => x.UpdatedByUserId)
                .IsRequired(false);

            // ---- Связи ----

            b.HasOne(x => x.ParameterDefinition)
                .WithMany()
                .HasForeignKey(x => x.ParameterDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ComponentFamily)
                .WithMany(f => f.ParameterValues)
                .HasForeignKey(x => x.ComponentFamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Component)
                .WithMany(c => c.ParameterValues)
                .HasForeignKey(x => x.ComponentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ---- Constraints ----

            b.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_ParameterValues_Target",
                    "((\"ComponentId\" IS NOT NULL AND \"ComponentFamilyId\" IS NULL) " +
                    "OR (\"ComponentId\" IS NULL AND \"ComponentFamilyId\" IS NOT NULL))");
            });

            // ---- Индексы ----

            b.HasIndex(x => new { x.ComponentId, x.ParameterDefinitionId })
                .IsUnique()
                .HasFilter("\"ComponentId\" IS NOT NULL");

            b.HasIndex(x => new {x.ComponentFamilyId, x.ParameterDefinitionId})
                .IsUnique()
                .HasFilter("\"ComponentFamilyId\" IS NOT NULL");

            b.HasIndex(x => x.ParameterDefinitionId);
        }
    }
}
