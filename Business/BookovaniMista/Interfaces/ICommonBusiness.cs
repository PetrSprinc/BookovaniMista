using Entities.BookovaniMista.Models;
using System.Security.Claims;
using Entities.BookovaniMista.ViewModels;

namespace Business.BookovaniMista.Interfaces
{
    public interface ICommonBusiness
    {
        Task<Zamestnanec?> GetCurrentZamestnanecAsync(ClaimsPrincipal user);
        Task<VytizenostResult> GetVytizenostAsync(DateTime? odDatum, DateTime? doDatum);
    }
}