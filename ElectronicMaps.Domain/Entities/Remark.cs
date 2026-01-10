using ElectronicMaps.Domain.Common;

namespace ElectronicMaps.Domain.Entities
{
    public class Remark : DomainObject
    {
        public required string Text { get; set; }
        public bool IsActive { get; set; }
        public int Order { get;set; }

        public ICollection<ParameterValueRemark> ParameterValues { get; set; } = new List<ParameterValueRemark>();

    }
}
