using Microsoft.EntityFrameworkCore;
using ScreenTools.Core;

namespace ScreenTools.Infrastructure;

public class ScreenToolsDbContext : DbContext
{
    public ScreenToolsDbContext(DbContextOptions<ScreenToolsDbContext> options) : base(options)
    {
    }
    
    public DbSet<GalleryPath> GalleryPaths { get; set; }
}