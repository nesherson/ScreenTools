using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ScreenTools.Core;

namespace ScreenTools.Infrastructure;

public class FilePathRepository
{
    private readonly ScreenToolsDbContext _dbContext;
    
    public FilePathRepository(ScreenToolsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(FilePath filePath)
    {
        await _dbContext.FilePaths.AddAsync(filePath);
    }
    
    public void Update(FilePath filePath)
    {
        _dbContext.FilePaths.Update(filePath);
    }
    
    public async Task AddRangeAsync(FilePath[] galleryPaths)
    {
        await _dbContext.FilePaths.AddRangeAsync(galleryPaths);
    }

    public async Task<List<FilePath>> GetAllAsync(string? includes = null)
    {
        var query = _dbContext.FilePaths.AsQueryable();

        if (includes != null)
        {
            query = query.Include(includes);
        }
        
        return await query.ToListAsync();
    }

    public async Task<FilePath> GetByFilePathTypeAbrvAsync(string abrv)
    {
        return await _dbContext.FilePaths
            .Include(fp => fp.FilePathType)
            .FirstOrDefaultAsync(x => x.FilePathType.Abrv == abrv);
    }
    
    public async Task<FilePath?> GetByIdAsync(int id)
    {
        return await _dbContext.FilePaths.FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<int> DeleteAllAsync()
    {
        return await _dbContext.FilePaths.ExecuteDeleteAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var itemToRemove = await _dbContext.FilePaths
            .FirstOrDefaultAsync(x => x.Id == id);
        
        _dbContext.FilePaths.Remove(itemToRemove);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}