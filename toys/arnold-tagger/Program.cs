using arnold.Managers;
using arnold.Utilities;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddArnold();

var rootDefinition = new CommandDefinition(
    name: "arnold-tagger",
    description: "Quickly tag files in arnold",
    (
        [FromServices] LibraryManager libraryManager,
        [FromServices] MetaManager metaManager,
        string libraryName,
        string file
     ) => {
         if( !File.Exists(file) ) throw new FileNotFoundException( null, file );
         var library = libraryManager.GetLibrary(libraryName) ?? throw new InvalidOperationException($"Failed to find library \"{libraryName}\"");
         var usedTags = libraryManager.ListMetadata(library)
            .SelectMany( meta => meta.Tags )
            .Distinct();

         return 0;
     }
);

var commandFactory = new CommandFactory(services.BuildServiceProvider());
//var rootCommand = commandFactory.CreateCommand( new(), true );
//var parseResult = rootCommand.Parse(args);
//await parseResult.InvokeAsync();