using arnold.Models;
using arnold.Services;
using arnold.Utilities;
using Microsoft.EntityFrameworkCore;

namespace arnold.Managers;

public class MetaManager( ArnoldService arnoldService, LibraryManager libraryManager ) {
    public FileMetadata? GetMetadata( string libraryName, string fileName ) {
        ArgumentMissingException.Test( [(nameof(libraryName), libraryName), (nameof(fileName), fileName )] );
        var library = libraryManager.GetLibrary( libraryName );
        if( library is null ) throw new InvalidOperationException($"Failed to find library {libraryName}");

        return arnoldService.Metadata
            .Include( fi => fi.Tags ).Include( fi => fi.Attributes )
            .FirstOrDefault( meta => meta.Name.ToLower() == fileName );
    }

    public void AddTags( string libraryName, string fileName, params IEnumerable<string> tags ) {
        var metadata = GetMetadata( libraryName, fileName );
        if( metadata is null ) throw new InvalidOperationException($"Failed to find file {fileName}");

        foreach( var tag in tags ) {
            if( metadata.ContainsTag(tag) ) continue;
            metadata.Tags.Add( new FileTag() {
                Tag = tag
            });
        }
        arnoldService.SaveChanges();
    }

    public void RemoveTags( string libraryName, string fileName, params IEnumerable<string> tags ) {
        var metadata = GetMetadata( libraryName, fileName );
        if( metadata is null ) throw new InvalidOperationException($"Failed to find file {fileName}");

        tags = tags.Select( tag => tag.ToLower() );
        var toRemove = metadata.Tags.Where( tag => tags.Contains( tag.Tag.ToLower() ) );
        foreach( var tag in toRemove ) {
            metadata.Tags.Remove( tag );
        }
        arnoldService.SaveChanges();
    }
}