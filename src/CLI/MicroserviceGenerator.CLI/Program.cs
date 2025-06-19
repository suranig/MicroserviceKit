using System.CommandLine;
using MicroserviceGenerator.CLI.Commands.Generate;
using MicroserviceGenerator.CLI.Commands.List;
using MicroserviceGenerator.CLI.Commands.Describe;

var rootCommand = new RootCommand("MicroserviceKit - Complete toolkit for generating .NET 8 microservices");

// Add new commands
rootCommand.AddCommand(GenerateCommand.Create());
rootCommand.AddCommand(ListTemplatesCommand.Create());
rootCommand.AddCommand(DescribeCommand.Create());

return await rootCommand.InvokeAsync(args); 