using System.Collections.Concurrent;
using MockEngine.Interfaces;
using MockEngine.Models;

namespace MockEngine.Storage;

public class InMemoryMockStore : IMockStore
{
    private readonly ConcurrentDictionary<Guid, MockEndpoint> _endpoints = new();
    private readonly ConcurrentDictionary<string, MockSchema> _schemas = new();

    public Task<MockEndpoint?> GetEndpointAsync(string path, string method)
    {
        var endpoint = _endpoints.Values
            .FirstOrDefault(e => e.Path.Equals(path, StringComparison.OrdinalIgnoreCase) && 
                                e.Method.Equals(method, StringComparison.OrdinalIgnoreCase));
        
        return Task.FromResult(endpoint);
    }

    public Task<MockEndpoint?> GetEndpointByIdAsync(Guid id)
    {
        _endpoints.TryGetValue(id, out var endpoint);
        return Task.FromResult(endpoint);
    }

    public Task<IEnumerable<MockEndpoint>> GetAllEndpointsAsync()
    {
        // Clean up expired endpoints
        var now = DateTime.UtcNow;
        foreach (var endpoint in _endpoints.Values.Where(e => e.ExpiresAt.HasValue && e.ExpiresAt < now))
        {
            _endpoints.TryRemove(endpoint.Id, out _);
        }
        
        return Task.FromResult<IEnumerable<MockEndpoint>>(_endpoints.Values.ToList());
    }

    public Task<MockEndpoint> CreateEndpointAsync(MockEndpoint endpoint)
    {
        if (endpoint.Id == Guid.Empty)
        {
            endpoint.Id = Guid.NewGuid();
        }
        
        endpoint.CreatedAt = DateTime.UtcNow;
        _endpoints[endpoint.Id] = endpoint;
        return Task.FromResult(endpoint);
    }

    public Task<MockEndpoint?> UpdateEndpointAsync(Guid id, MockEndpoint endpoint)
    {
        if (!_endpoints.ContainsKey(id))
        {
            return Task.FromResult<MockEndpoint?>(null);
        }
        
        endpoint.Id = id;
        _endpoints[id] = endpoint;
        return Task.FromResult<MockEndpoint?>(endpoint);
    }

    public Task<bool> DeleteEndpointAsync(Guid id)
    {
        var result = _endpoints.TryRemove(id, out _);
        return Task.FromResult(result);
    }

    public Task<MockSchema?> GetSchemaAsync(string id)
    {
        _schemas.TryGetValue(id, out var schema);
        return Task.FromResult(schema);
    }

    public Task<IEnumerable<MockSchema>> GetAllSchemasAsync()
    {
        return Task.FromResult<IEnumerable<MockSchema>>(_schemas.Values.ToList());
    }

    public Task<MockSchema> CreateSchemaAsync(MockSchema schema)
    {
        if (string.IsNullOrEmpty(schema.Id))
        {
            schema.Id = Guid.NewGuid().ToString();
        }
        
        schema.CreatedAt = DateTime.UtcNow;
        _schemas[schema.Id] = schema;
        return Task.FromResult(schema);
    }

    public Task<MockSchema?> UpdateSchemaAsync(string id, MockSchema schema)
    {
        if (!_schemas.ContainsKey(id))
        {
            return Task.FromResult<MockSchema?>(null);
        }
        
        schema.Id = id;
        schema.UpdatedAt = DateTime.UtcNow;
        _schemas[id] = schema;
        return Task.FromResult<MockSchema?>(schema);
    }

    public Task<bool> DeleteSchemaAsync(string id)
    {
        var result = _schemas.TryRemove(id, out _);
        return Task.FromResult(result);
    }
}