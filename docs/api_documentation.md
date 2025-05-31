# 5. Dokumentacja API <a name="dokumentacja-api"></a>

Dokumentacja API Backend (`ResourceManagementSystem.API`) jest generowana automatycznie przy użyciu Swagger (OpenAPI).

## Dostęp do Swagger UI
Po uruchomieniu projektu API, interfejs Swagger UI jest zazwyczaj dostępny pod adresem:
`https://localhost:<PORT_API>/swagger`

Gdzie `<PORT_API>` to port, na którym działa Twoje API (np. 5003 dla HTTPS lub 5002 dla HTTP, zgodnie z `launchSettings.json`).

## Główne Endpointy

### Moduł Autoryzacji (`/Auth`)
*   **`POST /Auth/Register`**: Rejestruje nowego użytkownika.
    *   Ciało żądania: `UserRegisterDto` (`username`, `email`, `password`).
    *   Odpowiedź (sukces): `200 OK` z `AuthResponseDto` (zawierającym m.in. token JWT).
*   **`POST /Auth/Login`**: Loguje istniejącego użytkownika.
    *   Ciało żądania: `UserLoginDto` (`email`, `password`).
    *   Odpowiedź (sukces): `200 OK` z `AuthResponseDto`.
*   **`POST /Auth/Logout`**: Wylogowuje użytkownika (głównie po stronie klienta, serwer może unieważnić sesję/token).
    *   Wymaga autoryzacji (nagłówek `Authorization: Bearer <token>`).
    *   Odpowiedź (sukces): `200 OK`.

### Moduł Zasobów (`/Resources`)
Wszystkie endpointy tego modułu wymagają autoryzacji (nagłówek `Authorization: Bearer <token>`).

*   **`GET /Resources`**: Pobiera listę wszystkich zasobów.
    *   Odpowiedź: Tablica `ResourceDto`.
*   **`GET /Resources/{id}`**: Pobiera szczegóły konkretnego zasobu.
    *   Odpowiedź: `ResourceDto` lub `404 Not Found`.
*   **`POST /Resources`**: Tworzy nowy zasób.
    *   Ciało żądania: `CreateResourceDto`.
    *   Odpowiedź (sukces): `201 Created` z utworzonym `ResourceDto`.
*   **`PUT /Resources/{id}`**: Aktualizuje istniejący zasób.
    *   Ciało żądania: `UpdateResourceDto`.
    *   Odpowiedź (sukces): `200 OK` z zaktualizowanym `ResourceDto` lub `204 No Content`.
*   **`DELETE /Resources/{id}`**: Usuwa zasób.
    *   Wymaga roli "Admin".
    *   Odpowiedź (sukces): `204 No Content`.

## Uwierzytelnianie
API używa tokenów JWT (JSON Web Tokens) do uwierzytelniania. Po pomyślnym zalogowaniu lub rejestracji, API zwraca token JWT. Klient powinien przechowywać ten token w `localStorage` i dołączać go do każdego żądania do chronionych endpointów w nagłówku `Authorization` jako `Bearer <token>`.
