# Mock Engine

Mock Engine is a flexible microservice designed to generate and serve mocked APIs. It enables rapid simulation of API endpoints for development, testing, and application demonstrations.

## Features

- 🚀 **Dynamic Mock Endpoints** - Create on-the-fly endpoints with custom responses
- 🔄 **Realistic Data Generation** - Use Bogus to generate consistent mock data
- 📊 **JSON & OpenAPI Schema Support** - Create mocks from existing schemas
- ⏱️ **Latency Simulation** - Add configurable delays to test application resilience
- 📝 **Built-in Swagger Documentation** - Interactive interface to explore and test APIs
- 🔍 **Intelligent Middleware** - Automatically intercepts requests to serve mock responses

## Prerequisites

- [.NET 9.0](https://dotnet.microsoft.com/download) or later

## Installation

```bash
# Clone the repository
git clone https://github.com/hichammh/mock-engine.git
cd mock-engine

# Restore packages and build
dotnet restore
dotnet build

# Run the application
cd src/MockEngine
dotnet run
```

The service will be available at `http://localhost:5001`.

## Usage

### Creating a Simple Mock Endpoint

```bash
curl -X POST http://localhost:5001/api/mocks \
  -H "Content-Type: application/json" \
  -d '{
    "path": "/products",
    "method": "GET",
    "statusCode": 200,
    "responseBody": {
      "products": [
        {"id": 1, "name": "Product 1"},
        {"id": 2, "name": "Product 2"}
      ]
    }
  }'
```

### Accessing the Mock Endpoint

```bash
curl -X GET http://localhost:5001/products
```

### Generating Data from an Example

```bash
curl -X POST http://localhost:5001/api/mocks/generate-from-example \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "{{faker.name.fullName}}",
    "email": "{{faker.internet.email}}",
    "address": {
      "street": "{{faker.address.streetAddress}}",
      "city": "{{faker.address.city}}"
    }
  }' \
  -G -d 'count=5'
```

## API Gateway Integration

The mock-engine service is designed to seamlessly integrate with an API Gateway that routes requests to `/mock-engine/` to this service. It can be used with any compatible API Gateway (such as the APImitate project's API Gateway using YARP).

## Project Structure

```
src/
├── MockEngine/                  # Main project
    ├── Config/                  # Service configuration
    ├── Controllers/             # API controllers
    ├── Interfaces/              # Interfaces and contracts
    ├── Middleware/              # Middleware for intercepting requests
    ├── Models/                  # Data models
    ├── Services/                # Business logic and services
    └── Storage/                 # Storage implementations
```

## Technologies Used

- ASP.NET Core 9.0
- Bogus (fake data generation)
- JsonSchema.Net (schema validation)
- Microsoft.OpenAPI (OpenAPI specification processing)

## Contributing


## License

[MIT](LICENSE)