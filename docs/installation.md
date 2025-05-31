# 3. Instalacja i Konfiguracja <a name="instalacja-i-konfiguracja"></a>

## Wymagania Wstępne
Przed uruchomieniem projektu upewnij się, że masz zainstalowane następujące oprogramowanie:

*   **.NET SDK:** Wersja 8.0 (lub nowsza, zgodna z projektem).
*   **IDE:** Visual Studio 2022, JetBrains Rider lub VS Code z odpowiednimi rozszerzeniami C#.
*   **Docker Desktop:** (Zalecane do łatwego uruchamiania SQL Server i RabbitMQ). Alternatywnie, możesz zainstalować te usługi bezpośrednio w systemie.
*   **Git:** Do sklonowania repozytorium.
*   **Node.js i npm (opcjonalnie):** Jeśli planujesz rozwijać frontend z użyciem narzędzi JavaScript/CSS lub dla Playwright (choć Playwright .NET instaluje własne przeglądarki).

## Kroki Instalacyjne

1.  **Klonowanie Repozytorium:**
    Sklonuj repozytorium na swój lokalny dysk i przejdź do katalogu projektu:
    ```bash
    git clone https://github.com/elizadomnik/ResourceManagementSystem.githttps://github.com/elizadomnik/ResourceManagementSystem.git
    cd ResourceManagementSystemCSharp 
    ```

2.  **Uruchamianie Usług Zależnych (SQL Server & RabbitMQ) za pomocą Dockera (Zalecane):**

    *   **Uruchomienie SQL Server w Dockerze:**
        Otwórz terminal i wykonaj następujące polecenie, aby pobrać i uruchomić kontener SQL Server (Developer Edition):
        ```bash
        docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrong(!)Password" \
        -p 1433:1433 --name mssql_dev -d \
        mcr.microsoft.com/mssql/server:2022-latest-ubuntu 
        ```
        *   Zastąp `yourStrong(!)Password` silnym hasłem dla użytkownika `sa`. **Zapamiętaj to hasło!**
        *   To polecenie uruchomi SQL Server nasłuchujący na porcie `1433` Twojego komputera.
        *   Obraz `:2022-latest-ubuntu` jest przykładem; możesz wybrać inny tag, np. `:2019-latest-ubuntu` lub wersję dla Windows, jeśli preferujesz.

    *   **Uruchomienie RabbitMQ w Dockerze:**
        Otwórz terminal i wykonaj następujące polecenie, aby pobrać i uruchomić kontener RabbitMQ z interfejsem zarządzania:
        ```bash
        docker run -d --hostname my-rabbit --name rabbitmq_dev \
        -p 5672:5672 -p 15672:15672 \
        rabbitmq:3-management
        ```
        *   Interfejs zarządzania RabbitMQ będzie dostępny pod adresem `http://localhost:15672` (login: `guest`, hasło: `guest`).
        *   RabbitMQ będzie nasłuchiwał na połączenia AMQP na porcie `5672`.

    *Alternatywnie, jeśli nie używasz Dockera, zainstaluj SQL Server i RabbitMQ Server bezpośrednio w swoim systemie operacyjnym zgodnie z ich oficjalną dokumentacją.*

3.  **Konfiguracja Bazy Danych (SQL Server):**
    *   Otwórz plik `ResourceManagementSystem.API/appsettings.json` (lub `appsettings.Development.json`).
    *   Znajdź sekcję `ConnectionStrings` i zmodyfikuj `DefaultConnection`.
        *   **Jeśli używasz SQL Server w Dockerze (jak wyżej):**
            ```json
            "ConnectionStrings": {
              "DefaultConnection": "Server=localhost,1433;Database=ResourceManagementSystemDB_Dev;User ID=sa;Password=yourStrong(!)Password;MultipleActiveResultSets=true;TrustServerCertificate=True"
            }
            ```
            Zastąp `yourStrong(!)Password` hasłem ustawionym przy uruchamianiu kontenera Dockera. `TrustServerCertificate=True` jest często potrzebne dla połączeń z SQL Server w Dockerze z domyślnym certyfikatem.
        *   **Jeśli używasz lokalnej instancji SQL Server (nie w Dockerze):**
            ```json
            "ConnectionStrings": {
              "DefaultConnection": "Server=NAZWA_TWOJEGO_SERWERA;Database=ResourceManagementSystemDB_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
            }
            ```
            Zastąp `NAZWA_TWOJEGO_SERWERA` (np. `localhost`, `(localdb)\\mssqllocaldb`, `localhost\\SQLEXPRESS`).
    *   **Migracje Bazy Danych:**
        Otwórz terminal w katalogu `ResourceManagementSystem.API` i wykonaj polecenie, aby zastosować migracje i utworzyć bazę danych:
        ```bash
        dotnet ef database update
        ```

4.  **Konfiguracja RabbitMQ:**
    *   Otwórz plik `ResourceManagementSystem.API/appsettings.json`.
    *   Sprawdź lub dostosuj sekcję `RabbitMQ`. Jeśli używasz domyślnych ustawień kontenera Docker RabbitMQ, poniższa konfiguracja powinna być poprawna:
        ```json
        "RabbitMQ": {
          "HostName": "localhost",
          "UserName": "guest", 
          "Password": "guest"
        }
        ```

5.  **Konfiguracja API (JWT):**
    *   Otwórz plik `ResourceManagementSystem.API/appsettings.json`.
    *   W sekcji `Jwt` zmodyfikuj `Key`. Powinien to być długi, losowy i tajny ciąg znaków. **Nie używaj domyślnego klucza w środowisku produkcyjnym!**
        ```json
        "Jwt": {
          "Key": "TWOJ_BARDZO_DLUGI_I_TAJNY_KLUCZ_JWT_DO_GENEROWANIA_PODPISOW",
          "Issuer": "ResourceManagementSystem.API",
          "Audience": "ResourceManagementSystem.Clients",
          "ExpiryHours": "1"
        }
        ```

6.  **Konfiguracja Aplikacji UI (Blazor):**
    *   Otwórz plik `ResourceManagementSystem.UI.Web/appsettings.json` (lub `appsettings.Development.json`).
    *   Upewnij się, że `ApiSettings:BaseUrl` (lub `ApiBaseUrl`, zależnie od klucza użytego w kodzie) wskazuje na poprawny adres URL Twojego działającego API Backend. Domyślnie, jeśli API i UI działają lokalnie, może to być np.:
        ```json
        "ApiSettings": { // Lub ApiBaseUrl bezpośrednio
          "BaseUrl": "https://localhost:5003" // Zmień port, jeśli API działa na innym i użyj http/https odpowiednio
        }
        ```

7.  **Przywrócenie Zależności NuGet:**
    Otwórz solucję w IDE lub wykonaj w głównym katalogu solucji:
    ```bash
    dotnet restore
    ```

8.  **Instalacja Sterowników Playwright (dla testów UI):**
    Jeśli planujesz uruchamiać testy UI, przejdź do katalogu projektu `ResourceManagementSystem.UI.Tests` i wykonaj:
    ```bash
    # PowerShell
    pwsh bin/Debug/net8.0/playwright.ps1 install
    # lub dla CMD
    # .\bin\Debug\net8.0\playwright.ps1 install
    # lub dla Bash
    # bash ./bin/Debug/net8.0/playwright.sh install
    ```
    (Dostosuj ścieżkę do wersji .NET i systemu operacyjnego).

Po wykonaniu tych kroków, projekt powinien być gotowy do zbudowania i uruchomienia.