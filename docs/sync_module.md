
# 6. Moduł Synchronizacji Danych <a name="moduł-synchronizacji-danych"></a>

Moduł synchronizacji danych ma na celu zapewnienie spójności informacji i powiadamianie o zmianach w systemie. Składa się on z dwóch głównych mechanizmów zintegrowanych w module API: SignalR i RabbitMQ.

## SignalR (`ResourceHub`)

### Cel
Umożliwienie komunikacji w czasie rzeczywistym między serwerem API a podłączonymi klientami (głównie aplikacją Blazor UI). Gdy zasób jest tworzony, aktualizowany lub usuwany, serwer wysyła powiadomienie do wszystkich klientów, aby mogli oni natychmiast odświeżyć swoje widoki.

### Implementacja
*   **Hub:** `ResourceManagementSystem.API.Hubs.ResourceHub`
*   **Endpoint Huba:** Dostępny pod adresem `/hubs/resource` (względem base URL API).
*   **Wysyłane Wiadomości (z serwera do klienta):**
    *   `ReceiveResourceCreated`: Wysyłana po utworzeniu nowego zasobu. Argument: `ResourceDto` nowo utworzonego zasobu.
    *   `ReceiveResourceUpdate`: Wysyłana po aktualizacji istniejącego zasobu. Argument: `ResourceDto` zaktualizowanego zasobu.
    *   `ReceiveResourceDeleted`: Wysyłana po usunięciu zasobu. Argument: `Guid` ID usuniętego zasobu.
*   **Integracja:** `ResourceService` w API wywołuje odpowiednie metody `ResourceHub` po pomyślnych operacjach CRUD. Aplikacja UI (`ResourceUIService`) nasłuchuje na te wiadomości i aktualizuje listę zasobów.
*   **Uwierzytelnianie:** Połączenie z Hubem może być chronione (np. poprzez dodanie atrybutu `[Authorize]` do klasy Huba). Klient musi wtedy przekazać token JWT podczas nawiązywania połączenia (np. w query string `access_token` dla transportu WebSockets).

## RabbitMQ (`RabbitMQProducerService`)

### Cel
Umożliwienie asynchronicznego i niezawodnego publikowania zdarzeń o zmianach danych do systemu kolejkowego. Pozwala to na oddzielenie logiki powiadamiania innych systemów lub wykonywania długotrwałych zadań od głównego przepływu żądania API. Inne serwisy lub komponenty (nawet w innych technologiach) mogą subskrybować te zdarzenia i reagować na nie.

### Implementacja
*   **Serwis Publikujący:** `ResourceManagementSystem.API.Services.RabbitMQProducerService`
*   **Konfiguracja:** Połączenie z serwerem RabbitMQ jest konfigurowane w `appsettings.json` projektu API.
*   **Exchange:** Używany jest exchange typu `topic` o nazwie `resource.events` (durable).
*   **Publikowane Wiadomości:**
    *   **Po utworzeniu zasobu:**
        *   Routing Key: `resource.created`
        *   Treść: `ResourceDto` utworzonego zasobu.
    *   **Po aktualizacji zasobu:**
        *   Routing Key: `resource.updated.{ResourceId}` (np. `resource.updated.xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)
        *   Treść: `ResourceDto` zaktualizowanego zasobu.
    *   **Po usunięciu zasobu:**
        *   Routing Key: `resource.deleted.{ResourceId}`
        *   Treść: Obiekt anonimowy zawierający `ResourceId`, `Name`, `DeletedBy`, `DeletedAt`.
    *   Wiadomości są publikowane jako trwałe (`Persistent = true`).
*   **Integracja:** `ResourceService` w API wywołuje `RabbitMQProducerService.PublishMessage` po pomyślnych operacjach CRUD.

### Potencjalne Zastosowania Konsumentów RabbitMQ
*   Aktualizacja cache'y w innych serwisach.
*   Synchronizacja danych z systemami zewnętrznymi.
*   Generowanie logów audytowych.
*   Wyzwalanie procesów analitycznych.
*   Implementacja backplane dla SignalR w środowisku skalowalnym (bardziej zaawansowane).

Na obecnym etapie zaimplementowano jedynie stronę produkującą wiadomości. Stworzenie konsumentów jest poza zakresem podstawowej funkcjonalności, ale infrastruktura jest gotowa.

    