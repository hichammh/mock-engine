using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MockEngine.Interfaces;
using MockEngine.Models;

namespace MockEngine.Controllers;

[ApiController]
[Route("api/schemas")]
public class SchemaController : ControllerBase
{
    private readonly IMockStore _mockStore;
    private readonly ILogger<SchemaController> _logger;

    public SchemaController(IMockStore mockStore, ILogger<SchemaController> logger)
    {
        _mockStore = mockStore;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MockSchema>>> GetAllSchemas()
    {
        var schemas = await _mockStore.GetAllSchemasAsync();
        return Ok(schemas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MockSchema>> GetSchemaById(string id)
    {
        var schema = await _mockStore.GetSchemaAsync(id);
        if (schema == null)
        {
            return NotFound();
        }
        
        return Ok(schema);
    }

    [HttpPost]
    public async Task<ActionResult<MockSchema>> CreateSchema([FromBody] MockSchema schema)
    {
        if (schema == null)
        {
            return BadRequest("Schema cannot be null");
        }
        
        try
        {
            // Simple validation - just try to parse the content
            if (schema.SchemaType == "OpenAPI")
            {
                var reader = new Microsoft.OpenApi.Readers.OpenApiStringReader();
                reader.Read(schema.Content, out var diagnostic);
                
                if (diagnostic.Errors.Any())
                {
                    return BadRequest($"Invalid OpenAPI schema: {string.Join(", ", diagnostic.Errors)}");
                }
            }
            else
            {
                // Basic JSON validation
                JsonDocument.Parse(schema.Content);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating schema");
            return BadRequest($"Invalid schema: {ex.Message}");
        }

        var createdSchema = await _mockStore.CreateSchemaAsync(schema);
        return CreatedAtAction(nameof(GetSchemaById), new { id = createdSchema.Id }, createdSchema);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MockSchema>> UpdateSchema(string id, [FromBody] MockSchema schema)
    {
        if (schema == null)
        {
            return BadRequest("Schema cannot be null");
        }

        var updatedSchema = await _mockStore.UpdateSchemaAsync(id, schema);
        if (updatedSchema == null)
        {
            return NotFound();
        }
        
        return Ok(updatedSchema);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSchema(string id)
    {
        var deleted = await _mockStore.DeleteSchemaAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}