using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTools.Core;

public class FilePath
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Path { get; set; }
    public int FilePathTypeId { get; set; }
    public FilePathType FilePathType { get; set; }
}