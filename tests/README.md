# QuMail - Test Suite

This directory contains comprehensive unit and integration tests for the Quantum-Secure Email Client (QuMail).

## Test Structure

```
tests/
├── README.md                    # This file
├── backend/                     # Additional .NET backend tests
├── frontend/                    # Additional Flutter/Dart tests
├── python/                      # Python crypto service tests
└── integration/                 # End-to-end integration tests
```

## Running Tests

### Backend Tests (.NET)

```bash
# Navigate to the backend test project
cd Email_client/QuMail.EmailProtocol.Tests

# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~SecureKeyManagerTests"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests (Flutter/Dart)

```bash
# Navigate to the frontend directory
cd frontend

# Run all tests
flutter test

# Run with coverage
flutter test --coverage

# Run specific test file
flutter test test/auth_service_test.dart
```

### Python Tests

```bash
# Navigate to the tests/python directory
cd tests/python

# Install test dependencies
pip install pytest pytest-cov pytest-mock flask-testing

# Run all Python tests
pytest

# Run with coverage
pytest --cov=. --cov-report=html

# Run specific test file
pytest test_pqc_server.py -v
```

### Integration Tests

```bash
# Ensure all services are running first (see main README.md)

# Run integration tests
cd tests/integration
python run_integration_tests.py
```

## Test Categories

### Unit Tests
- **SecureKeyManagerTests**: Tests for quantum key generation and management
- **Level1OTPTests**: One-Time Pad encryption/decryption tests
- **Level2AESTests**: AES-256-GCM encryption tests
- **Level3PQCTests**: Post-Quantum Cryptography tests
- **AuthServiceTests**: Authentication and JWT handling tests
- **EmailServiceTests**: Email composition and encryption tests

### Integration Tests
- **FullEncryptionFlowTests**: End-to-end 3-layer encryption tests
- **KeyExchangeTests**: Quantum key exchange workflow tests
- **APIEndpointTests**: REST API endpoint validation tests

## Test Coverage Goals

| Component | Target Coverage |
|-----------|----------------|
| Backend Services | 80% |
| Frontend Services | 75% |
| Crypto Services | 90% |
| Controllers | 70% |

## Writing New Tests

### Backend (.NET) Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;

namespace QuMail.EmailProtocol.Tests;

public class NewFeatureTests
{
    [Fact]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange

        // Act

        // Assert
    }

    [Theory]
    [InlineData("input1", "expected1")]
    [InlineData("input2", "expected2")]
    public void MethodName_MultipleInputs_ReturnsExpected(string input, string expected)
    {
        // Test implementation
    }
}
```

### Frontend (Dart) Test Template

```dart
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('FeatureName Tests', () {
    setUp(() {
      // Setup code
    });

    test('should do something when condition', () {
      // Arrange

      // Act

      // Assert
      expect(actual, expected);
    });
  });
}
```

### Python Test Template

```python
import pytest
from unittest.mock import Mock, patch

class TestFeatureName:
    def setup_method(self):
        """Setup for each test"""
        pass

    def test_method_should_do_something(self):
        # Arrange

        # Act

        # Assert
        assert result == expected

    @pytest.mark.parametrize("input,expected", [
        ("input1", "expected1"),
        ("input2", "expected2"),
    ])
    def test_method_with_multiple_inputs(self, input, expected):
        # Test implementation
        pass
```

## Continuous Integration

Tests are automatically run on:
- Every push to `main` branch
- Every pull request
- Nightly scheduled builds

## Troubleshooting

### Common Issues

1. **Database connection errors**: Ensure PostgreSQL is running and configured
2. **Service unavailable**: Start all crypto services before running integration tests
3. **Flutter test failures**: Run `flutter pub get` to ensure dependencies are installed
4. **Python import errors**: Ensure you're in the correct virtual environment
