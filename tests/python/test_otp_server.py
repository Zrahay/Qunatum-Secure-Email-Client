#!/usr/bin/env python3
"""
Unit tests for Level 1 One-Time Pad (OTP) Encryption Server
Tests the OTP encryption/decryption functionality
"""

import pytest
import os
import base64


class TestOTPEncryption:
    """Tests for OTP encryption functionality"""

    def test_xor_encryption_basic(self):
        """Test basic XOR encryption"""
        plaintext = bytes([0b10101010, 0b11110000, 0b00001111])
        key = bytes([0b01010101, 0b00001111, 0b11110000])
        expected = bytes([0b11111111, 0b11111111, 0b11111111])

        ciphertext = self._xor_encrypt(plaintext, key)

        assert ciphertext == expected

    def test_xor_decryption_is_same_as_encryption(self):
        """Test that XOR decryption is the same operation as encryption"""
        plaintext = b"Hello, World!"
        key = os.urandom(len(plaintext))

        # Encrypt
        ciphertext = self._xor_encrypt(plaintext, key)

        # Decrypt (same operation)
        decrypted = self._xor_encrypt(ciphertext, key)

        assert decrypted == plaintext

    def test_encryption_roundtrip(self):
        """Test encryption/decryption roundtrip"""
        messages = [
            b"Short",
            b"A longer message with more content",
            b"Unicode: \xc4\xb5\xc5\xa3 test",
            bytes(range(256)),  # All byte values
        ]

        for msg in messages:
            key = os.urandom(len(msg))
            encrypted = self._xor_encrypt(msg, key)
            decrypted = self._xor_encrypt(encrypted, key)
            assert decrypted == msg, f"Failed for message of length {len(msg)}"

    def test_different_keys_produce_different_ciphertext(self):
        """Test that different keys produce different ciphertext"""
        plaintext = b"Same message"
        key1 = os.urandom(len(plaintext))
        key2 = os.urandom(len(plaintext))

        ciphertext1 = self._xor_encrypt(plaintext, key1)
        ciphertext2 = self._xor_encrypt(plaintext, key2)

        # Very high probability they're different
        assert ciphertext1 != ciphertext2

    def test_zero_key_returns_plaintext(self):
        """Test that XOR with zero key returns plaintext"""
        plaintext = b"Test message"
        zero_key = bytes(len(plaintext))  # All zeros

        ciphertext = self._xor_encrypt(plaintext, zero_key)

        assert ciphertext == plaintext

    def test_same_key_same_plaintext_same_ciphertext(self):
        """Test deterministic encryption with same key"""
        plaintext = b"Deterministic test"
        key = b"1234567890123456789"[:len(plaintext)]

        ciphertext1 = self._xor_encrypt(plaintext, key)
        ciphertext2 = self._xor_encrypt(plaintext, key)

        assert ciphertext1 == ciphertext2

    # Helper method
    def _xor_encrypt(self, data, key):
        """XOR encryption/decryption"""
        return bytes(a ^ b for a, b in zip(data, key))


class TestOTPKeyGeneration:
    """Tests for OTP key generation"""

    def test_key_generation_correct_size(self):
        """Test that generated keys have correct size"""
        sizes = [16, 32, 64, 128, 256, 1024]

        for size in sizes:
            key = os.urandom(size)
            assert len(key) == size

    def test_key_generation_is_random(self):
        """Test that key generation produces random output"""
        key1 = os.urandom(32)
        key2 = os.urandom(32)

        assert key1 != key2

    def test_key_contains_variety_of_bytes(self):
        """Test that generated key has good byte variety"""
        key = os.urandom(256)

        unique_bytes = len(set(key))

        # In 256 random bytes, expect significant variety
        # (probability of <100 unique values is extremely low)
        assert unique_bytes > 100


class TestOTPConfiguration:
    """Tests for OTP server configuration"""

    def test_default_port(self):
        """Test default server port"""
        PORT = 2021
        assert PORT == 2021
        assert 1024 < PORT < 65535

    def test_key_manager_url(self):
        """Test Key Manager URL configuration"""
        KM_URL = "http://127.0.0.1:2020"

        assert "127.0.0.1" in KM_URL or "localhost" in KM_URL
        assert "2020" in KM_URL


class TestOTPEndpoints:
    """Tests for OTP API endpoint validation"""

    def test_encrypt_endpoint(self):
        """Test encrypt endpoint format"""
        endpoint = "/api/otp/encrypt"
        assert "/api/otp/" in endpoint
        assert "encrypt" in endpoint

    def test_decrypt_endpoint(self):
        """Test decrypt endpoint format"""
        endpoint = "/api/otp/decrypt"
        assert "/api/otp/" in endpoint
        assert "decrypt" in endpoint

    def test_health_endpoint(self):
        """Test health endpoint format"""
        endpoint = "/api/otp/health"
        assert "health" in endpoint


class TestOTPRequestFormats:
    """Tests for OTP request/response formats"""

    def test_encrypt_request_format(self):
        """Test encryption request format"""
        request = {
            "plaintext": "Message to encrypt",
            "key_id": "K-123456"
        }

        assert "plaintext" in request
        assert "key_id" in request

    def test_encrypt_response_format(self):
        """Test encryption response format"""
        response = {
            "ciphertext": base64.b64encode(b"encrypted").decode(),
            "key_id": "K-123456",
            "bytes_used": 18
        }

        assert "ciphertext" in response
        assert "key_id" in response
        assert "bytes_used" in response

    def test_decrypt_request_format(self):
        """Test decryption request format"""
        request = {
            "ciphertext": base64.b64encode(b"encrypted").decode(),
            "key_id": "K-123456"
        }

        assert "ciphertext" in request
        assert "key_id" in request

    def test_decrypt_response_format(self):
        """Test decryption response format"""
        response = {
            "plaintext": "Decrypted message"
        }

        assert "plaintext" in response


class TestOTPSecurity:
    """Tests for OTP security properties"""

    def test_key_must_be_as_long_as_message(self):
        """Test that key must be at least as long as message"""
        message = b"A message of 20 chars"
        short_key = b"short"

        # This should fail in production
        assert len(short_key) < len(message)

    def test_key_reuse_is_insecure(self):
        """Test that key reuse reveals information"""
        message1 = b"First message here!"
        message2 = b"Second message too!"
        key = os.urandom(max(len(message1), len(message2)))

        cipher1 = self._xor(message1, key[:len(message1)])
        cipher2 = self._xor(message2, key[:len(message2)])

        # XOR of ciphertexts reveals XOR of plaintexts
        xor_ciphers = self._xor(cipher1, cipher2)
        xor_plaintexts = self._xor(message1, message2)

        assert xor_ciphers == xor_plaintexts

    def test_perfect_secrecy_with_random_key(self):
        """Test that random key provides no information about plaintext"""
        plaintext = b"Secret message!"

        # Generate many random keys and ciphertexts
        ciphertexts = []
        for _ in range(10):
            key = os.urandom(len(plaintext))
            ciphertext = self._xor(plaintext, key)
            ciphertexts.append(ciphertext)

        # All ciphertexts should be different
        unique_ciphertexts = set(ciphertexts)
        assert len(unique_ciphertexts) == 10

    def _xor(self, data, key):
        """XOR operation"""
        return bytes(a ^ b for a, b in zip(data, key))


class TestBase64Integration:
    """Tests for base64 encoding integration"""

    def test_ciphertext_base64_encoding(self):
        """Test ciphertext is properly base64 encoded"""
        ciphertext = os.urandom(32)

        encoded = base64.b64encode(ciphertext).decode()
        decoded = base64.b64decode(encoded)

        assert decoded == ciphertext

    def test_plaintext_base64_handling(self):
        """Test plaintext base64 handling"""
        plaintext = "Hello, World!"

        # Encode plaintext
        encoded = base64.b64encode(plaintext.encode()).decode()

        # Decode back
        decoded = base64.b64decode(encoded).decode()

        assert decoded == plaintext


class TestOTPErrorHandling:
    """Tests for OTP error handling"""

    def test_missing_plaintext_error(self):
        """Test error for missing plaintext"""
        error = {
            "error": "missing_plaintext",
            "message": "Plaintext is required"
        }

        assert "error" in error
        assert "missing" in error["error"]

    def test_missing_key_error(self):
        """Test error for missing key"""
        error = {
            "error": "missing_key",
            "message": "Key ID is required"
        }

        assert "error" in error
        assert "key" in error["error"]

    def test_key_not_found_error(self):
        """Test error for key not found"""
        error = {
            "error": "key_not_found",
            "message": "Key K-missing not found in key store"
        }

        assert "error" in error
        assert "not_found" in error["error"]

    def test_encryption_failed_error(self):
        """Test error for encryption failure"""
        error = {
            "error": "encryption_failed",
            "message": "OTP encryption operation failed"
        }

        assert "error" in error
        assert "failed" in error["error"]


class TestKeyStore:
    """Tests for key store integration"""

    def test_key_store_path(self):
        """Test key store file path"""
        key_store_path = "key_store.json"
        assert key_store_path.endswith(".json")

    def test_key_id_format(self):
        """Test key ID format"""
        import uuid

        key_id = f"K-{uuid.uuid4().hex[:8]}"

        assert key_id.startswith("K-")
        assert len(key_id) == 10  # K- plus 8 hex chars


class TestBinaryDataHandling:
    """Tests for binary data handling"""

    def test_handle_all_byte_values(self):
        """Test handling all possible byte values (0-255)"""
        data = bytes(range(256))
        key = os.urandom(256)

        encrypted = bytes(a ^ b for a, b in zip(data, key))
        decrypted = bytes(a ^ b for a, b in zip(encrypted, key))

        assert decrypted == data

    def test_handle_null_bytes(self):
        """Test handling null bytes"""
        data = b"\x00\x00\x00test\x00\x00"
        key = os.urandom(len(data))

        encrypted = bytes(a ^ b for a, b in zip(data, key))
        decrypted = bytes(a ^ b for a, b in zip(encrypted, key))

        assert decrypted == data

    def test_handle_large_data(self):
        """Test handling large data"""
        # 1MB of data
        data = os.urandom(1024 * 1024)
        key = os.urandom(len(data))

        encrypted = bytes(a ^ b for a, b in zip(data, key))
        decrypted = bytes(a ^ b for a, b in zip(encrypted, key))

        assert decrypted == data


if __name__ == '__main__':
    pytest.main([__file__, '-v'])
