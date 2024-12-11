using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

public class FileMetadata {
    [Key]
    public long Id { get; set; }
    public long LibraryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    public virtual FileTag[] Tags { get; set; } = Array.Empty<FileTag>();
    public virtual FileAttribute[] Attributes { get; set; } = Array.Empty<FileAttribute>();

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