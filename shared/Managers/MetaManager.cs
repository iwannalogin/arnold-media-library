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
            .FirstOrDefault( meta => meta.Name.ToLower() == fileName.ToLower() );
    }

    public void AddTags( FileLibrary library, string[] paths, params IEnumerable<string> tags ) {
        var files = new List<string>();
        foreach( var path in paths ) {
            var pathAttr = File.GetAttributes(path);
            if( pathAttr.HasFlag( FileAttributes.Directory ) ) {
                var dirFiles = Directory.GetFiles( path, "*", SearchOption.AllDirectories );
                files.AddRange( dirFiles.Select( f => f.ToLower() ) );
            } else {
                files.Add( path.ToLower() );
            }
        }

        var metadata = arnoldService
            .Metadata
            .Where( meta => meta.LibraryId == library.Id )
            .Where( meta => files.Contains( meta.Name.ToLower() ) )
            .Include( fi => fi.Tags );

        foreach( var meta in metadata ) {
            foreach( var tag in tags ) {
                if( meta.ContainsTag(tag) ) continue;
                meta.Tags.Add( new FileTag() {
                    Tag = tag
                } );
            }
            //meta.AddTags( tags );
        }
        arnoldService.SaveChanges();
    }

    public void AddTags( string libraryName, string fileName, params IEnumerable<string> tags ) {
        var metadata = GetMetadata( libraryName, fileName );
        if( metadata is null ) throw new InvalidOperationException($"Failed to find file {fileName}");

        metadata.AddTags( tags );
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