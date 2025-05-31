# 4. Uruchamianie Aplikacji <a name="uruchamianie-aplikacji"></a>

Aby uruchomić pełny system, musisz uruchomić kilka komponentów: SQL Server, RabbitMQ Server, API Backend oraz Aplikację UI.

## Wymagane Uruchomione Usługi Zewnętrzne

1.  **SQL Server:** Upewnij się, że Twoja instancja SQL Server jest uruchomiona i dostępna dla API.
2.  **RabbitMQ Server:** Upewnij się, że Twój serwer RabbitMQ jest uruchomiony i dostępny dla API.

## Uruchamianie Modułów Aplikacji

### 1. API Backend (`ResourceManagementSystem.API`)

*   **Za pomocą IDE (Visual Studio / Rider):**
    1.  Otwórz solucję `ResourceManagementSystem.sln`.
    2.  Ustaw projekt `ResourceManagementSystem.API` jako projekt startowy.
    3.  Uruchom projekt (zwykle przez naciśnięcie F5 lub przycisku "Run").
    4.  API powinno być dostępne pod adresem zdefiniowanym w `Properties/launchSettings.json` (np. `https://localhost:5003` lub `http://localhost:5002`). Po uruchomieniu powinien otworzyć się Swagger UI.

*   **Za pomocą .NET CLI:**
    1.  Otwórz terminal w katalogu `ResourceManagementSystem.API`.
    2.  Wykonaj polecenie:
        ```bash
        dotnet run
        ```
    3.  Zwróć uwagę na adresy URL wyświetlone w konsoli, pod którymi API nasłuchuje.

### 2. Aplikacja UI (`ResourceManagementSystem.UI.Web`)

*   **Za pomocą IDE (Visual Studio / Rider):**
    1.  Otwórz solucję `ResourceManagementSystem.sln`.
    2.  Ustaw projekt `ResourceManagementSystem.UI.Web` jako projekt startowy (jeśli chcesz uruchamiać tylko UI, lub skonfiguruj wielokrotne uruchamianie projektów, aby API i UI startowały razem).
    3.  Uruchom projekt.
    4.  Aplikacja UI powinna być dostępna pod adresem zdefiniowanym w jej `Properties/launchSettings.json` (np. `http://localhost:5173`).

*   **Za pomocą .NET CLI:**
    1.  Otwórz terminal w katalogu `ResourceManagementSystem.UI.Web`.
    2.  Wykonaj polecenie:
        ```bash
        dotnet run
        ```
    3.  Otwórz przeglądarkę pod adresem URL wyświetlonym w konsoli.

**Kolejność Uruchamiania:**
Zaleca się najpierw uruchomić SQL Server i RabbitMQ, następnie API Backend, a na końcu aplikację UI.

**Dostęp do Aplikacji:**
*   **API Swagger UI:** `https://localhost:<PORT_API>/swagger`
*   **Aplikacja UI:** `http://localhost:<PORT_UI>`

Po uruchomieniu, powinieneś móc zarejestrować się, zalogować i zarządzać zasobami przez interfejs użytkownika. Zmiany powinny być odzwierciedlane przez SignalR (jeśli masz otwartych kilku klientów lub obserwujesz) oraz publikowane do RabbitMQ (możesz to sprawdzić w interfejsie zarządzania RabbitMQ).