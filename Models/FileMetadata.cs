using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

public class FileMetadata {
    [Key]
    public long Id { get; set; }
    public long LibraryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    public virtual List<FileTag> Tags { get; set; } = new List<FileTag>();
    public virtual List<FileAttribute> Attributes { get; set; } = new List<FileAttribute>();

    public bool ContainsTag( string tag ) {
        var testTag = tag.ToLower();
        foreach( var existingTag in Tags ) {
            if( existingTag.Tag.ToLower() == testTag ) return true;            
        }
        return false;
    }

    [ModelBuilder]
    private static void OnModelCreating( ModelBuilder builder ) {
        builder.Entity<FileMetadata>()
            .HasMany( fi => fi.Tags )
            .WithOne()
            .HasForeignKey( ft => ft.FileId );

        builder.Entity<FileMetadata>()
            .HasMany( fi => fi.Tags )
            .WithOne()
            .HasForeignKey( fa => fa.FileId );
    }
}