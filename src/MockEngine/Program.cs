using MockEngine.Interfaces;
using MockEngine.Middleware;
using MockEngine.Services;
using MockEngine.Storage;
using MockEngine.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Load configuration
builder.Services.Configure<MockEngineConfig>(
    builder.Configuration.GetSection(MockEngineConfig.ConfigSection));

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Mock Engine API", Version = "v1" });
});

// Register mock engine services
builder.Services.AddSingleton<IMockStore, InMemoryMockStore>();
builder.Services.AddScoped<IMockGenerator, MockGenerator>();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Use our custom mock middleware - this should be before controllers
app.UseDynamicMockMiddleware();

app.UseRouting();

app.MapControllers();

app.Run();
