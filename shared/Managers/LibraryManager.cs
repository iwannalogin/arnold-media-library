using System.Data;
using arnold.Models;
using arnold.Services;
using arnold.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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

    public IEnumerable<FileMetadata> ListAbandonedMetadata( FileLibrary library ) {
        foreach( var meta in ListMetadata(library) ) {
            if( !File.Exists(meta.Name) ) yield return meta;
        }
    }

    public void DeleteMetadata( IEnumerable<FileMetadata> metadata ) {
        arnoldService.Metadata.RemoveRange( metadata );
        arnoldService.SaveChanges();
    }

    public IEnumerable<FileMonitor> ListMonitors( FileLibrary library )
        => arnoldService.Monitors.Where( monitor => monitor.LibraryId == library.Id );

    public FileMonitor AddMonitor( string libraryName, string name, string directory, string rule, bool isInclusion = true, bool recurse = false ) {
        var fileLibrary = GetLibrary(libraryName);
        if( fileLibrary is null ) throw new KeyNotFoundException($"Unable to find library \"${libraryName}\"");

        var fileMonitor = arnoldService.Monitors.Add( new () {
           LibraryId = fileLibrary.Id,
           Library = fileLibrary,
           Name = name,
           Directory = directory,
           Recurse = recurse,
           Rule = rule,
           IsInclusionRule = isInclusion
        });
        arnoldService.SaveChanges();
        return fileMonitor.Entity;
    }

    public IEnumerable<FileMetadata> UpdateMetadata( string library )
        => UpdateMetadata( GetLibrary(library)! );

    public IEnumerable<FileMetadata> UpdateMetadata( FileLibrary library ) {
        var hasNewEntries = false;
        var monitors = ListMonitors(library);
        foreach( var monitorGroup in monitors.GroupBy( monitor => ( monitor.Directory, monitor.Recurse ) ) ) {
            var directory = monitorGroup.Key.Directory;
            var recurse = monitorGroup.Key.Recurse;

            foreach( var file in Directory.EnumerateFiles( directory, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) ) {
                var shouldAdd =
                    monitorGroup.Any( mon => mon.IsMatch( file ) )
                    && !arnoldService.Metadata.Any( meta => meta.LibraryId == library.Id && meta.Name.ToLower() == file.ToLower() );
                
                if( shouldAdd ) {
                    yield return arnoldService.Metadata.Add( new() {
                       Name = file,
                       Label = Path.GetFileName(file),
                       LibraryId = library.Id 
                    } ).Entity;
                    hasNewEntries = true;
                }
            }
        }
        if( hasNewEntries ) arnoldService.SaveChanges();
    }

    public FileAttributeDefinition? GetAttributeDefinition( string name )
        => arnoldService.AttributeDefinitions
        .FirstOrDefault( attrDef => attrDef.Name.ToLower() == name.ToLower() );

    public FileAttributeDefinition DefineAttribute( string name, string description ) {
        var attrDef = GetAttributeDefinition(name);
        if( attrDef is null ) {
            attrDef ??= arnoldService.AttributeDefinitions.Add( new () {
                Name = name,
                Description = description
            }).Entity;
        } else {
            attrDef.Description = description;
        }
        arnoldService.SaveChanges();
        return attrDef;
    }


}