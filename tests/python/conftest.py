#!/usr/bin/env python3
"""
Pytest configuration and fixtures for QuMail Python tests
"""

import pytest
import os
import sys

# Add project paths to PYTHONPATH for imports
PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(__file__)))
sys.path.insert(0, PROJECT_ROOT)
sys.path.insert(0, os.path.join(PROJECT_ROOT, 'level1'))
sys.path.insert(0, os.path.join(PROJECT_ROOT, 'level2new'))
sys.path.insert(0, os.path.join(PROJECT_ROOT, 'level3'))
sys.path.insert(0, os.path.join(PROJECT_ROOT, 'Key_Manager', 'km'))


@pytest.fixture
def sample_plaintext():
    """Provide sample plaintext for encryption tests"""
    return "Hello, Quantum World! This is a test message."


@pytest.fixture
def sample_key_32():
    """Provide a sample 32-byte key"""
    return os.urandom(32)


@pytest.fixture
def sample_key_16():
    """Provide a sample 16-byte key"""
    return os.urandom(16)


@pytest.fixture
def sample_iv():
    """Provide a sample 12-byte IV for AES-GCM"""
    return os.urandom(12)


@pytest.fixture
def sample_user_data():
    """Provide sample user data for auth tests"""
    return {
        'email': 'test@example.com',
        'name': 'Test User',
        'password': 'SecurePassword123!'
    }


@pytest.fixture
def sample_email_data():
    """Provide sample email data for encryption tests"""
    return {
        'sender': 'alice@example.com',
        'recipient': 'bob@example.com',
        'subject': 'Test Email Subject',
        'body': 'This is the email body content.',
        'attachments': []
    }


@pytest.fixture
def mock_pqc_keypair():
    """Provide mock PQC keypair for testing"""
    import base64
    return {
        'publicKey': base64.b64encode(os.urandom(1184)).decode(),  # Kyber-512 public key size
        'privateKey': base64.b64encode(os.urandom(2400)).decode()  # Kyber-512 private key size
    }


# Test markers
def pytest_configure(config):
    """Configure custom pytest markers"""
    config.addinivalue_line(
        "markers", "slow: marks tests as slow (deselect with '-m \"not slow\"')"
    )
    config.addinivalue_line(
        "markers", "integration: marks tests as integration tests"
    )
    config.addinivalue_line(
        "markers", "unit: marks tests as unit tests"
    )
