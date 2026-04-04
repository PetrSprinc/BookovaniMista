using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;

namespace Business.BookovaniMista
{
    public class RezervaceBusiness : IRezervaceBusiness
    {
        private readonly BookovaniMistaDbContext _db;

        public RezervaceBusiness(BookovaniMistaDbContext db) => _db = db;

        public Task<bool> IsMistoBookedAsync(int mistoId, DateTime date)
        {
            var d = date.Date;
            return _db.Rezervace
                .AnyAsync(r => r.MistoId == mistoId && r.DatumRezervace >= d && r.DatumRezervace < d.AddDays(1));
        }
    }
}