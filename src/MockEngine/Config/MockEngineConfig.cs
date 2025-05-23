namespace MockEngine.Config;

public class MockEngineConfig
{
    public static readonly string ConfigSection = "MockEngine";
    
    public TimeSpan DefaultMockLifetime { get; set; } = TimeSpan.FromHours(24);
    public bool EnableSchemaValidation { get; set; } = true;
    public int DefaultResponseDelay { get; set; } = 0;
    
    public StorageConfig Storage { get; set; } = new StorageConfig();
}

public class StorageConfig
{
    public string Type { get; set; } = "InMemory";
    public string ConnectionString { get; set; } = string.Empty;
}