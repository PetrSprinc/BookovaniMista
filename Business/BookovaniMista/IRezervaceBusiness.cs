namespace Business.BookovaniMista
{
    public interface IRezervaceBusiness
    {
        Task<bool> IsMistoBookedAsync(int mistoId, DateTime date);
    }
}