using arnold.Models;
using arnold.Services;

namespace arnold.Managers;

public class LibraryManager( ArnoldService arnoldService ) {
    public IQueryable<FileLibrary> ListLibraries()
        => arnoldService.Libraries;

    public FileLibrary? GetLibrary( string libraryName ) {
        if(string.IsNullOrWhiteSpace(libraryName)) {
            throw new ArgumentNullException( nameof(libraryName) );
        }

        return arnoldService
            .Libraries
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
            