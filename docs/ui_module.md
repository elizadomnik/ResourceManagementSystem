# 7. Moduł Interfejsu Użytkownika (`ResourceManagementSystem.UI.Web`) <a name="moduł-interfejsu-użytkownika"></a>

Interfejs użytkownika (UI) to aplikacja webowa Blazor Server, która umożliwia użytkownikom interakcję z systemem zarządzania zasobami.

## Główne Funkcje
*   Rejestracja nowych użytkowników.
*   Logowanie istniejących użytkowników.
*   Wyświetlanie listy zarządzanych zasobów.
*   Tworzenie nowych zasobów.
*   Edycja istniejących zasobów.
*   Usuwanie zasobów.
*   Automatyczna aktualizacja listy zasobów w czasie rzeczywistym dzięki integracji z SignalR.

## Technologie i Architektura
*   **Framework:** Blazor Server (.NET 8)
*   **Renderowanie:** Interaktywne po stronie serwera (`InteractiveServer`).
*   **Komunikacja z API:** Za pomocą `HttpClient` do endpointów REST API.
*   **Komunikacja w czasie rzeczywistym:** Klient SignalR łączący się z `ResourceHub` w API.
*   **Zarządzanie Stanem Uwierzytelnienia:**
    *   Token JWT otrzymany z API jest przechowywany w `localStorage` przeglądarki.
    *   `AuthUIService` zarządza stanem logowania, interakcją z `localStorage` i komunikacją z endpointami autoryzacji API.
*   **Zarządzanie Danymi Zasobów:**
    *   `ResourceUIService` odpowiada za pobieranie danych zasobów z API, zarządzanie połączeniem SignalR i aktualizowanie lokalnej listy zasobów na podstawie powiadomień z huba.
*   **Struktura Komponentów:**
    *   `Pages/`: Zawiera główne strony aplikacji (np. `LoginPage.razor`, `RegisterPage.razor`, `ResourcesPage.razor`).
    *   `Layout/`: Zawiera komponenty layoutu (np. `MainLayout.razor`, `NavMenu.razor`).
    *   `Shared/` (jeśli istnieje): Dla współdzielonych komponentów.
*   **Serwisy UI:**
    *   `AuthUIService`: Obsługa logiki logowania, rejestracji, wylogowywania, zarządzanie tokenem w `localStorage`.
    *   `LocalStorageService`: Serwis pomocniczy do interakcji z `localStorage` przeglądarki za pomocą `IJSRuntime`.
    *   `ResourceUIService`: Pobieranie i wyświetlanie zasobów, integracja z SignalR dla aktualizacji w czasie rzeczywistym.

## Kluczowe Przepływy Użytkownika

1.  **Rejestracja/Logowanie:**
    *   Użytkownik wypełnia formularz na stronie `/register` lub `/login`.
    *   `AuthUIService` wysyła dane do API.
    *   Po pomyślnej odpowiedzi, token JWT i nazwa użytkownika są zapisywane w `localStorage`.
    *   Stan aplikacji jest aktualizowany, użytkownik jest przekierowywany na stronę zasobów.
2.  **Wyświetlanie Zasobów:**
    *   `ResourcesPage` przy inicjalizacji (jeśli użytkownik jest zalogowany) używa `ResourceUIService` do pobrania listy zasobów z API.
    *   `ResourceUIService` nawiązuje połączenie z `ResourceHub` (SignalR).
    *   Lista zasobów jest wyświetlana w tabeli.
3.  **Operacje CRUD na Zasobach:**
    *   Użytkownik wykonuje akcję (np. kliknięcie "Utwórz", "Edytuj", "Usuń").
    *   Odpowiednie metody w `ResourceUIService` są wywoływane, które komunikują się z API.
    *   Po pomyślnej operacji w API:
        *   API wysyła powiadomienie przez SignalR.
        *   `ResourceUIService` odbiera powiadomienie i aktualizuje lokalną listę zasobów.
        *   Komponent `ResourcesPage` jest automatycznie odświeżany, pokazując zmiany.

## Uruchamianie Modułu UI
Patrz sekcja [Uruchamianie Aplikacji](running_the_app.md#uruchamianie-modułów-aplikacji). Pamiętaj, że API Backend musi być uruchomione, aby UI mogło poprawnie funkcjonować.