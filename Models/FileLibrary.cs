using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

public class FileLibrary {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public IEnumerable<FileMetadata> Files { get; set; } = Enumerable.Empty<FileMetadata>();

    [ModelBuilder]
    private static void OnModelCreating( ModelBuilder builder ) {
        builder.Entity<FileLibrary>()
            .HasMany( fl => fl.Files )
            .WithOne()
            .HasForeignKey( fi => fi.LibraryId );
    }
}