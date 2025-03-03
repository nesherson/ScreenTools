using Microsoft.EntityFrameworkCore;
using ScreenTools.Core;

namespace ScreenTools.Infrastructure;

public class GalleryPathRepository
{
    private readonly ScreenToolsDbContext _dbContext;
    
    public GalleryPathRepository(ScreenToolsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(GalleryPath galleryPath)
    {
        await _dbContext.GalleryPaths.AddAsync(galleryPath);
    }
    
    public async Task AddRangeAsync(GalleryPath[] galleryPaths)
    {
        await _dbContext.GalleryPaths.AddRangeAsync(galleryPaths);
    }

    public async Task<List<GalleryPath>> GetAllAsync()
    {
        return await _dbContext.GalleryPaths.ToListAsync();
    }
    
    public async Task<int> DeleteAllAsync()
    {
        return await _dbContext.GalleryPaths.ExecuteDeleteAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}