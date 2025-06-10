using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Abstractions;

namespace Microservice.Core.TemplateEngine
{
    public class MigrationEngine
    {
        private readonly GenerationContext _context;
        private readonly TemplateConfiguration _configuration;
        private readonly ILogger<MigrationEngine> _logger;

        public MigrationEngine(
            GenerationContext context,
            TemplateConfiguration configuration,
            ILogger<MigrationEngine> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task GenerateMigrationsAsync()
        {
            // Check if persistence or database is enabled (support both standard and enterprise configurations)
            var persistenceEnabled = false;
            
            if (_configuration.Features?.Database?.WriteModel?.Provider != null && 
                _configuration.Features.Database.WriteModel.Provider != "inmemory")
            {
                persistenceEnabled = true;
            }
            else if (_configuration.Features?.Persistence?.Provider != null && 
                     _configuration.Features.Persistence.Provider != "inmemory")
            {
                persistenceEnabled = true;
            }
            
            if (!persistenceEnabled)
            {
                _logger.LogInformation("Persistence is not enabled, skipping migrations");
                return;
            }

            _logger.LogInformation("Starting migration generation");

            // Create migrations directory
            var migrationsPath = Path.Combine(_context.GetInfrastructureProjectPath(), "Persistence", "Migrations");
            Directory.CreateDirectory(migrationsPath);

            // Generate initial migration
            await GenerateInitialMigrationAsync(migrationsPath);

            _logger.LogInformation("Migration generation completed successfully");
        }

        private async Task GenerateInitialMigrationAsync(string migrationsPath)
        {
            var migrationName = $"Initial_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var migrationPath = Path.Combine(migrationsPath, migrationName);
            Directory.CreateDirectory(migrationPath);

            // Create migration files
            await _context.WriteFileAsync(
                Path.Combine("Persistence", "Migrations", migrationName, $"{migrationName}.cs"),
                GenerateMigrationClass(migrationName));

            await _context.WriteFileAsync(
                Path.Combine("Persistence", "Migrations", migrationName, $"{migrationName}.Designer.cs"),
                GenerateMigrationDesigner(migrationName));

            await _context.WriteFileAsync(
                Path.Combine("Persistence", "Migrations", migrationName, $"{migrationName}.resx"),
                GenerateMigrationResource());
        }

        private string GenerateMigrationClass(string migrationName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Migrations;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_configuration.Namespace}.Infrastructure.Persistence.Migrations");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {migrationName} : Migration");
            sb.AppendLine("    {");
            sb.AppendLine("        protected override void Up(MigrationBuilder migrationBuilder)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Add your migration code here");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected override void Down(MigrationBuilder migrationBuilder)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Add your rollback code here");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateMigrationDesigner(string migrationName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Infrastructure;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Migrations;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_configuration.Namespace}.Infrastructure.Persistence.Migrations");
            sb.AppendLine("{");
            sb.AppendLine($"    [DbContext(typeof({_configuration.MicroserviceName}DbContext))]");
            sb.AppendLine($"    [Migration(\"{migrationName}\")]");
            sb.AppendLine($"    partial class {migrationName}");
            sb.AppendLine("    {");
            sb.AppendLine("        protected override void BuildTargetModel(ModelBuilder modelBuilder)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Add your model configuration here");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateMigrationResource()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                   "<root>" +
                   "  <xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">" +
                   "    <xsd:import namespace=\"http://www.w3.org/XML/1998/namespace\" />" +
                   "    <xsd:element name=\"root\" msdata:IsDataSet=\"true\">" +
                   "      <xsd:complexType>" +
                   "        <xsd:choice maxOccurs=\"unbounded\">" +
                   "          <xsd:element name=\"metadata\">" +
                   "            <xsd:complexType>" +
                   "              <xsd:sequence>" +
                   "                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" />" +
                   "              </xsd:sequence>" +
                   "              <xsd:attribute name=\"name\" use=\"required\" type=\"xsd:string\" />" +
                   "              <xsd:attribute name=\"type\" type=\"xsd:string\" />" +
                   "              <xsd:attribute name=\"mimetype\" type=\"xsd:string\" />" +
                   "              <xsd:attribute ref=\"xml:space\" />" +
                   "            </xsd:complexType>" +
                   "          </xsd:element>" +
                   "          <xsd:element name=\"assembly\">" +
                   "            <xsd:complexType>" +
                   "              <xsd:attribute name=\"alias\" type=\"xsd:string\" />" +
                   "              <xsd:attribute name=\"name\" type=\"xsd:string\" />" +
                   "            </xsd:complexType>" +
                   "          </xsd:element>" +
                   "          <xsd:element name=\"data\">" +
                   "            <xsd:complexType>" +
                   "              <xsd:sequence>" +
                   "                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />" +
                   "                <xsd:element name=\"comment\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"2\" />" +
                   "              </xsd:sequence>" +
                   "              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" msdata:Ordinal=\"1\" />" +
                   "              <xsd:attribute name=\"type\" type=\"xsd:string\" msdata:Ordinal=\"3\" />" +
                   "              <xsd:attribute name=\"mimetype\" type=\"xsd:string\" msdata:Ordinal=\"4\" />" +
                   "              <xsd:attribute ref=\"xml:space\" />" +
                   "            </xsd:complexType>" +
                   "          </xsd:element>" +
                   "          <xsd:element name=\"resheader\">" +
                   "            <xsd:complexType>" +
                   "              <xsd:sequence>" +
                   "                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />" +
                   "              </xsd:sequence>" +
                   "              <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" />" +
                   "            </xsd:complexType>" +
                   "          </xsd:element>" +
                   "        </xsd:choice>" +
                   "      </xsd:complexType>" +
                   "    </xsd:element>" +
                   "  </xsd:schema>" +
                   "  <resheader name=\"resmimetype\">" +
                   "    <value>text/microsoft-resx</value>" +
                   "  </resheader>" +
                   "  <resheader name=\"version\">" +
                   "    <value>2.0</value>" +
                   "  </resheader>" +
                   "  <resheader name=\"reader\">" +
                   "    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>" +
                   "  </resheader>" +
                   "  <resheader name=\"writer\">" +
                   "    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>" +
                   "  </resheader>" +
                   "</root>";
        }
    }
} 