using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Domain
{
    public class ComponentCreateDto
    {
        /// <summary>
        /// Полное имя компонента (например: "Р1-ОС 125-10").
        /// </summary>
        public string Name { get; init; } = default!;
        /// <summary>
        /// Id семейства — если известно.
        /// В этом случае FamilyName игнорируется.
        /// </summary>
        public int? ComponentFamilyId { get; init; }


        /// <summary>
        /// Код формы для компонента (например: "FORM64" для резистора).
        /// Должен существовать в таблице FormTypes и иметь Scope = Component.
        /// </summary>
        public string FormCode { get; init; } = default!;


        /// <summary>
        /// Название семейства (например: "Р1-ОС"), если Id неизвестен.
        /// Если семейство не существует — будет создано с FamilyFormCode = "FORM4".
        /// </summary>
        public string? FamilyName { get; set; }



        /// <summary>
        /// Список значений параметров компонента.
        /// Код параметра должен совпадать с ParameterDefinition.Code для формы FormCode.
        /// </summary>
        public List<ParameterDto> Parameters { get; set; } = new();


    }
}
