using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure
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

            SeedInitialForms(modelBuilder); // сюда добавим три формы (Resistor, Chip, 64)
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

            b.Property(x => x.FamilyFormCode)
                .HasMaxLength(50);
        }

        private void ConfigureComponent(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<Component>();

            b.ToTable("Components");

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.FormCode)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(x => x.CanonicalName)
                .HasMaxLength(200);

            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.CanonicalName);

            b.HasOne(x => x.ComponentFamily)
                .WithMany(f => f.Components)
                .HasForeignKey(x => x.ComponentFamilyId)
                .OnDelete(DeleteBehavior.Cascade);
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

            b.Property(x => x.TemplateKey)
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

            b.Property(x => x.Group)
                .HasMaxLength(100);

            b.HasOne(x => x.FormType)
                .WithMany(f => f.Parameters)
                .HasForeignKey(x => x.FormTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureParameterValue(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<ParameterValue>();

            b.ToTable("ParameterValues");

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
        }


        private static void SeedInitialForms(ModelBuilder modelBuilder)
        {
            const int formResistorId = 1;
            const int formChipId = 2;
            const int form64Id = 3;

            modelBuilder.Entity<FormType>().HasData(
                new FormType { Id = formResistorId, Code = "FORM_RESISTOR", DisplayName = "Форма резистора", Scope = FormScope.Component },
                new FormType { Id = formChipId, Code = "FORM_CHIP", DisplayName = "Форма микросхемы", Scope = FormScope.Component },
                new FormType { Id = form64Id, Code = "FORM_64", DisplayName = "Форма 64", Scope = FormScope.Component }
            );

            var defs = new List<ParameterDefinition>();
            var id = 1;

            // ========= ФОРМА РЕЗИСТОРА =========
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "DcVoltage", DisplayName = "Постоянное напряжение", Unit = "В", ValueKind = ParameterValueKind.Double, Order = 1 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "AcVoltage", DisplayName = "Переменное напряжение", Unit = "В", ValueKind = ParameterValueKind.Double, Order = 2 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "ImpulseVoltage", DisplayName = "Импульсное напряжение", Unit = "В", ValueKind = ParameterValueKind.Double, Order = 3 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "SumVoltage", DisplayName = "Суммарное напряжение", Unit = "В", ValueKind = ParameterValueKind.Double, Order = 4 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "Frequancy", DisplayName = "Частота", Unit = "Гц", ValueKind = ParameterValueKind.Int, Order = 5 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "ImpulseDuration", DisplayName = "Длительность импульса", Unit = null, ValueKind = ParameterValueKind.Double, Order = 6 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "ImpulsePower", DisplayName = "Импульсная мощность", Unit = null, ValueKind = ParameterValueKind.Double, Order = 7 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "MeanPower", DisplayName = "Средняя мощность", Unit = null, ValueKind = ParameterValueKind.Double, Order = 8 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "LoadKoeffImpulse", DisplayName = "Коэффициент нагрузки (импульс)", Unit = null, ValueKind = ParameterValueKind.Double, Order = 9 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "CurrentMovingContact", DisplayName = "Ток через подвижный контакт", Unit = "А", ValueKind = ParameterValueKind.Double, Order = 10 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "AmbientTemperature", DisplayName = "Температура окружающей среды", Unit = "°C", ValueKind = ParameterValueKind.Int, Order = 11 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "SuperHeatTemperature", DisplayName = "Температура перегрева", Unit = "°C", ValueKind = ParameterValueKind.Double, Order = 12 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "SumPower", DisplayName = "Суммарная мощность", Unit = null, ValueKind = ParameterValueKind.Double, Order = 13 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "AmbientTemperatureCase", DisplayName = "Температура корпуса", Unit = "°C", ValueKind = ParameterValueKind.Int, Order = 14 });
            defs.Add(new ParameterDefinition { Id = id++, FormTypeId = formResistorId, Code = "LoadKoeff", DisplayName = "Коэффициент нагрузки", Unit = null, ValueKind = ParameterValueKind.String, Order = 15 });

            // ========= ВСПОМОГАТЕЛЬНАЯ ФУНКЦИЯ ДЛЯ WithPins =========
            void AddWithPinsParam(int formId, string code, string displayName, string? unit, int order)
            {
                defs.Add(new ParameterDefinition
                {
                    Id = id++,
                    FormTypeId = formId,
                    Code = code,
                    DisplayName = displayName,
                    Unit = unit,
                    ValueKind = ParameterValueKind.WithPins,
                    Order = order
                });
            }

            // ========= ФОРМА CHIP =========
            AddWithPinsParam(formChipId, "SupplayVoltage", "Напряжение питания", "В", 1);
            AddWithPinsParam(formChipId, "SupplyOrder", "Порядок подачи напряжения питания", null, 2);
            AddWithPinsParam(formChipId, "LowLevelVolatge", "Напряжение низкого уровня", "В", 3);
            AddWithPinsParam(formChipId, "HighLevelVolatge", "Напряжение высокого уровня", "В", 4);
            AddWithPinsParam(formChipId, "ImpulseDuration", "Длительность импульса", "нс", 5);
            AddWithPinsParam(formChipId, "TurnOnTrasnsition", "Время перехода при включении", "нс", 6);
            AddWithPinsParam(formChipId, "TurnOffTransition", "Время перехода при выключении", "нс", 7);
            AddWithPinsParam(formChipId, "Frequency", "Частота", "МГц", 8);
            AddWithPinsParam(formChipId, "Timet1", "Время t1", "нс", 9);
            AddWithPinsParam(formChipId, "Timet2", "Время t2", "нс", 10);
            AddWithPinsParam(formChipId, "OutCurrentLowLevel", "Выходной ток низкого уровня", "мА", 11);
            AddWithPinsParam(formChipId, "OutCurrentHighLevel", "Выходной ток высокого уровня", "мА", 12);
            AddWithPinsParam(formChipId, "CapacityLoad", "Ёмкость нагрузки", "пФ", 13);
            AddWithPinsParam(formChipId, "PowerDissipation", "Мощность рассеивания", "мВт", 14);
            AddWithPinsParam(formChipId, "PosName", "Позиционное обозначение и номера выводов", null, 15);

            defs.Add(new ParameterDefinition
            {
                Id = id++,
                FormTypeId = formChipId,
                Code = "AmbientTemperatureCase",
                DisplayName = "Температура корпуса",
                Unit = "°C",
                ValueKind = ParameterValueKind.Int,
                Order = 16
            });

            defs.Add(new ParameterDefinition
            {
                Id = id++,
                FormTypeId = formChipId,
                Code = "LoadKoeff",
                DisplayName = "Коэффициент нагрузки",
                Unit = null,
                ValueKind = ParameterValueKind.String,
                Order = 17
            });

            // ========= ФОРМА 64 (все параметры WithPins) =========
            AddWithPinsParam(form64Id, "SupplayVoltage", "Напряжение питания", "В", 1);
            AddWithPinsParam(form64Id, "SupplyOrder", "Порядок подачи напряжения питания", null, 2);
            AddWithPinsParam(form64Id, "LowLevelVolatge", "Напряжение низкого уровня", "В", 3);
            AddWithPinsParam(form64Id, "HighLevelVolatge", "Напряжение высокого уровня", "В", 4);
            AddWithPinsParam(form64Id, "ImpulseDuration", "Длительность импульса", "нс", 5);
            AddWithPinsParam(form64Id, "TurnOnTrasnsition", "Время перехода при включении", "нс", 6);
            AddWithPinsParam(form64Id, "TurnOffTransition", "Время перехода при выключении", "нс", 7);
            AddWithPinsParam(form64Id, "Frequency", "Частота", "МГц", 8);
            AddWithPinsParam(form64Id, "Timet1", "Время t1", "нс", 9);
            AddWithPinsParam(form64Id, "Timet2", "Время t2", "нс", 10);
            AddWithPinsParam(form64Id, "OutCurrentLowLevel", "Выходной ток низкого уровня", "мА", 11);
            AddWithPinsParam(form64Id, "OutCurrentHighLevel", "Выходной ток высокого уровня", "мА", 12);
            AddWithPinsParam(form64Id, "CapacityLoad", "Ёмкость нагрузки", "пФ", 13);
            AddWithPinsParam(form64Id, "PowerDissipation", "Мощность рассеивания", "мВт", 14);
            AddWithPinsParam(form64Id, "AmbientTemperatureCase", "Температура корпуса", "°C", 15);
            AddWithPinsParam(form64Id, "PosName", "Позиционное обозначение и номера выводов", null, 16);
            AddWithPinsParam(form64Id, "LoadKoeff", "Коэффициент нагрузки", null, 17);

            modelBuilder.Entity<ParameterDefinition>().HasData(defs);
        }


    }
}
