using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

public class FileLibraryOverview( FileLibrary library ) {
    public long Id => library.Id;
    public string Name => library.Name;
    public string Description => library.Description;
    public long FileCount => library.Files.Count;
    public long MonitorCount => library.Monitors.Count;

    public override string ToString() => Name;

    public static IEnumerable<FileLibraryOverview> FromQuery( IQueryable<FileLibrary> query )
        => query
            .AsNoTracking()
            .Include( fl => fl.Files )
            .Include( fl => fl.Monitors )
            .Select( fl => new FileLibraryOverview(fl) );
}

public class FileLibrary {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; init; } = -1;
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