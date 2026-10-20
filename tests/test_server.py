import unittest

from paperless_mcp.server import app


class PaperlessServerTests(unittest.IsolatedAsyncioTestCase):
    async def test_registers_paperless_tools(self):
        tools = await app.list_tools()

        self.assertEqual(
            {tool.name for tool in tools},
            {
                "search_documents",
                "get_document",
                "list_tags",
                "list_correspondents",
                "list_document_types",
            },
        )


if __name__ == "__main__":
    unittest.main()
