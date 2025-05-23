using MockEngine.Models;

namespace MockEngine.Interfaces;

public interface IMockStore
{
    Task<MockEndpoint?> GetEndpointAsync(string path, string method);
    Task<MockEndpoint?> GetEndpointByIdAsync(Guid id);
    Task<IEnumerable<MockEndpoint>> GetAllEndpointsAsync();
    Task<MockEndpoint> CreateEndpointAsync(MockEndpoint endpoint);
    Task<MockEndpoint?> UpdateEndpointAsync(Guid id, MockEndpoint endpoint);
    Task<bool> DeleteEndpointAsync(Guid id);
    Task<MockSchema?> GetSchemaAsync(string id);
    Task<IEnumerable<MockSchema>> GetAllSchemasAsync();
    Task<MockSchema> CreateSchemaAsync(MockSchema schema);
    Task<MockSchema?> UpdateSchemaAsync(string id, MockSchema schema);
    Task<bool> DeleteSchemaAsync(string id);
}