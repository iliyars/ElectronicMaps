
using ElectronicMaps.Domain.ValueObjects;

namespace ElectronicMaps.Domain.Services
{
    /// <summary>
    /// Provide read-only queries related to component forms and their parameterds
    /// </summary>
    public interface IFormQueryService
    {
        /// <summary>
        /// Retirives a parameter form for the specified component.
        /// The result contains all parameters (as DTOs).
        /// </summary>
        /// <param name="componentName">The logical name of the component to load the form for.</param>
        /// <param name="ct">Cancellation token for the asynchronius operaion.</param>
        /// <returns>A read-only list of parameter DTOs describing the component.</returns>
        Task<IReadOnlyList<Parameter>> GetComponentFormAsync(
            string componentName,
            CancellationToken ct = default);

        Task<bool> ComponentExistsAsync(string componentName, CancellationToken ct = default);
    }
}
