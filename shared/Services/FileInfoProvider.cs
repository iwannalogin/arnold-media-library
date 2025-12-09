using arnold.Models;

namespace arnold.Services;

public class FileInfoProvider : IMetadataProvider {
    public (string name, string description)[] TargetAttributes => [
        ("name", "File Name"),
        ("extension", "File Extension"),
        ("size", "File size (in bytes)"),
        ("updated", "Last file update"),
        ("created", "File creation date"),
    ];

    public Task AddTagsAsync(FileMetadata file) => Task.CompletedTask;

    public Task SetAttributesAsync(FileMetadata file) => Task.Run( () => {
        var fileInfo = new FileInfo( file.Name );

        file.SetAttribute("name", Path.GetFileNameWithoutExtension(file.Name));
        file.SetAttribute("extension", Path.GetExtension( file.Name ) );
        file.SetAttribute("size", fileInfo.Length.ToString() );
        file.SetAttribute("updated", fileInfo.LastWriteTimeUtc.ToString("o") );
        file.SetAttribute("created", fileInfo.CreationTimeUtc.ToString("o") );
    } );
}