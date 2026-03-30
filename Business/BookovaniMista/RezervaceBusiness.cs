using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista.Models;

namespace Business.BookovaniMista
{
    public static class RezervaceBusiness
    {
        public static Task<bool> IsMistoBookedAsync(this IQueryable<Rezervace> rezervace, int mistoId, DateTime date)
        {
            var d = date.Date;
            return rezervace.AnyAsync(r => r.MistoId == mistoId && r.DatumRezervace >= d && r.DatumRezervace < d.AddDays(1));
        }
    }
}