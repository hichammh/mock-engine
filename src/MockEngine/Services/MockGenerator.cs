using System.Text.Json;
using System.Text.Json.Nodes;
using Bogus;
using Microsoft.OpenApi.Readers;
using MockEngine.Interfaces;

namespace MockEngine.Services;

public class MockGenerator : IMockGenerator
{
    private readonly ILogger<MockGenerator> _logger;
    private readonly Faker _faker;

    public MockGenerator(ILogger<MockGenerator> logger)
    {
        _logger = logger;
        _faker = new Faker();
    }

    public Task<object> GenerateFromJsonSchemaAsync(string jsonSchema)
    {
        try
        {
            // Simplified implementation - parse schema as JSON and generate based on structure
            JsonNode? schemaNode = JsonNode.Parse(jsonSchema);
            if (schemaNode == null)
            {
                throw new ArgumentException("Invalid JSON schema", nameof(jsonSchema));
            }
            
            var result = GenerateMockFromJsonStructure(schemaNode);
            return Task.FromResult<object>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating data from JSON schema");
            throw new ArgumentException("Invalid JSON schema", nameof(jsonSchema), ex);
        }
    }

    public Task<object> GenerateFromOpenApiSchemaAsync(string openApiSchema, string path, string method)
    {
        try
        {
            var reader = new OpenApiStringReader();
            var document = reader.Read(openApiSchema, out var diagnostic);

            if (diagnostic.Errors.Any())
            {
                throw new ArgumentException("Invalid OpenAPI schema: " + string.Join(", ", diagnostic.Errors));
            }

            // Find path and operation
            if (!document.Paths.TryGetValue(path, out var pathItem))
            {
                throw new ArgumentException($"Path '{path}' not found in OpenAPI schema");
            }

            var operation = method.ToUpperInvariant() switch
            {
                "GET" => pathItem.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Get, out var op) ? op : null,
                "POST" => pathItem.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Post, out var op) ? op : null,
                "PUT" => pathItem.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Put, out var op) ? op : null,
                "DELETE" => pathItem.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Delete, out var op) ? op : null,
                "PATCH" => pathItem.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Patch, out var op) ? op : null,
                _ => null
            };

            if (operation == null)
            {
                throw new ArgumentException($"Method '{method}' not found for path '{path}' in OpenAPI schema");
            }

            // Find success response schema
            var successResponse = operation.Responses.FirstOrDefault(r => 
                r.Key == "200" || r.Key == "201" || r.Key == "OK" || r.Key == "Created");

            if (successResponse.Value == null)
            {
                return Task.FromResult<object>(new { message = "Success" });
            }

            // Use example if available or generate default response
            foreach (var content in successResponse.Value.Content)
            {
                if (content.Value.Example != null)
                {
                    // Convert OpenAPI example to a C# object
                    var exampleJson = JsonSerializer.Serialize(content.Value.Example);
                    return Task.FromResult<object>(JsonSerializer.Deserialize<object>(exampleJson)!);
                }
                
                if (content.Value.Schema != null)
                {
                    // Generate a simple mock object
                    return Task.FromResult<object>(GenerateDefaultResponse());
                }
            }

            return Task.FromResult<object>(GenerateDefaultResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating data from OpenAPI schema");
            throw new ArgumentException("Error processing OpenAPI schema", nameof(openApiSchema), ex);
        }
    }

    public Task<object> GenerateFromExampleAsync(JsonNode example, int? count = null)
    {
        try
        {
            if (count.HasValue && count > 1)
            {
                if (example is JsonArray || example is JsonObject)
                {
                    // Generate multiple items
                    var array = new JsonArray();
                    for (int i = 0; i < count; i++)
                    {
                        array.Add(GenerateFakeData(example));
                    }
                    return Task.FromResult<object>(array);
                }
            }

            return Task.FromResult<object>(GenerateFakeData(example));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating data from example");
            throw new ArgumentException("Error processing example", nameof(example), ex);
        }
    }

    private JsonNode GenerateMockFromJsonStructure(JsonNode schemaNode)
    {
        if (schemaNode is JsonObject obj && obj.TryGetPropertyValue("type", out var typeNode))
        {
            string? type = typeNode?.GetValue<string>();
            
            return type switch
            {
                "object" => GenerateMockObject(obj),
                "array" => GenerateMockArray(obj),
                "string" => JsonValue.Create(_faker.Lorem.Sentence())!,
                "number" => JsonValue.Create(_faker.Random.Double())!,
                "integer" => JsonValue.Create(_faker.Random.Int())!,
                "boolean" => JsonValue.Create(_faker.Random.Bool())!,
                _ => JsonValue.Create(_faker.Lorem.Word())!
            };
        }
        
        // Default to an empty object if schema is not properly structured
        return new JsonObject();
    }

    private JsonObject GenerateMockObject(JsonObject schemaObj)
    {
        var result = new JsonObject();
        
        if (schemaObj.TryGetPropertyValue("properties", out var propsNode) && propsNode is JsonObject props)
        {
            foreach (var prop in props.AsObject())
            {
                if (prop.Value is JsonObject propSchema)
                {
                    result.Add(prop.Key, GenerateMockFromJsonStructure(propSchema));
                }
            }
        }
        
        return result;
    }

    private JsonArray GenerateMockArray(JsonObject schemaObj)
    {
        var result = new JsonArray();
        var count = _faker.Random.Int(1, 5);
        
        if (schemaObj.TryGetPropertyValue("items", out var itemsNode))
        {
            for (int i = 0; i < count; i++)
            {
                result.Add(GenerateMockFromJsonStructure(itemsNode!));
            }
        }
        
        return result;
    }

    private object GenerateDefaultResponse()
    {
        return new
        {
            id = _faker.Random.Guid(),
            name = _faker.Name.FullName(),
            email = _faker.Internet.Email(),
            createdAt = _faker.Date.Past(),
            status = _faker.PickRandom("active", "inactive", "pending"),
            data = new
            {
                description = _faker.Lorem.Paragraph(),
                value = _faker.Random.Decimal()
            }
        };
    }

    private JsonNode GenerateFakeData(JsonNode template)
    {
        if (template is JsonObject obj)
        {
            var result = new JsonObject();
            foreach (var property in obj)
            {
                result.Add(property.Key, GenerateFakeData(property.Value!));
            }
            return result;
        }
        else if (template is JsonArray array)
        {
            var result = new JsonArray();
            foreach (var item in array)
            {
                result.Add(GenerateFakeData(item!));
            }
            return result;
        }
        else if (template is JsonValue value)
        {
            if (value.TryGetValue<string>(out var stringValue))
            {
                // Check for special placeholders that begin with {{faker.
                if (stringValue.StartsWith("{{faker."))
                {
                    var fakerMethod = stringValue.Substring(8, stringValue.Length - 10); // Remove "{{faker." and "}}"
                    return GenerateFakerValue(fakerMethod);
                }
                return JsonValue.Create(_faker.Random.Replace("*".PadRight(stringValue.Length, '*')));
            }
            else if (value.TryGetValue<int>(out var intValue))
            {
                return JsonValue.Create(_faker.Random.Int(0, intValue * 2));
            }
            else if (value.TryGetValue<double>(out var doubleValue))
            {
                return JsonValue.Create(_faker.Random.Double(0, doubleValue * 2));
            }
            else if (value.TryGetValue<bool>(out var boolValue))
            {
                return JsonValue.Create(_faker.Random.Bool());
            }
            else
            {
                return value;
            }
        }
        
        return template;
    }

    private JsonNode GenerateFakerValue(string fakerMethod)
    {
        return fakerMethod switch
        {
            "name.fullName" => JsonValue.Create(_faker.Name.FullName()),
            "name.firstName" => JsonValue.Create(_faker.Name.FirstName()),
            "name.lastName" => JsonValue.Create(_faker.Name.LastName()),
            "internet.email" => JsonValue.Create(_faker.Internet.Email()),
            "internet.url" => JsonValue.Create(_faker.Internet.Url()),
            "address.streetAddress" => JsonValue.Create(_faker.Address.StreetAddress()),
            "address.city" => JsonValue.Create(_faker.Address.City()),
            "address.zipCode" => JsonValue.Create(_faker.Address.ZipCode()),
            "phone.phoneNumber" => JsonValue.Create(_faker.Phone.PhoneNumber()),
            "lorem.paragraph" => JsonValue.Create(_faker.Lorem.Paragraph()),
            "lorem.sentence" => JsonValue.Create(_faker.Lorem.Sentence()),
            "date.past" => JsonValue.Create(_faker.Date.Past().ToString("o")),
            "date.future" => JsonValue.Create(_faker.Date.Future().ToString("o")),
            "date.recent" => JsonValue.Create(_faker.Date.Recent().ToString("o")),
            "finance.amount" => JsonValue.Create(_faker.Finance.Amount()),
            "finance.account" => JsonValue.Create(_faker.Finance.Account()),
            "random.uuid" => JsonValue.Create(_faker.Random.Uuid().ToString()),
            "random.number" => JsonValue.Create(_faker.Random.Number(1000)),
            "commerce.productName" => JsonValue.Create(_faker.Commerce.ProductName()),
            "commerce.price" => JsonValue.Create(_faker.Commerce.Price()),
            "company.companyName" => JsonValue.Create(_faker.Company.CompanyName()),
            _ => JsonValue.Create(_faker.Lorem.Word())
        };
    }
}