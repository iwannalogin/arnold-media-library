using System.ComponentModel.DataAnnotations;

namespace arnold.Models;

public class FileAttributeDefinition {
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}