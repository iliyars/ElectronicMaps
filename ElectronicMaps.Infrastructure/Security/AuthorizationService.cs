using ElectronicMaps.Domain.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Security
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ICurrentUserService _currentUser;

        public AuthorizationService(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanEditComponents() =>
            _currentUser.Role == AppRole.Editor || _currentUser.Role == AppRole.Admin;

        public bool CanEditParameters() =>
            _currentUser.Role == AppRole.Editor || _currentUser.Role == AppRole.Admin;

        public void EnsureCanEditParameters()
        {
            if (!CanEditParameters())
                throw new UnauthorizedAccessException(
                    $"User '{_currentUser.WindowsIdentity}' is not allowed to edit parameters.");
        }

        public bool IsAdmin() =>
            _currentUser.Role == AppRole.Admin;
    }
}
