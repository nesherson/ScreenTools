using Microsoft.EntityFrameworkCore;
using ScreenTools.Core;

namespace ScreenTools.Infrastructure;

public class FilePathTypeRepository
{
    private readonly ScreenToolsDbContext _dbContext;
    
    public FilePathTypeRepository(ScreenToolsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<FilePathType> FindByAbrv(string abrv)
    {
        return await _dbContext.FilePathTypes
            .FirstOrDefaultAsync(x => x.Abrv == abrv);
    }

    public async Task<List<FilePathType>> GetAll()
    {
        return await _dbContext.FilePathTypes
            .ToListAsync();
    }
}