using Microsoft.EntityFrameworkCore;
using ScreenTools.Core;

namespace ScreenTools.Infrastructure;

public class ScreenToolsDbContext : DbContext
{
    public ScreenToolsDbContext(DbContextOptions<ScreenToolsDbContext> options) : base(options)
    {
    }
    
    public DbSet<FilePath> FilePaths { get; set; }
    public DbSet<FilePathType> FilePathTypes { get; set; }
}