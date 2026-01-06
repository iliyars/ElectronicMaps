using ElectronicMaps.Domain.Common;
using ElectronicMaps.Domain.Security;

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
