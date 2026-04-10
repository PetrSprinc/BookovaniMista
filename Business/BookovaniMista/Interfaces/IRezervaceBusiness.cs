using Entities.BookovaniMista.Models;

namespace Business.BookovaniMista.Interfaces
{
    public interface IRezervaceBusiness
    {
        Task<(bool Success, string? Error, Rezervace? Rezervace)> RezervovatAsync(RezervaceDto dto, string userIdentifier, CancellationToken cancellationToken = default);
    }
}