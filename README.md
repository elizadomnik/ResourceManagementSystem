# Resource Management System (C# .NET)

Witamy w projekcie Systemu Zarządzania Zasobami! Jest to aplikacja rozproszona napisana w języku C# z wykorzystaniem platformy .NET, symulująca system zarządzania zasobami w firmie.

## O Projekcie

Projekt ten został stworzony jako zadanie mające na celu demonstrację budowy aplikacji wielowarstwowej, z naciskiem na:
*   Architekturę opartą na mikroserwisach/modułach (API, UI, Synchronizacja).
*   Wykorzystanie ASP.NET Core dla backendu i Blazor Server dla interfejsu użytkownika.
*   Komunikację w czasie rzeczywistym za pomocą SignalR.
*   Asynchroniczną wymianę wiadomości z użyciem RabbitMQ.
*   Uwierzytelnianie i autoryzację oparte na tokenach JWT.
*   Interakcję z bazą danych SQL Server poprzez Entity Framework Core.
*   Praktyczne zastosowanie Git do zarządzania kodem źródłowym, współpracy i dokumentacji.
*   Implementację różnych rodzajów testów (jednostkowych, integracyjnych, UI).

## Kluczowe Technologie

*   **Backend:** C#, ASP.NET Core, Entity Framework Core, SignalR, RabbitMQ
*   **Frontend:** Blazor Server
*   **Baza Danych:** SQL Server
*   **Testowanie:** xUnit, Moq, Playwright
*   **Kontrola Wersji:** Git

## Dokumentacja

Szczegółowa dokumentacja projektu, w tym opis architektury, instrukcje instalacji, konfiguracji, uruchamiania oraz opis poszczególnych modułów, znajduje się w folderze [`docs/`](./docs/README.md).

**Sugerujemy zacząć od przeczytania [Wprowadzenia do dokumentacji](./docs/introduction.md).**

## Struktura Repozytorium (Główne Foldery)

*   `ResourceManagementSystem.API/`: Projekt backendu API.
*   `ResourceManagementSystem.Core/`: Biblioteka z rdzeniowymi modelami i interfejsami.
*   `ResourceManagementSystem.UI.Web/`: Projekt interfejsu użytkownika Blazor Server.
*   `ResourceManagementSystem.API.Tests/`: Testy jednostkowe i integracyjne dla API.
*   `ResourceManagementSystem.UI.Tests/`: Testy automatyczne UI (Playwright).
*   `docs/`: Pełna dokumentacja projektu.

## Jak Zacząć?

1.  Sklonuj repozytorium:
    ```bash
    cd ResourceManagementSystemCSharp
    ```
2.  Zapoznaj się z [Instrukcją Instalacji i Konfiguracji](./docs/installation.md).
3.  Postępuj zgodnie z instrukcjami w [Uruchamianie Aplikacji](./docs/running_the_app.md).