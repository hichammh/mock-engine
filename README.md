# Mock Engine

Mock Engine est un microservice flexible conçu pour générer et servir des API mockées. Il permet de simuler rapidement des endpoints d'API pour le développement, les tests et la démonstration d'applications.

## Fonctionnalités

- 🚀 **Création dynamique d'endpoints mock** - Création à la volée d'endpoints avec réponse personnalisée
- 🔄 **Génération de données réalistes** - Utilisation de Bogus pour générer des données fictives cohérentes
- 📊 **Support des schémas JSON & OpenAPI** - Création de mocks à partir de schémas existants
- ⏱️ **Simulation de latence** - Ajout de délais configurables pour tester la résilience des applications
- 📝 **Documentation Swagger intégrée** - Interface interactive pour explorer et tester les API
- 🔍 **Middleware intelligent** - Interception automatique des requêtes pour servir les réponses mockées

## Prérequis

- [.NET 9.0](https://dotnet.microsoft.com/download) ou version ultérieure

## Installation

```bash
# Cloner le dépôt
git clone https://github.com/hichammh/mock-engine.git
cd mock-engine

# Restaurer les packages et compiler
dotnet restore
dotnet build

# Lancer l'application
cd src/MockEngine
dotnet run
```

Le service sera accessible à l'adresse `http://localhost:5001`.

## Utilisation

### Création d'un endpoint mock simple

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

### Accès à l'endpoint mock

```bash
curl -X GET http://localhost:5001/products
```

### Génération de données à partir d'un exemple

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

## Intégration avec API Gateway

Le service mock-engine est conçu pour s'intégrer parfaitement avec une API Gateway qui route les requêtes vers `/mock-engine/` vers ce service. Il peut être utilisé avec n'importe quelle API Gateway compatible (comme l'API Gateway du projet APImitate qui utilise YARP).

## Structure du Projet

```
src/
├── MockEngine/                  # Projet principal
    ├── Config/                  # Configuration du service
    ├── Controllers/             # API controllers
    ├── Interfaces/              # Interfaces et contrats
    ├── Middleware/              # Middleware pour intercepter les requêtes
    ├── Models/                  # Modèles de données
    ├── Services/                # Logique métier et services
    └── Storage/                 # Implémentations de stockage
```

## Technologies utilisées

- ASP.NET Core 9.0
- Bogus (génération de données fictives)
- JsonSchema.Net (validation de schémas)
- Microsoft.OpenAPI (traitement de spécifications OpenAPI)

## Contribuer

Les contributions sont les bienvenues ! N'hésitez pas à ouvrir une issue ou soumettre une pull request.

## Licence

[MIT](LICENSE)