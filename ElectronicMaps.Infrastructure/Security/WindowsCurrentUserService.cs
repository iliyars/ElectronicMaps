using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Repositories;
using ElectronicMaps.Domain.Security;


namespace ElectronicMaps.Infrastructure.Security
{
    public class WindowsCurrentUserService : ICurrentUserService
    {
        private readonly IAppUserRepository _userRepo;

        private AppUser? _cachedUser;

        public WindowsCurrentUserService(IAppUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public string? WindowsIdentity => _cachedUser?.WindowsIdentity;

        public string? DisplayName => _cachedUser?.DisplayName;

        public AppRole Role => _cachedUser?.Role ?? AppRole.Viewer;

        public bool IsAuthenticated => _cachedUser != null;

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var name = identity?.Name;

            if(string.IsNullOrWhiteSpace(name))
            {
                _cachedUser = null;
                return;
            }

            var user = await _userRepo.GetByWindowsIdentityAsync(name, ct);

            if(user is null)
            {
                user = new AppUser
                {
                    WindowsIdentity = name,
                    DisplayName = identity.Name ?? name,
                    Role = AppRole.Viewer,
                    IsActive = true
                };

                await _userRepo.AddAsync(user, ct);
                await _userRepo.SaveChangesAsync(ct);
            }

            _cachedUser = user;

        }


    }
}
