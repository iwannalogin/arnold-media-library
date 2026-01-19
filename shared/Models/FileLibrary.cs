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

    public List<FileMetadata> Files { get; init; } = [];
    public List<FileMonitor> Monitors { get; init; } = [];

    public override string ToString() => Name;

    [ModelBuilder]
    private static void OnModelCreating( ModelBuilder builder ) {
        builder.Entity<FileLibrary>()
            .HasMany( fl => fl.Files )
            .WithOne()
            .HasForeignKey( fi => fi.LibraryId );

        builder.Entity<FileLibrary>()
            .Navigation( fl => fl.Files );

        builder.Entity<FileLibrary>()
            .HasMany( fl => fl.Monitors )
            .WithOne()
            .HasForeignKey( fm => fm.LibraryId );

        builder.Entity<FileLibrary>()
            .Navigation( fl => fl.Monitors );
    }
}