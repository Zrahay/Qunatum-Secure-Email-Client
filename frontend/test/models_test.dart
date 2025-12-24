import 'package:flutter_test/flutter_test.dart';
import 'package:frontend/models/user.dart';
import 'package:frontend/models/auth_requests.dart';

void main() {
  group('User Model Tests', () {
    test('User.fromJson creates User with all fields', () {
      // Arrange
      final json = {
        'id': '123e4567-e89b-12d3-a456-426614174000',
        'email': 'test@example.com',
        'name': 'Test User',
        'avatar': 'https://example.com/avatar.jpg',
        'externalEmail': 'external@gmail.com',
        'emailProvider': 'gmail',
      };

      // Act
      final user = User.fromJson(json);

      // Assert
      expect(user.id, '123e4567-e89b-12d3-a456-426614174000');
      expect(user.email, 'test@example.com');
      expect(user.name, 'Test User');
      expect(user.avatar, 'https://example.com/avatar.jpg');
      expect(user.externalEmail, 'external@gmail.com');
      expect(user.emailProvider, 'gmail');
    });

    test('User.fromJson handles null optional fields', () {
      // Arrange
      final json = {
        'id': '123',
        'email': 'test@example.com',
        'name': 'Test User',
        'avatar': null,
        'externalEmail': null,
        'emailProvider': null,
      };

      // Act
      final user = User.fromJson(json);

      // Assert
      expect(user.id, '123');
      expect(user.email, 'test@example.com');
      expect(user.name, 'Test User');
      expect(user.avatar, isNull);
      expect(user.externalEmail, isNull);
      expect(user.emailProvider, isNull);
    });

    test('User.toJson serializes all fields correctly', () {
      // Arrange
      final user = User(
        id: '456',
        email: 'user@example.com',
        name: 'John Doe',
        avatar: 'https://example.com/john.jpg',
        externalEmail: 'john@gmail.com',
        emailProvider: 'gmail',
      );

      // Act
      final json = user.toJson();

      // Assert
      expect(json['id'], '456');
      expect(json['email'], 'user@example.com');
      expect(json['name'], 'John Doe');
      expect(json['avatar'], 'https://example.com/john.jpg');
      expect(json['externalEmail'], 'john@gmail.com');
      expect(json['emailProvider'], 'gmail');
    });

    test('User.toJson serializes null optional fields', () {
      // Arrange
      final user = User(
        id: '789',
        email: 'minimal@example.com',
        name: 'Minimal User',
      );

      // Act
      final json = user.toJson();

      // Assert
      expect(json['id'], '789');
      expect(json['email'], 'minimal@example.com');
      expect(json['name'], 'Minimal User');
      expect(json['avatar'], isNull);
      expect(json['externalEmail'], isNull);
      expect(json['emailProvider'], isNull);
    });

    test('User round-trip (fromJson -> toJson) preserves data', () {
      // Arrange
      final originalJson = {
        'id': 'round-trip-id',
        'email': 'roundtrip@example.com',
        'name': 'Round Trip User',
        'avatar': 'https://example.com/rt.jpg',
        'externalEmail': 'rt@outlook.com',
        'emailProvider': 'outlook',
      };

      // Act
      final user = User.fromJson(originalJson);
      final resultJson = user.toJson();

      // Assert
      expect(resultJson['id'], originalJson['id']);
      expect(resultJson['email'], originalJson['email']);
      expect(resultJson['name'], originalJson['name']);
      expect(resultJson['avatar'], originalJson['avatar']);
      expect(resultJson['externalEmail'], originalJson['externalEmail']);
      expect(resultJson['emailProvider'], originalJson['emailProvider']);
    });
  });

  group('LoginRequest Tests', () {
    test('LoginRequest.toJson serializes correctly', () {
      // Arrange
      final request = LoginRequest(
        email: 'login@example.com',
        password: 'securePassword123',
      );

      // Act
      final json = request.toJson();

      // Assert
      expect(json['email'], 'login@example.com');
      expect(json['password'], 'securePassword123');
      expect(json.length, 2);
    });

    test('LoginRequest handles special characters in password', () {
      // Arrange
      final request = LoginRequest(
        email: 'test@example.com',
        password: r'P@$$w0rd!#$%^&*()',
      );

      // Act
      final json = request.toJson();

      // Assert
      expect(json['password'], r'P@$$w0rd!#$%^&*()');
    });

    test('LoginRequest handles unicode in password', () {
      // Arrange
      final request = LoginRequest(
        email: 'unicode@example.com',
        password: '\u5bc6\u7801123',
      );

      // Act
      final json = request.toJson();

      // Assert
      expect(json['password'], '\u5bc6\u7801123');
    });
  });

  group('RegisterRequest Tests', () {
    test('RegisterRequest.toJson includes all required fields', () {
      // Arrange
      final request = RegisterRequest(
        email: 'register@example.com',
        password: 'newPassword123',
        name: 'New User',
        externalEmail: 'external@gmail.com',
        emailProvider: 'gmail',
        appPassword: '1234567890123456',
      );

      // Act
      final json = request.toJson();

      // Assert
      expect(json['email'], 'register@example.com');
      expect(json['password'], 'newPassword123');
      expect(json['name'], 'New User');
      expect(json['externalEmail'], 'external@gmail.com');
      expect(json['emailProvider'], 'gmail');
      expect(json['appPassword'], '1234567890123456');
      expect(json.containsKey('username'), isFalse); // Not set
    });

    test('RegisterRequest.toJson includes username when provided', () {
      // Arrange
      final request = RegisterRequest(
        email: 'register@example.com',
        password: 'password123',
        name: 'User With Username',
        username: 'customuser',
        externalEmail: 'custom@yahoo.com',
        emailProvider: 'yahoo',
        appPassword: 'abcdefghijklmnop',
      );

      // Act
      final json = request.toJson();

      // Assert
      expect(json['username'], 'customuser');
    });

    test('RegisterRequest.toJson excludes empty username', () {
      // Arrange
      final request = RegisterRequest(
        email: 'register@example.com',
        password: 'password123',
        name: 'User',
        username: '',
        externalEmail: 'user@outlook.com',
        emailProvider: 'outlook',
        appPassword: '1234567890123456',
      );

      // Act
      final json = request.toJson();

      // Assert
      expect(json.containsKey('username'), isFalse);
    });

    test('RegisterRequest supports all email providers', () {
      // Arrange & Act & Assert
      for (final provider in ['gmail', 'yahoo', 'outlook']) {
        final request = RegisterRequest(
          email: 'test@example.com',
          password: 'password',
          name: 'Test',
          externalEmail: 'test@$provider.com',
          emailProvider: provider,
          appPassword: '1234567890123456',
        );

        final json = request.toJson();
        expect(json['emailProvider'], provider);
      }
    });
  });

  group('AuthResponse Tests', () {
    test('AuthResponse.fromJson parses correctly', () {
      // Arrange
      final json = {
        'token': 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.signature',
        'user': {
          'id': 'user-123',
          'email': 'auth@example.com',
          'name': 'Auth User',
          'avatar': null,
          'externalEmail': null,
          'emailProvider': null,
        },
      };

      // Act
      final response = AuthResponse.fromJson(json);

      // Assert
      expect(response.token, startsWith('eyJ'));
      expect(response.user.id, 'user-123');
      expect(response.user.email, 'auth@example.com');
      expect(response.user.name, 'Auth User');
    });

    test('AuthResponse.fromJson handles complete user data', () {
      // Arrange
      final json = {
        'token': 'valid.jwt.token',
        'user': {
          'id': 'complete-user',
          'email': 'complete@example.com',
          'name': 'Complete User',
          'avatar': 'https://example.com/avatar.png',
          'externalEmail': 'complete@gmail.com',
          'emailProvider': 'gmail',
        },
      };

      // Act
      final response = AuthResponse.fromJson(json);

      // Assert
      expect(response.user.avatar, 'https://example.com/avatar.png');
      expect(response.user.externalEmail, 'complete@gmail.com');
      expect(response.user.emailProvider, 'gmail');
    });
  });

  group('Validation Tests', () {
    test('Email format validation', () {
      // Valid email formats
      final validEmails = [
        'test@example.com',
        'user.name@example.com',
        'user+tag@example.com',
        'user@subdomain.example.com',
      ];

      for (final email in validEmails) {
        final emailRegex = RegExp(r'^[\w\.-]+@[\w\.-]+\.\w+$');
        expect(emailRegex.hasMatch(email), isTrue,
            reason: '$email should be valid');
      }
    });

    test('App password length validation', () {
      // App passwords should be 16 characters
      const validAppPassword = '1234567890123456';
      const invalidAppPassword = '12345';

      expect(validAppPassword.length, 16);
      expect(invalidAppPassword.length, isNot(16));
    });
  });
}
