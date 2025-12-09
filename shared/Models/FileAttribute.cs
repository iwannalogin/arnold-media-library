using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

[PrimaryKey(nameof(AttributeId), nameof(FileId))]
public class FileAttribute {
    public long FileId { get; set; }
    public long AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public required virtual FileAttributeDefinition Definition { get; set; }

    [ModelBuilder]
    private static void OnModelCreating( ModelBuilder builder ) {
        builder.Entity<FileAttribute>()
            .HasOne( fa => fa.Definition )
            .WithMany()
            .HasForeignKey( fa => fa.AttributeId );
        
        builder.Entity<FileAttribute>()
            .Navigation( fa => fa.Definition )
            .AutoInclude();
    }
}