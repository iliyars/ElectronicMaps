using ElectronicMaps.Domain.Common;


namespace ElectronicMaps.Domain.Entities
{
    public class FormType : DomainObject
    {
       
        public string Code { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";

        public ICollection<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();


    }
}
