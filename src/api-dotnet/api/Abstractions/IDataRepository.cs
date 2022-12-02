namespace TM.PoC.API.Abstractions;

public interface IDataRepository<T> where T : class, new()
{
    Task<List<T>> GetAsync();

    Task<T> GetByIdAsync(string? bsonId);

    Task CreateAsync(T t);

    Task DeleteAsync(string bsonId);
}