using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

[PrimaryKey(nameof(FileId), nameof(Tag))]
public class FileTag {
    public long FileId { get; set; }
    /// <summary>
    /// Tag associated with file. Must always be in all caps.
    /// TODO: Enforce in the DB layer if possible
    /// </summary>
    public string Tag { get; set; } = string.Empty;
}