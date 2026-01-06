

namespace ElectronicMaps.Domain.Security
{
    public interface ICurrentUserService
    {
        /// <summary>Windows identity: DOMAIN\user</summary>
        string? WindowsIdentity { get; }

        /// <summary>Имя пользователя из БД, если найден.</summary>
        string? DisplayName { get; }

        /// <summary>Роль в приложении.</summary>
        AppRole Role { get; }

        bool IsAuthenticated { get; }      // наш пользователь найден в БД
    }
}
