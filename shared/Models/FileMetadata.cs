using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

public class FileMetadata {
    [Key]
    public long Id { get; set; }
    public long LibraryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    public virtual List<FileTag> Tags { get; set; } = [];
    public virtual List<FileAttribute> Attributes { get; set; } = [];

    public bool ContainsTag( string tag ) {
        var testTag = tag.ToLower();
        foreach( var existingTag in Tags ) {
            if( existingTag.Tag.Equals(testTag, StringComparison.CurrentCultureIgnoreCase)) return true;            
        }
        return false;
    }

    public bool AddTag( string tag ) {
        if( ContainsTag(tag) ) return false;
        Tags.Add( new FileTag() {
            Tag = tag
        } );
        return true;
    }

    public IEnumerable<bool> AddTags( params IEnumerable<string> tags ) {
        foreach( var tag in tags ) {
            yield return AddTag(tag);
        }
    }

    public override string ToString() => Name;

    [ModelBuilder]
    private static void OnModelCreating( ModelBuilder builder ) {
        builder.Entity<FileMetadata>()
            .HasMany( fi => fi.Tags )
            .WithOne()
            .HasForeignKey( ft => ft.FileId );

        builder.Entity<FileMetadata>().Navigation( fi => fi.Tags );

        builder.Entity<FileMetadata>()
            .HasMany( fi => fi.Tags )
            .WithOne()
            .HasForeignKey( fa => fa.FileId );

        builder.Entity<FileMetadata>().Navigation( fi => fi.Attributes );
    }
}