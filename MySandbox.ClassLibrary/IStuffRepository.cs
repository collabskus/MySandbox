namespace MySandbox.ClassLibrary;

// Repository interface
public interface IStuffRepository
{
    Task AddAsync(string item);
    Task<IEnumerable<StuffItem>> GetAllAsync();
    Task<StuffItem?> GetByIdAsync(int id);
    Task<bool> RemoveAsync(int id);
}

