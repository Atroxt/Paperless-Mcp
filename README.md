# Paperless-Mcp

An MCP server that gives AI assistants read access to a Paperless-ngx
instance through its REST API.

## Requirements

- Python 3.10 or later
- A Paperless-ngx instance and an API token with permission to read documents

## Installation and configuration

Install the package in the environment used by your MCP client:

```sh
python -m pip install .
```

Set the Paperless-ngx server URL and API token before starting the server:

```sh
export PAPERLESS_URL="https://paperless.example"
export PAPERLESS_API_TOKEN="your-api-token"
paperless-mcp
```

The server communicates over MCP stdio. Configure your MCP client to launch
`paperless-mcp` with those environment variables.

## Available tools

- `search_documents(query, page, page_size)` searches the document list.
- `get_document(document_id)` returns a document's metadata and extracted
  content.
- `list_tags(page, page_size)` lists tags.
- `list_correspondents(page, page_size)` lists correspondents.
- `list_document_types(page, page_size)` lists document types.

List tools return Paperless-ngx's paginated API response. Page sizes are limited
to 100. The server is read-only and does not upload, modify, or delete documents.

## Development

Run the focused tests with:

```sh
python -m unittest discover -s tests -v
```