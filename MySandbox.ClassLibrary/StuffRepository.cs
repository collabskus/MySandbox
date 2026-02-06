using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MySandbox.ClassLibrary;

// Repository implementation
public class StuffRepository(ILogger<StuffRepository> logger, StuffDbContext dbContext) : IStuffRepository
{
    private readonly ILogger<StuffRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly StuffDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task AddAsync(string item)
    {
        // Input validation - OWASP best practice
        if (string.IsNullOrWhiteSpace(item))
        {
            _logger.LogWarning("Attempted to add null or empty item");
            throw new ArgumentException("Item name cannot be null or empty", nameof(item));
        }

        // Length validation - prevent DoS attacks
        if (item.Length > 500)
        {
            _logger.LogWarning("Attempted to add item exceeding maximum length");
            throw new ArgumentException("Item name cannot exceed 500 characters", nameof(item));
        }

        var stuffItem = new StuffItem
        {
            Name = item, // EF Core uses parameterized queries - SQL injection safe
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.StuffItems.AddAsync(stuffItem);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Added item: {Item} with ID: {Id}", item, stuffItem.Id);
    }

    public async Task<IEnumerable<StuffItem>> GetAllAsync()
    {
        _logger.LogInformation("Getting all items");
        // EF Core uses parameterized queries - SQL injection safe
        return await _dbContext.StuffItems
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<StuffItem?> GetByIdAsync(int id)
    {
        // Input validation
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID requested: {Id}", id);
            return null;
        }

        // EF Core uses parameterized queries - SQL injection safe
        var item = await _dbContext.StuffItems.FindAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Item with id {Id} not found", id);
        }

        return item;
    }

    public async Task<bool> RemoveAsync(int id)
    {
        // Input validation
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID for removal: {Id}", id);
            return false;
        }

        var item = await _dbContext.StuffItems.FindAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Cannot remove - item with id {Id} not found", id);
            return false;
        }

        _dbContext.StuffItems.Remove(item);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Removed item: {Item} with ID: {Id}", item.Name, item.Id);
        return true;
    }
}

