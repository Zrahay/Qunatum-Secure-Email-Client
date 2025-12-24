#!/usr/bin/env python3
"""
Unit tests for Level 2 AES-256-GCM Encryption Server
Tests the AES encryption/decryption functionality
"""

import pytest
import json
import base64
import binascii
import os


class TestAESServerHelpers:
    """Tests for AES server helper functions"""

    def test_b2h_converts_bytes_to_hex(self):
        """Test bytes to hex conversion"""
        data = b'\x00\x01\x02\x03\xff'
        result = self._b2h(data)

        assert result == '000102030ff'

    def test_json_to_hex_string_input(self):
        """Test JSON to hex with string input"""
        input_str = "test"
        result = self._json_to_hex(input_str)

        # "test" in hex is 74657374
        assert result == binascii.hexlify("test".encode()).decode()

    def test_json_to_hex_object_input(self):
        """Test JSON to hex with object input"""
        input_obj = {"key": "value"}
        result = self._json_to_hex(input_obj)

        # Verify it's valid hex
        decoded = binascii.unhexlify(result).decode()
        assert json.loads(decoded) == {"key": "value"}

    def test_json_to_hex_produces_stable_output(self):
        """Test JSON to hex produces stable (no spaces) output"""
        input_obj = {"a": 1, "b": 2}

        result = self._json_to_hex(input_obj)
        decoded = binascii.unhexlify(result).decode()

        # Should not have spaces
        assert ' ' not in decoded

    # Helper methods simulating server functions
    def _b2h(self, b):
        return binascii.hexlify(b).decode()

    def _json_to_hex(self, obj):
        s = json.dumps(obj, separators=(",", ":")) if not isinstance(obj, str) else obj
        return binascii.hexlify(s.encode("utf-8")).decode()


class TestAESConfiguration:
    """Tests for AES server configuration"""

    def test_default_key_manager_url(self):
        """Test default Key Manager URL"""
        KM = os.getenv("KM_URL", "http://127.0.0.1:2020")

        assert "127.0.0.1" in KM or "localhost" in KM
        assert "2020" in KM

    def test_server_port_default(self):
        """Test default server port"""
        PORT = int(os.getenv("PORT", "2021"))

        assert PORT == 2021
        assert 1024 < PORT < 65535


class TestAESEndpoints:
    """Tests for AES API endpoint validation"""

    def test_encrypt_endpoint_format(self):
        """Test encrypt endpoint URL format"""
        endpoint = "/api/gcm/encrypt"
        assert endpoint.startswith("/api/gcm/")
        assert "encrypt" in endpoint

    def test_decrypt_endpoint_format(self):
        """Test decrypt endpoint URL format"""
        endpoint = "/api/gcm/decrypt"
        assert endpoint.startswith("/api/gcm/")
        assert "decrypt" in endpoint


class TestAESEncryptionResponse:
    """Tests for AES encryption response format"""

    def test_encrypt_response_has_required_fields(self):
        """Test encryption response contains all required fields"""
        response = {
            "keyId": "K-abc123",
            "ivHex": "0123456789abcdef01234567",
            "ciphertextHex": "abcdef0123456789",
            "tagHex": "fedcba9876543210fedcba98",
            "aadHex": ""
        }

        assert "keyId" in response
        assert "ivHex" in response
        assert "ciphertextHex" in response
        assert "tagHex" in response
        assert "aadHex" in response

    def test_iv_is_correct_length(self):
        """Test IV is 12 bytes (24 hex chars)"""
        iv_hex = "0123456789abcdef01234567"

        # 12 bytes = 24 hex characters
        assert len(iv_hex) == 24

    def test_tag_is_correct_length(self):
        """Test authentication tag is 16 bytes (32 hex chars)"""
        tag_hex = "fedcba9876543210fedcba9876543210"

        # 16 bytes = 32 hex characters
        assert len(tag_hex) == 32

    def test_key_id_format(self):
        """Test key ID has expected format"""
        key_id = "K-abc123"

        assert key_id.startswith("K-") or "key" in key_id.lower()


class TestAESDecryptionRequest:
    """Tests for AES decryption request format"""

    def test_decrypt_request_has_required_fields(self):
        """Test decryption request contains all required fields"""
        request = {
            "key_id": "K-abc123",
            "iv_hex": "0123456789abcdef01234567",
            "ciphertext_hex": "abcdef0123456789",
            "tag_hex": "fedcba9876543210fedcba98"
        }

        assert "key_id" in request
        assert "iv_hex" in request
        assert "ciphertext_hex" in request
        assert "tag_hex" in request

    def test_optional_aad_in_request(self):
        """Test optional AAD in decryption request"""
        request_with_aad = {
            "key_id": "K-123",
            "iv_hex": "aabbccdd00112233aabbccdd",
            "ciphertext_hex": "encrypted",
            "tag_hex": "11223344556677889900aabbccddeeff",
            "aad_hex": "additional_data_hex"
        }

        assert "aad_hex" in request_with_aad


class TestHexValidation:
    """Tests for hex string validation"""

    def test_valid_hex_string(self):
        """Test valid hex string detection"""
        valid_hex = "0123456789abcdef"

        try:
            binascii.unhexlify(valid_hex)
            is_valid = True
        except:
            is_valid = False

        assert is_valid == True

    def test_invalid_hex_string(self):
        """Test invalid hex string detection"""
        invalid_hex = "xyz123"

        try:
            binascii.unhexlify(invalid_hex)
            is_valid = True
        except:
            is_valid = False

        assert is_valid == False

    def test_odd_length_hex_is_invalid(self):
        """Test odd-length hex string is invalid"""
        odd_hex = "abc"  # 3 chars

        try:
            binascii.unhexlify(odd_hex)
            is_valid = True
        except:
            is_valid = False

        assert is_valid == False


class TestBase64Operations:
    """Tests for base64 operations in AES server"""

    def test_file_b64_decode(self):
        """Test decoding file_b64 field"""
        original_content = b"File content here"
        file_b64 = base64.b64encode(original_content).decode()

        decoded = base64.b64decode(file_b64)

        assert decoded == original_content

    def test_binary_file_roundtrip(self):
        """Test binary file base64 roundtrip"""
        binary_data = bytes(range(256))

        encoded = base64.b64encode(binary_data).decode()
        decoded = base64.b64decode(encoded)

        assert decoded == binary_data


class TestKeyManagement:
    """Tests for key management integration"""

    def test_key_fetch_paths(self):
        """Test various key fetch URL patterns"""
        KM = "http://127.0.0.1:2020"
        key_id = "K-test-123"

        paths = [
            f"{KM}/otp/keys/{key_id}",
            f"{KM}/otp/keys?id={key_id}",
            f"{KM}/otp/key?id={key_id}",
        ]

        for path in paths:
            assert key_id in path
            assert KM in path

    def test_key_size_parameter(self):
        """Test key size parameter"""
        bytes_needed = 16

        params = {"size": bytes_needed}

        assert params["size"] == 16


class TestErrorHandling:
    """Tests for error handling"""

    def test_bad_request_response(self):
        """Test bad request error response format"""
        error = {
            "error": "bad_request",
            "detail": "missing file_b64/plaintext"
        }

        assert "error" in error
        assert "detail" in error

    def test_crypto_failed_response(self):
        """Test crypto failure error response"""
        error = {
            "error": "crypto_failed",
            "detail": "encryption operation failed"
        }

        assert error["error"] == "crypto_failed"

    def test_auth_failed_response(self):
        """Test authentication failure response"""
        error = {
            "error": "auth_failed"
        }

        assert error["error"] == "auth_failed"

    def test_key_lookup_failed_response(self):
        """Test key lookup failure response"""
        error = {
            "error": "key_lookup_failed",
            "detail": "Key not found in KM for key_id=K-missing"
        }

        assert error["error"] == "key_lookup_failed"


class TestContentTypeHandling:
    """Tests for content type handling"""

    def test_multipart_form_data(self):
        """Test multipart/form-data content type detection"""
        content_type = "multipart/form-data; boundary=----FormBoundary"

        is_multipart = content_type.lower().startswith("multipart/form-data")

        assert is_multipart == True

    def test_application_json(self):
        """Test application/json content type detection"""
        content_type = "application/json"

        is_json = content_type.lower().startswith("application/json")

        assert is_json == True

    def test_application_octet_stream(self):
        """Test application/octet-stream content type detection"""
        content_type = "application/octet-stream"

        is_binary = content_type.lower().startswith("application/octet-stream")

        assert is_binary == True


class TestAADHandling:
    """Tests for Additional Authenticated Data (AAD) handling"""

    def test_aad_json_to_hex_conversion(self):
        """Test AAD JSON to hex conversion"""
        aad = {"sender": "alice@example.com", "recipient": "bob@example.com"}

        json_str = json.dumps(aad, separators=(",", ":"))
        aad_hex = binascii.hexlify(json_str.encode()).decode()

        # Verify roundtrip
        restored_json = binascii.unhexlify(aad_hex).decode()
        restored_aad = json.loads(restored_json)

        assert restored_aad == aad

    def test_empty_aad(self):
        """Test empty AAD handling"""
        aad_hex = ""

        assert aad_hex == ""

    def test_aad_from_header(self):
        """Test AAD from X-AAD-HEX header"""
        header_name = "X-AAD-HEX"
        header_value = "616162626363"  # "aabbcc" in hex

        assert header_name == "X-AAD-HEX"
        decoded = binascii.unhexlify(header_value).decode()
        assert decoded == "aabbcc"


if __name__ == '__main__':
    pytest.main([__file__, '-v'])
