using arnold.Managers;
using arnold.Routing;
using arnold.Services;
using arnold.Utilities;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDbContext<ArnoldService>();
services.AddSingleton<LibraryManager>();

services.AddFormattingServices();

var commandFactory = new CommandFactory(services.BuildServiceProvider());
var rootCommand = commandFactory.CreateCommand( RootRouting.RootHandler );

var parseResult = rootCommand.Parse( args );
await parseResult.InvokeAsync();