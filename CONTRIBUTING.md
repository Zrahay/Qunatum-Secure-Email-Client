# Contributing to QuMail

Thank you for your interest in contributing to QuMail - Quantum Secure Email Client. This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Pull Request Process](#pull-request-process)
- [Reporting Issues](#reporting-issues)
- [Security Vulnerabilities](#security-vulnerabilities)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment. Please:

- Be respectful and considerate in all interactions
- Welcome newcomers and help them get started
- Accept constructive criticism gracefully
- Focus on what is best for the community and project
- Show empathy towards other community members

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/Quantum-Secure-Email-Client.git
   cd Quantum-Secure-Email-Client
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/ORIGINAL_OWNER/Quantum-Secure-Email-Client.git
   ```
4. **Create a branch** for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Setup

### Prerequisites

Ensure you have the following installed:

- PostgreSQL 17+
- .NET 9 SDK
- Flutter SDK 3.8.1+
- Python 3.8+
- Git

### Setting Up the Development Environment

1. **Database Setup**
   ```bash
   psql -U postgres
   CREATE DATABASE quantum_auth;
   \c quantum_auth
   \i database/schema.sql
   \i database/email_schema.sql
   ```

2. **Environment Configuration**
   ```bash
   cp .env.example .env
   # Edit .env with your local settings
   ```

3. **Install Python Dependencies**
   ```bash
   pip install flask flask-cors requests cryptography pytest
   ```

4. **Install .NET Dependencies**
   ```bash
   cd Email_client/QuMail.EmailProtocol
   dotnet restore
   ```

5. **Install Flutter Dependencies**
   ```bash
   cd frontend
   flutter pub get
   ```

### Running the Development Environment

Start all services for local development:

```bash
# Terminal 1 - Key Manager
cd Key_Manager/km && python server.py

# Terminal 2 - OTP API
cd level1 && python otp_api_test.py

# Terminal 3 - AES Server
cd level2new && python server2.py

# Terminal 4 - PQC Server
cd level3 && python pqc_server.py

# Terminal 5 - Backend API
cd Email_client/QuMail.EmailProtocol && dotnet run

# Terminal 6 - Frontend
cd frontend && flutter run -d windows
```

Or use Docker:
```bash
docker-compose -f docker/docker-compose.yml up -d
```

## How to Contribute

### Types of Contributions

We welcome the following types of contributions:

- **Bug Fixes**: Fix issues reported in the issue tracker
- **New Features**: Implement new functionality (please discuss first)
- **Documentation**: Improve README, code comments, or add guides
- **Tests**: Add or improve test coverage
- **Performance**: Optimize existing code
- **Security**: Identify and fix security vulnerabilities

### Before You Start

1. **Check existing issues** to see if someone is already working on it
2. **Open an issue** to discuss significant changes before implementing
3. **Keep changes focused** - one feature or fix per pull request

## Coding Standards

### General Guidelines

- Write clean, readable, and maintainable code
- Follow existing code patterns and conventions
- Add comments for complex logic
- Keep functions small and focused
- Use meaningful variable and function names

### C# (.NET Backend)

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use PascalCase for public members, camelCase for private members
- Use async/await for asynchronous operations
- Add XML documentation for public APIs

Example:
```csharp
/// <summary>
/// Encrypts the message using the specified encryption layer.
/// </summary>
/// <param name="message">The message to encrypt.</param>
/// <param name="key">The encryption key.</param>
/// <returns>The encrypted message.</returns>
public async Task<string> EncryptMessageAsync(string message, string key)
{
    // Implementation
}
```

### Dart (Flutter Frontend)

- Follow [Effective Dart](https://dart.dev/guides/language/effective-dart) guidelines
- Use lowerCamelCase for variables and functions
- Use UpperCamelCase for classes and types
- Prefer const constructors where possible

Example:
```dart
class EmailService {
  final ApiClient _apiClient;

  EmailService(this._apiClient);

  Future<List<Email>> fetchEmails() async {
    // Implementation
  }
}
```

### Python (Crypto Services)

- Follow [PEP 8](https://www.python.org/dev/peps/pep-0008/) style guide
- Use snake_case for functions and variables
- Use PascalCase for classes
- Add docstrings to functions and classes

Example:
```python
def encrypt_with_otp(plaintext: str, key: bytes) -> bytes:
    """
    Encrypt plaintext using One-Time Pad encryption.

    Args:
        plaintext: The message to encrypt.
        key: The encryption key (must be same length as plaintext).

    Returns:
        The encrypted ciphertext as bytes.
    """
    # Implementation
```

## Pull Request Process

### Creating a Pull Request

1. **Ensure your code follows the coding standards**
2. **Write or update tests** for your changes
3. **Run all tests** and ensure they pass:
   ```bash
   # .NET tests
   cd Email_client/QuMail.EmailProtocol.Tests
   dotnet test

   # Flutter tests
   cd frontend
   flutter test

   # Python tests
   cd tests/python
   pytest -v
   ```
4. **Update documentation** if needed
5. **Commit your changes** with a clear message:
   ```bash
   git commit -m "Add feature: brief description of changes"
   ```
6. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```
7. **Open a Pull Request** against the `main` branch

### Pull Request Guidelines

- **Title**: Use a clear, descriptive title
- **Description**: Explain what changes you made and why
- **Link Issues**: Reference any related issues (e.g., "Fixes #123")
- **Screenshots**: Include screenshots for UI changes
- **Testing**: Describe how you tested your changes

### Pull Request Template

```markdown
## Description
Brief description of the changes.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Other (describe)

## Related Issues
Fixes #(issue number)

## Testing
Describe how you tested your changes.

## Checklist
- [ ] My code follows the project's coding standards
- [ ] I have added tests for my changes
- [ ] All existing tests pass
- [ ] I have updated documentation as needed
```

### Review Process

1. A maintainer will review your PR
2. Address any feedback or requested changes
3. Once approved, your PR will be merged
4. Your contribution will be credited in the project

## Reporting Issues

### Before Reporting

1. **Search existing issues** to avoid duplicates
2. **Check the troubleshooting guide** in README.md
3. **Verify it's reproducible** on the latest version

### Creating an Issue

Use the following template:

```markdown
## Description
Clear description of the issue.

## Steps to Reproduce
1. Step one
2. Step two
3. ...

## Expected Behavior
What you expected to happen.

## Actual Behavior
What actually happened.

## Environment
- OS: [e.g., Windows 11, Ubuntu 22.04]
- .NET Version: [e.g., 9.0]
- Flutter Version: [e.g., 3.8.1]
- Python Version: [e.g., 3.11]

## Logs/Screenshots
Include any relevant logs or screenshots.
```

## Security Vulnerabilities

If you discover a security vulnerability, please:

1. **DO NOT** open a public issue
2. Email the maintainers directly with details
3. Include steps to reproduce the vulnerability
4. Allow time for the issue to be addressed before disclosure

Security issues will be treated with high priority and addressed as quickly as possible.

---

Thank you for contributing to QuMail! Your contributions help make secure communication accessible to everyone.
