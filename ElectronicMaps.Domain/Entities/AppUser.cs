using ElectronicMaps.Domain.Securirty;
using ElectronicMaps.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Entities
{
    public class AppUser : DomainObject
    {
        public string WindowsIdentity { get; set; } = default!;

        public string DisplayName { get; set; } = default!;
        public AppRole Role { get; set; }

        public bool IsActive { get; set; } = true;


    }
}
