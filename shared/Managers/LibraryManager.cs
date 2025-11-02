using System.Data;
using arnold.Models;
using arnold.Services;
using arnold.Utilities;
using Microsoft.EntityFrameworkCore;

namespace arnold.Managers;

public class LibraryManager( ArnoldService arnoldService ) {
    public IQueryable<FileLibrary> ListLibraries()
        => arnoldService.Libraries;

    public FileLibrary CreateLibrary( string libraryName, string? description = null ) {
        ArgumentMissingException.Test( nameof(libraryName), libraryName );
        var library = GetLibrary( libraryName );
        if( library is not null ) {
            throw new InvalidOperationException( $"Library {libraryName} already exists." );
        }

        library = arnoldService.Libraries.Add( new FileLibrary() {
            Name = libraryName,
            Description = description ?? "New media library"
        } ).Entity;

        arnoldService.SaveChanges();
        return library;
    }

    public bool DeleteLibrary( string libraryName ) {
        try {
            return DeleteLibrary( GetLibrary(libraryName) );
        } catch {
            return false;
        }
    }
    public bool DeleteLibrary( FileLibrary? library ) {
        try {
            if( library is null ) return false;
            arnoldService.Libraries.Remove(library);
            arnoldService.SaveChanges();
            return true;
        } catch {
            return false;
        }
    }

    public FileLibrary? GetLibrary( string libraryName ) {
        ArgumentMissingException.Test( nameof(libraryName), libraryName );
        return arnoldService
            .Libraries
            .Include( fl => fl.Files ).Include( fl => fl.Monitors )
            .FirstOrDefault( lib => lib.Name.ToLower() == libraryName.ToLower() );
    }

    public IQueryable<FileMetadata> ListMetadata( string libraryName ) {
        var library = GetLibrary(libraryName);
        if( library is null ) throw new KeyNotFoundException($"Unable to find library \"${libraryName}\"");
        return ListMetadata(library);
    }

    public IQueryable<FileMetadata> ListMetadata( FileLibrary library )
        => arnoldService
            .Metadata
            .Where( meta => meta.LibraryId == library.Id );
}
            