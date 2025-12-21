using ElectronicMaps.Domain.Enums;
using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class Component : DomainObject
    {
        public string Name { get; set; } = null!;

        //Проверка
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
        public DateTimeOffset? VerifiedAt { get; set; }
        public int? VerifiedByUserId { get; set; }
        public string? VerificationNote { get; set; }

        //Аудит
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int? CreatedByUserId { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }

        // Конкурентность (универсально для SQLite + SQL Server)
        public int Version { get; set; } = 1;


        public int ComponentFamilyId { get; set; }
        public ComponentFamily ComponentFamily { get; set; } = null!;

        public int FormTypeId { get;set; }
        public FormType? FormType { get; set; }

        public ICollection<ParameterValue> ParameterValues { get; set; } = new List<ParameterValue>();


    }
}
