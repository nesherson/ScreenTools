using Microsoft.EntityFrameworkCore;
using ScreenTools.Core;

namespace ScreenTools.Infrastructure;

public class ScreenToolsDbContext : DbContext
{
    public ScreenToolsDbContext()
    {
    }
    
    public ScreenToolsDbContext(DbContextOptions<ScreenToolsDbContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data source=ScreenTools.db");
    }

    public DbSet<GalleryPath> GalleryPaths { get; set; }
}