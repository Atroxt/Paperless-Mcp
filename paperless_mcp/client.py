"""Small HTTP client for the Paperless-ngx REST API."""

import json
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, urlsplit
from urllib.request import Request, urlopen


class PaperlessClient:
    def __init__(self, base_url: str, token: str, timeout: float = 30):
        parsed_url = urlsplit(base_url.rstrip("/"))
        if parsed_url.scheme not in {"http", "https"} or not parsed_url.netloc:
            raise ValueError("PAPERLESS_URL must be an absolute HTTP(S) URL")
        if parsed_url.username or parsed_url.password or parsed_url.query or parsed_url.fragment:
            raise ValueError("PAPERLESS_URL must not contain credentials, a query, or a fragment")
        if not token:
            raise ValueError("PAPERLESS_API_TOKEN must not be empty")

        self.base_url = base_url.rstrip("/")
        self.token = token
        self.timeout = timeout

    def _get_json(self, endpoint: str, params: dict | None = None) -> dict:
        url = f"{self.base_url}/api/{endpoint.lstrip('/')}"
        if params:
            url = f"{url}?{urlencode(params)}"
        request = Request(
            url,
            headers={
                "Accept": "application/json",
                "Authorization": f"Token {self.token}",
            },
        )
        try:
            with urlopen(request, timeout=self.timeout) as response:
                return json.load(response)
        except HTTPError as error:
            raise RuntimeError(f"Paperless-ngx API returned HTTP {error.code}") from None
        except URLError as error:
            raise RuntimeError(f"Could not reach Paperless-ngx API: {error.reason}") from None
        except json.JSONDecodeError as error:
            raise RuntimeError("Paperless-ngx API returned invalid JSON") from error

    @staticmethod
    def _validate_pagination(page: int, page_size: int) -> None:
        if page < 1:
            raise ValueError("page must be at least 1")
        if not 1 <= page_size <= 100:
            raise ValueError("page_size must be between 1 and 100")

    def search_documents(self, query: str = "", page: int = 1, page_size: int = 25) -> dict:
        self._validate_pagination(page, page_size)
        return self._get_json(
            "documents/",
            {"query": query, "page": page, "page_size": page_size},
        )

    def get_document(self, document_id: int) -> dict:
        if document_id < 1:
            raise ValueError("document_id must be at least 1")
        return self._get_json(f"documents/{document_id}/")

    def list_resource(self, resource: str, page: int = 1, page_size: int = 100) -> dict:
        if resource not in {"tags", "correspondents", "document_types"}:
            raise ValueError("unsupported Paperless-ngx resource")
        self._validate_pagination(page, page_size)
        return self._get_json(
            f"{resource}/",
            {"page": page, "page_size": page_size},
        )
