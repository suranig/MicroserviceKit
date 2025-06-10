using System.Threading.Tasks;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Abstractions;

namespace Microservice.Core.TemplateEngine
{
    public interface ITemplateEngine
    {
        Task GenerateAsync(GenerationContext context, TemplateConfiguration configuration);
    }
} 