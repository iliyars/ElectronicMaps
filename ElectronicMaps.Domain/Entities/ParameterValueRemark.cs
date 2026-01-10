using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Domain.Entities
{
    public class ParameterValueRemark
    {
        public int ParameterValueId { get; set; }
        public ParameterValue ParameterValue { get; set; } = null!;

        public int RemarkId { get; set; }
        public Remark Remark { get; set; } = null!;
    }
}
