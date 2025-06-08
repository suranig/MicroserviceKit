# 🚀 Szybki Test Nowego Mikroserwisu

## 1. Test automatyczny wszystkich szablonów

```bash
make cli-test-templates
```

## 2. Test pojedynczego szablonu (standardowy)

```bash
# Przejdź do katalogu testowego
cd test_cli/basic
rm -rf *

# Wygeneruj mikrousługę
dotnet run --project ../../src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj -- new TestService --config ../../templates/levels/standard-service.json --output .

# Skompiluj i uruchom
dotnet build
dotnet run --project src/Api/TestService.Api
```

## 3. Test własnego mikroserwisu

```bash
# Stwórz katalog
mkdir ~/my-microservice && cd ~/my-microservice

# Wygeneruj (zmień ścieżki na swoje)
dotnet run --project /path/to/microservice-.net8-ddd/src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj -- new ProductService --config /path/to/microservice-.net8-ddd/templates/levels/standard-service.json --output .

# Skompiluj i uruchom
dotnet build
dotnet run --project src/Api/ProductService.Api
```

## 4. Przetestuj API

```bash
# Health check
curl http://localhost:5000/health

# Swagger UI
# Otwórz: http://localhost:5000/swagger
```

## ✅ Oczekiwane rezultaty

- Generator uruchamia się bez błędów
- Projekt kompiluje się bez błędów  
- API uruchamia się na porcie 5000
- Health check zwraca status 200
- Swagger UI jest dostępne

## 🐛 Jeśli coś nie działa

Sprawdź [TESTING_GUIDE.md](TESTING_GUIDE.md) dla szczegółowych instrukcji rozwiązywania problemów. 