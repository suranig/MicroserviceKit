using System.CommandLine;
using MicroserviceGenerator.CLI.Models;
using MicroserviceGenerator.CLI.Services;

namespace MicroserviceGenerator.CLI.Commands.Generate;

public static class GenerateCommand
{
    public static Command Create()
    {
        var command = new Command("generate", "Generate a new microservice from template");
        
        var nameArgument = new Argument<string>("name", "Name of the microservice");
        var templateOption = new Option<string?>("--template", "Template name or path");
        var outputOption = new Option<string>("--output", () => "./", "Output directory");
        var interactiveOption = new Option<bool>("--interactive", () => false, "Run in interactive mode");
        var customizeOption = new Option<bool>("--customize", () => false, "Customize template parameters");
        
        // New options for parameterization
        var aggregatesOption = new Option<string[]>("--aggregates", () => Array.Empty<string>(), "Custom aggregates");
        var externalServicesOption = new Option<string[]>("--external-services", () => Array.Empty<string>(), "External services");
        var databaseOption = new Option<string?>("--database", "Database provider (postgresql, sqlserver, mysql, sqlite)");
        var messagingOption = new Option<string?>("--messaging", "Messaging provider (rabbitmq, servicebus, inmemory)");
        var authOption = new Option<string?>("--auth", "Authentication type (jwt, oauth, none)");
        var apiStyleOption = new Option<string?>("--api-style", "API style (controllers, minimal)");
        
        command.AddArgument(nameArgument);
        command.AddOption(templateOption);
        command.AddOption(outputOption);
        command.AddOption(interactiveOption);
        command.AddOption(customizeOption);
        command.AddOption(aggregatesOption);
        command.AddOption(externalServicesOption);
        command.AddOption(databaseOption);
        command.AddOption(messagingOption);
        command.AddOption(authOption);
        command.AddOption(apiStyleOption);
        
        command.SetHandler(async (name, template, output, interactive, customize, aggregates, externalServices, database) =>
        {
            await ExecuteGenerateAsync(name, template, output, interactive, customize, aggregates, externalServices, database);
        }, nameArgument, templateOption, outputOption, interactiveOption, customizeOption, aggregatesOption, externalServicesOption, databaseOption);
        
        return command;
    }
    
    private static async Task ExecuteGenerateAsync(
        string name, 
        string? template, 
        string output, 
        bool interactive, 
        bool customize, 
        string[] aggregates, 
        string[] externalServices, 
        string? database)
    {
        try
        {
            Console.WriteLine($"🚀 Generating microservice: {name}");
            Console.WriteLine();
            
            var options = new GenerationOptions
            {
                ServiceName = name,
                TemplateName = template ?? "",
                OutputPath = output,
                Interactive = interactive,
                Customize = customize,
                CustomAggregates = aggregates.ToList(),
                ExternalServices = externalServices.ToList(),
                DatabaseProvider = database
            };
            
            // Run interactive mode if requested or if no template specified
            if (interactive || string.IsNullOrEmpty(template))
            {
                var interactiveMode = new InteractiveMode();
                options = await interactiveMode.RunInteractiveAsync(options);
            }
            
            // Validate options
            var validationService = new ValidationService();
            var validationResult = validationService.ValidateGenerationOptions(options);
            
            if (!validationResult.IsValid)
            {
                Console.WriteLine("❌ Validation errors:");
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine($"   • {error}");
                }
                Environment.Exit(1);
                return;
            }
            
            // Generate microservice
            var generateService = new GenerateService();
            await generateService.GenerateAsync(options);
            
            Console.WriteLine($"✅ Microservice '{name}' generated successfully!");
            Console.WriteLine($"📁 Output directory: {Path.GetFullPath(output)}");
            Console.WriteLine();
            Console.WriteLine("🚀 Next steps:");
            Console.WriteLine($"   cd {output}");
            Console.WriteLine("   dotnet restore");
            Console.WriteLine("   dotnet build");
            Console.WriteLine("   dotnet run --project src/*/*.Api");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating microservice: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner error: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }
} 