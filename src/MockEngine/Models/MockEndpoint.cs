namespace MockEngine.Models;

public class MockEndpoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public int StatusCode { get; set; } = 200;
    public Dictionary<string, string> Headers { get; set; } = new();
    public object? ResponseBody { get; set; }
    public int DelayMs { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsPersistent { get; set; } = false;
    public string? SchemaId { get; set; }
}