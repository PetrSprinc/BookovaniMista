using Entities.BookovaniMista.Models;

namespace Business.BookovaniMista.Interfaces
{
    public interface IRezervaceBusiness
    {
        Task<BookingResultDto> RezervovatAsync(RezervaceDto dto, string userIdentifier, CancellationToken cancellationToken = default);
    }
}
