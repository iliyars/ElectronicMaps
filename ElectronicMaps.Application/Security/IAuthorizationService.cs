using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Security
{
    public interface IAuthorizationService
    {
        bool CanEditParameters();
        bool CanEditComponents();
        bool IsAdmin();

        /// <summary>Бросает исключение, если прав недостаточно.</summary>
        void EnsureCanEditParameters();


    }
}
