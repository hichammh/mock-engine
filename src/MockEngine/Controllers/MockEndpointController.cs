using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using MockEngine.Interfaces;
using MockEngine.Models;

namespace MockEngine.Controllers;

[ApiController]
[Route("api/mocks")]
public class MockEndpointController : ControllerBase
{
    private readonly IMockStore _mockStore;
    private readonly IMockGenerator _mockGenerator;
    private readonly ILogger<MockEndpointController> _logger;

    public MockEndpointController(
        IMockStore mockStore, 
        IMockGenerator mockGenerator,
        ILogger<MockEndpointController> logger)
    {
        _mockStore = mockStore;
        _mockGenerator = mockGenerator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MockEndpoint>>> GetAllMocks()
    {
        var mocks = await _mockStore.GetAllEndpointsAsync();
        return Ok(mocks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MockEndpoint>> GetMockById(Guid id)
    {
        var mock = await _mockStore.GetEndpointByIdAsync(id);
        if (mock == null)
        {
            return NotFound();
        }
        
        return Ok(mock);
    }

    [HttpPost]
    public async Task<ActionResult<MockEndpoint>> CreateMock([FromBody] MockEndpoint mock)
    {
        if (mock == null)
        {
            return BadRequest("Mock endpoint cannot be null");
        }

        // If there's a schema ID associated, use it to generate sample data
        if (!string.IsNullOrEmpty(mock.SchemaId))
        {
            var schema = await _mockStore.GetSchemaAsync(mock.SchemaId);
            if (schema != null)
            {
                try
                {
                    if (schema.SchemaType == "OpenAPI")
                    {
                        mock.ResponseBody = await _mockGenerator.GenerateFromOpenApiSchemaAsync(
                            schema.Content, mock.Path, mock.Method);
                    }
                    else
                    {
                        // Default to JSON schema
                        mock.ResponseBody = await _mockGenerator.GenerateFromJsonSchemaAsync(schema.Content);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating mock data from schema");
                    return BadRequest($"Invalid schema: {ex.Message}");
                }
            }
        }

        var createdMock = await _mockStore.CreateEndpointAsync(mock);
        return CreatedAtAction(nameof(GetMockById), new { id = createdMock.Id }, createdMock);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MockEndpoint>> UpdateMock(Guid id, [FromBody] MockEndpoint mock)
    {
        if (mock == null)
        {
            return BadRequest("Mock endpoint cannot be null");
        }

        var updatedMock = await _mockStore.UpdateEndpointAsync(id, mock);
        if (updatedMock == null)
        {
            return NotFound();
        }
        
        return Ok(updatedMock);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteMock(Guid id)
    {
        var deleted = await _mockStore.DeleteEndpointAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpPost("generate-from-example")]
    public async Task<ActionResult<object>> GenerateFromExample([FromBody] JsonObject example, [FromQuery] int? count = null)
    {
        try
        {
            var result = await _mockGenerator.GenerateFromExampleAsync(example, count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating mock data from example");
            return BadRequest($"Error generating mock data: {ex.Message}");
        }
    }
}