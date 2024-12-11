using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

[PrimaryKey(nameof(AttributeId), nameof(FileId))]
public class FileAttribute {
    public long FileId { get; set; }
    public long AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public virtual FileAttributeDefinition? Definition { get; set; } = null;

    [ModelBuilder]
    private static void OnModelCreating( ModelBuilder builder ) {
        builder.Entity<FileAttribute>()
            .HasOne( fa => fa.Definition )
            .WithMany()
            .HasForeignKey( fa => fa.AttributeId );
    }
}