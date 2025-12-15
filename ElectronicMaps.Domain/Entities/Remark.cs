using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class Remark : DomainObject
    {
        public string Text { get; set; }
        public bool IsActive { get; set; }
        public int Order { get;set; }

        public ICollection<ComponentRemark> Components { get; set; } = new HashSet<ComponentRemark>();
        public ICollection<ComponentFamilyRemark> Families { get; set; } = new HashSet<ComponentFamilyRemark>();
    }

    public class ComponentRemark
    {
        public int ComponentId { get; set; }
        public Component Component { get; set; }
        public int RemarkId { get; set; }
        public Remark Remark { get; set; }
        public int Order { get; set; }
    }

    public class ComponentFamilyRemark
    {
        public int ComponentFamilyId { get; set; }
        public ComponentFamily ComponentFamily { get; set; }
        public int RemarkId { get; set; }
        public Remark Remark { get; set; }
        public int Order { get; set; }
    }
}
