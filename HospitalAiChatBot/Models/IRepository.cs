namespace HospitalAiChatBot.Models;

public interface IRepository<T> where T : class
{
    Task<string> AddAsync(T entity);

    Task<T?> GetByIdAsync(string id);

    Task<IEnumerable<T?>> GetAllAsync();

    Task UpdateAsync(string id, T entity);

    Task DeleteAsync(string id);
}
