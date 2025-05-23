using System.Text.Json.Nodes;

namespace MockEngine.Interfaces;

public interface IMockGenerator
{
    Task<object> GenerateFromJsonSchemaAsync(string jsonSchema);
    Task<object> GenerateFromOpenApiSchemaAsync(string openApiSchema, string path, string method);
    Task<object> GenerateFromExampleAsync(JsonNode example, int? count = null);
}