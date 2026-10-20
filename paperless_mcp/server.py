"""MCP tools for interacting with Paperless-ngx."""

import os

from mcp.server.fastmcp import FastMCP

from paperless_mcp.client import PaperlessClient


app = FastMCP("Paperless-ngx")


def _client() -> PaperlessClient:
    base_url = os.environ.get("PAPERLESS_URL")
    token = os.environ.get("PAPERLESS_API_TOKEN")
    if not base_url or not token:
        raise RuntimeError("Set PAPERLESS_URL and PAPERLESS_API_TOKEN before starting the server")
    return PaperlessClient(base_url, token)


@app.tool()
def search_documents(query: str = "", page: int = 1, page_size: int = 25) -> dict:
    """Search Paperless-ngx documents by text and return one page of results."""
    return _client().search_documents(query, page, page_size)


@app.tool()
def get_document(document_id: int) -> dict:
    """Get document metadata and extracted content by its Paperless-ngx ID."""
    return _client().get_document(document_id)


@app.tool()
def list_tags(page: int = 1, page_size: int = 100) -> dict:
    """List Paperless-ngx tags."""
    return _client().list_resource("tags", page, page_size)


@app.tool()
def list_correspondents(page: int = 1, page_size: int = 100) -> dict:
    """List Paperless-ngx correspondents."""
    return _client().list_resource("correspondents", page, page_size)


@app.tool()
def list_document_types(page: int = 1, page_size: int = 100) -> dict:
    """List Paperless-ngx document types."""
    return _client().list_resource("document_types", page, page_size)


def main() -> None:
    app.run()


if __name__ == "__main__":
    main()
