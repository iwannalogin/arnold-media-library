using arnold.Models;
using arnold.Services;
using arnold.Utilities;
using Microsoft.EntityFrameworkCore;

namespace arnold;

public class ArnoldManager {

    protected ArnoldService arnold;

    public ArnoldManager( ArnoldService arnold ) {
        this.arnold = arnold;
        arnold.Initialize();
    }

#region Library CRUD
    /// <summary>
    /// Get an IQueryable of all FileLibraries
    /// </summary>
    /// <returns>An IQueryable of all FileLibraries</returns>
    public IQueryable<FileLibrary> GetLibraries()
        => arnold.Libraries;

    /// <summary>
    /// Get a FileLibrary by its Name
    /// </summary>
    /// <param name="libraryName">Library Name</param>
    /// <returns>The FileLibrary on success, null on failure</returns>
    public FileLibrary? GetLibrary( string libraryName ) {
        if( string.IsNullOrWhiteSpace(libraryName ) ) return null;
        return arnold.Libraries
            .FirstOrDefault( lib => lib.Name.ToLower() == libraryName.ToLower() );
    }

    /// <summary>
    /// Create a new FileLibrary
    /// </summary>
    /// <param name="libraryName">Library Name</param>
    /// <param name="description">Library Description</param>
    /// <returns>Newly created FileLibrary</returns>
    /// <exception cref="InvalidOperationException">An existing library has been found</exception>
    /// <exception cref="ArgumentMissingException">A blank library name was provided</exception>
    public FileLibrary AddLibrary( string libraryName, string? description = null ) {
        ArgumentMissingException.Test( nameof(libraryName), libraryName );
        if( arnold.Libraries.Any( lib => lib.Name.ToLower() == libraryName.ToLower() ) ) {
            throw new InvalidOperationException($"Library \"{libraryName}\" already exists.");
        }

        var library = arnold.Add( new FileLibrary {
            Name = libraryName,
            Description = description ?? "New media library"
        } ).Entity;

        arnold.SaveChanges();
        return library;
    }

    /// <summary>
    /// Update the database to reflect FileLibrary changes
    /// </summary>
    /// <param name="library">The FileLibrary to update</param>
    public void UpdateLibrary(FileLibrary library) {
        arnold.Libraries.Update(library);
        arnold.SaveChanges();
    }

    /// <summary>
    /// Delete an existing FileLibrary
    /// </summary>
    /// <param name="library">The FileLibrary to delete</param>
    public void DeleteLibrary(FileLibrary library) {
        arnold.Libraries.Remove(library);
        arnold.SaveChanges();
    }
#endregion

#region Monitor CRUD
    /// <summary>
    /// Get an IQuerable of all FileMonitors
    /// </summary>
    /// <returns>An IQueryable of all FileMonitors</returns>
    public IQueryable<FileMonitor> GetMonitors()
        => arnold.Monitors
            .AsNoTracking();

    /// <summary>
    /// Get an IQueryable of all FileMonitors associated with a Library
    /// </summary>
    /// <param name="library">The target Library</param>
    /// <returns>An IQueryable of all FileMonitors associated with the Library.</returns>
    public IQueryable<FileMonitor> GetMonitors( FileLibrary library )
        => arnold.Monitors
            .AsNoTracking()
            .Where( mon => mon.LibraryId == library.Id );

    /// <summary>
    /// Get a FileMonitor by Library and Name
    /// </summary>
    /// <param name="library">The associated Library</param>
    /// <param name="monitorName">The Monitor Name</param>
    /// <returns>The FileMonitor, or null if none is found.</returns>
    public FileMonitor? GetMonitor( FileLibrary library, string monitorName )
        => GetMonitors(library)
            .FirstOrDefault( mon => mon.Name.ToLower() == monitorName.ToLower() );

    /// <summary>
    /// Add a new FileMonitor to a FileLibrary
    /// </summary>
    /// <param name="library">The Library to associate the FileMonitor with</param>
    /// <param name="monitorName">The Monitor name</param>
    /// <param name="directory">The directory to monitor</param>
    /// <param name="rule">A Regex rule to match names against</param>
    /// <param name="recurse">Should Monitor recurse the target directory</param>
    /// <param name="isInclusion">Include files that match this rule, or exclude files that don't match</param>
    /// <returns>The new FileMonitor</returns>
    /// <exception cref="InvalidOperationException">A monitor with the same name associated with the provided library already exists</exception>
    public FileMonitor AddMonitor( FileLibrary library, string monitorName, string directory, string rule = ".*", bool recurse = false, bool isInclusion = true ) {
        if( arnold.Monitors.Any( mon => mon.LibraryId == library.Id && mon.Name.ToLower() == monitorName ) ) {
            throw new InvalidOperationException($"A monitor named \"{monitorName}\" for library \"{library.Name}.\"");
        }

        var monitor = arnold.Monitors.Add(new FileMonitor {
            LibraryId = library.Id,
            Library = library,
            Name = monitorName,
            Directory = directory,
            Rule = rule,
            Recurse = recurse,
            IsInclusionRule = isInclusion
        }).Entity;
        arnold.SaveChanges();
        return monitor;
    }

    /// <summary>
    /// Update the database to reflect FileMonitor changes
    /// </summary>
    /// <param name="monitor">The FileMonitor to update</param>
    public void UpdateMonitor( FileMonitor monitor ) {
        arnold.Monitors.Update(monitor);
        arnold.SaveChanges();
    }

    /// <summary>
    /// Delete an existing FileMonitor
    /// </summary>
    /// <param name="monitor">The FileMonitor to delete</param>
    public void DeleteMonitor( FileMonitor monitor ) {
        arnold.Monitors.Remove(monitor);
        arnold.SaveChanges();
    }
#endregion

#region File CRUD
    public IQueryable<FileMetadata> GetFiles()
        => arnold.Metadata
            .Include( file => file.Tags )
            .Include( file => file.Attributes )
            .AsNoTracking();

    public IQueryable<FileMetadata> GetFiles( FileLibrary library )
        => GetFiles()
            .Where( file => file.LibraryId == library.Id );

    public FileMetadata? GetFile( FileLibrary library, string name )
        => GetFiles(library).FirstOrDefault( file => file.Name.ToLower() == name.ToLower() );

    public void UpdateFiles( IEnumerable<FileMetadata> files ) {
        arnold.Metadata.UpdateRange(files);
        arnold.SaveChanges();
    }

    public void DeleteFiles( IEnumerable<FileMetadata> files ) {
        arnold.Metadata.RemoveRange(files);
        arnold.SaveChanges();
    }
#endregion

#region Tag CRUD

#endregion

#region Attribute CRUD
    public IQueryable<FileAttributeDefinition> GetAttributeDefinitions()
        => arnold.AttributeDefinitions.AsNoTracking();

    public FileAttributeDefinition? GetAttributeDefinition( string name )
        => arnold.AttributeDefinitions.FirstOrDefault( def => def.Name.ToLower() == name.ToLower() );

    public FileAttributeDefinition AddAttributeDefinition( string name, string? description = null ) {
        if( arnold.AttributeDefinitions.Any( def => def.Name.ToLower() == name.ToLower() ) ) {
            throw new InvalidOperationException($"An attribute named \"{name}\" already exists.");
        }

        var attrDef = arnold.AttributeDefinitions.Add( new () {
           Name = name,
           Description = description ?? string.Empty
        }).Entity;

        arnold.SaveChanges();
        return attrDef;
    }
#endregion

#region Common Actions
    public IEnumerable<FileMetadata> RunMonitors( FileLibrary library ) {
        var results = new List<FileMetadata>();
        foreach( var monitorGroup in GetMonitors(library).AsEnumerable().GroupBy( mon => ( mon.Directory, mon.Recurse ) ) ) {
            var directory = monitorGroup.Key.Directory;
            var recurse = monitorGroup.Key.Recurse;

            foreach( var file in Directory.EnumerateFiles( directory, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) ) {
                var shouldAdd =
                    monitorGroup.Any( mon => mon.IsMatch( file ) )
                    && !arnold.Metadata.Any( meta => meta.LibraryId == library.Id && meta.Name.ToLower() == file.ToLower() );
                
                if( shouldAdd ) {
                    results.Add( new() {
                       Name = file,
                       Label = Path.GetFileName(file),
                       LibraryId = library.Id,
                       Library = library
                    } );
                }
            }
        }
        if( results.Any() ) {
            arnold.Metadata.AddRange(results);
            arnold.SaveChanges();
        }
        return results;
    }

    public void RunProviders( FileLibrary library ) {
        var providers = System.Reflection.Assembly
            .GetCallingAssembly()
            .GetTypes()
            .Where( type => type.IsAssignableTo( typeof(IMetadataProvider) ) && !type.IsInterface )
            .Select( type => (Activator.CreateInstance(type) as IMetadataProvider)! );

        var attrDefinitions = new List<FileAttributeDefinition>();
        foreach( var provider in providers ) {
            foreach( var attrDef in provider.TargetAttributes ) {
                if( attrDefinitions.Any( def => def.Name.ToLower() == attrDef.name.ToLower() ) ) {
                    return;
                }
                attrDefinitions.Add( GetAttributeDefinition(attrDef.name)
                    ?? AddAttributeDefinition( attrDef.name, attrDef.description ) );
            }
        }

        var files = GetFiles(library);
        foreach( var file in files ) {
            foreach( var provider in providers ) {
                Task.WhenAll(
                    provider.AddTagsAsync(file),
                    provider.SetAttributesAsync(file)
                ).Wait();
            }
        }
        UpdateFiles(files);
    }
#endregion
}