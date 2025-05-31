# 8. Testowanie <a name="testowanie"></a>

W projekcie zaimplementowano różne rodzaje testów w celu zapewnienia jakości i stabilności kodu.

## Testy Jednostkowe (`ResourceManagementSystem.API.Tests`)

*   **Framework:** xUnit
*   **Biblioteka Mockująca:** Moq
*   **Cel:** Testowanie poszczególnych jednostek (klas, metod) w izolacji, głównie w module API.
*   **Pokrycie:**
    *   **Kontrolery API (`AuthControllerTests.cs`, `ResourcesControllerTests.cs`):**
        *   Sprawdzanie poprawności zwracanych `IActionResult` (np. `OkObjectResult`, `BadRequestResult`, `NotFoundResult`).
        *   Weryfikacja, czy kontrolery poprawnie wywołują metody odpowiednich serwisów (z użyciem mocków).
        *   Testowanie obsługi `ModelState` i różnych ścieżek wykonania w zależności od wyników z serwisów.
        *   Symulowanie zalogowanego użytkownika (`HttpContext.User` z `ClaimsPrincipal`) do testowania logiki zależnej od oświadczeń.
    *   **Serwisy (np. `AuthServiceTests.cs`, `ResourceServiceTests.cs` - *do dodania*):**
        *   Testowanie logiki biznesowej w serwisach.
        *   Mockowanie zależności (np. `DbContext` przy użyciu dostawcy InMemory lub mocka `DbSet`, `IConfiguration`, `IHubContext`, `IRabbitMQProducerService`).
        *   Sprawdzanie poprawności interakcji z bazą danych (np. czy dane są poprawnie zapisywane/odczytywane w bazie InMemory).
        *   Weryfikacja logiki walidacji i obsługi błędów.

## Testy Integracyjne (`ResourceManagementSystem.API.Tests` lub osobny projekt)

*   **Framework:** xUnit
*   **Narzędzia:** `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory` (do testowania API w pamięci), baza danych InMemory, mocki dla zewnętrznych zależności (np. RabbitMQ, jeśli nie chcemy testować z prawdziwą instancją).
*   **Cel:** Testowanie współpracy między różnymi komponentami systemu, np. kontroler -> serwis -> baza danych, lub serwis -> SignalR Hub / RabbitMQ.
*   **Przykładowe Scenariusze (z `ResourceServiceIntegrationTests.cs`):**
    *   Weryfikacja, czy po wykonaniu operacji CRUD przez `ResourceService`:
        *   Dane są poprawnie zapisywane/modyfikowane w bazie danych (InMemory).
        *   Odpowiednie metody na mockach `IHubContext<ResourceHub>` (dla SignalR) są wywoływane.
        *   Odpowiednie metody na mocku `IRabbitMQProducerService` są wywoływane.
*   **Przyszłe Rozszerzenia:**
    *   Testy z użyciem `WebApplicationFactory` do testowania pełnego przepływu żądania HTTP przez pipeline ASP.NET Core.
    *   Testy z prawdziwą instancją RabbitMQ (np. w Dockerze) i nasłuchiwaniem na wiadomości w kolejkach.
    *   Testy z klientem SignalR łączącym się z Hubem uruchomionym przez `WebApplicationFactory`.

## Testy Automatyczne UI (`ResourceManagementSystem.UI.Tests`)

*   **Framework:** Playwright z xUnit jako runnerem.
*   **Wzorzec:** Page Object Model (POM) do hermetyzacji interakcji ze stronami UI.
*   **Cel:** Testowanie interfejsu użytkownika z perspektywy użytkownika końcowego, symulowanie interakcji z przeglądarką.
*   **Pokrycie (Przykładowe Scenariusze z `AuthTests.cs`):**
    *   **Logowanie:**
        *   Pomyślne logowanie z poprawnymi danymi i przekierowanie na stronę zasobów.
        *   Nieudane logowanie z niepoprawnymi danymi i wyświetlenie komunikatu o błędzie.
    *   **Rejestracja (do dodania):**
        *   Pomyślna rejestracja i automatyczne logowanie.
        *   Nieudana rejestracja (np. zajęty username/email, błędy walidacji).
    *   **Zarządzanie Zasobami (do dodania):**
        *   Wyświetlanie listy zasobów.
        *   Tworzenie nowego zasobu przez formularz w modalu.
        *   Edycja istniejącego zasobu.
        *   Usuwanie zasobu z potwierdzeniem.
        *   Weryfikacja aktualizacji UI w czasie rzeczywistym po zmianach (np. zainicjowanych przez inny "wirtualny" klient lub bezpośrednio przez API).
*   **Uruchamianie:** Testy UI wymagają uruchomionej aplikacji API Backend oraz aplikacji Blazor UI.

## Uruchamianie Testów

*   **Za pomocą IDE (Visual Studio / Rider):** Użyj wbudowanego Test Explorera.
*   **Za pomocą .NET CLI:**
    Przejdź do katalogu solucji lub konkretnego projektu testowego i wykonaj:
    ```bash
    dotnet test
    ```

Utrzymywanie i rozwijanie zestawu testów jest kluczowe dla długoterminowego sukcesu projektu.