import io
import json
import unittest
from unittest.mock import patch

from paperless_mcp.client import PaperlessClient


class PaperlessClientTests(unittest.TestCase):
    def setUp(self):
        self.client = PaperlessClient("https://paperless.example", "test-token")

    @patch("paperless_mcp.client.urlopen")
    def test_search_documents_sends_query_and_authentication(self, urlopen):
        urlopen.return_value.__enter__.return_value = io.BytesIO(
            json.dumps({"count": 0, "results": []}).encode()
        )

        result = self.client.search_documents("annual report", page=2, page_size=10)

        request = urlopen.call_args.args[0]
        self.assertEqual(
            request.full_url,
            "https://paperless.example/api/documents/?query=annual+report&page=2&page_size=10",
        )
        self.assertEqual(request.get_header("Authorization"), "Token test-token")
        self.assertEqual(result, {"count": 0, "results": []})

    @patch("paperless_mcp.client.urlopen")
    def test_get_document_uses_document_endpoint(self, urlopen):
        urlopen.return_value.__enter__.return_value = io.BytesIO(b'{"id": 12}')

        self.assertEqual(self.client.get_document(12), {"id": 12})
        self.assertEqual(
            urlopen.call_args.args[0].full_url,
            "https://paperless.example/api/documents/12/",
        )

    def test_rejects_invalid_configuration_and_pagination(self):
        with self.assertRaises(ValueError):
            PaperlessClient("file:///tmp/paperless", "test-token")
        with self.assertRaises(ValueError):
            PaperlessClient("https://paperless.example", "")
        with self.assertRaises(ValueError):
            self.client.search_documents(page_size=101)
        with self.assertRaises(ValueError):
            self.client.get_document(0)


if __name__ == "__main__":
    unittest.main()
