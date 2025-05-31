# Dokumentacja Systemu Zarządzania Zasobami (Resource Management System)

## Spis Treści
1. [Wprowadzenie](#wprowadzenie)
2. [Instalacja i Konfiguracja](#instalacja-i-konfiguracja)
3. [Uruchamianie Aplikacji](#uruchamianie-aplikacji)
4. [Dokumentacja API](#dokumentacja-api)
5. [Moduł Synchronizacji Danych](#moduł-synchronizacji-danych)
6. [Moduł Interfejsu Użytkownika](#moduł-interfejsu-użytkownika)
7. [Testowanie](#testowanie)

---

## 1. Wprowadzenie <a name="wprowadzenie"></a>

### Cel Projektu
Celem projektu jest opracowanie aplikacji rozproszonej w języku C#, która symuluje system zarządzania zasobami w firmie. Główny nacisk położony jest na wykorzystanie repozytorium Git do zarządzania kodem źródłowym, współpracy w zespole oraz dokumentowania zmian.

### Główne Funkcjonalności
System umożliwia:
*   Zarządzanie użytkownikami (rejestracja, logowanie, role).
*   Zarządzanie zasobami (np. sprzętem, pomieszczeniami, danymi projektowymi) poprzez operacje CRUD.
*   Synchronizację danych między usługami rozproszonymi (koncepcja zaimplementowana przez powiadomienia SignalR i publikację do RabbitMQ).

### Główne Moduły
Aplikacja składa się z następujących modułów:
*   **API Backend (`ResourceManagementSystem.API`):** Serwer RESTful API odpowiedzialny za logikę biznesową, komunikację z bazą danych i uwierzytelnianie.
*   **Moduł Synchronizacji (`ResourceManagementSystem.Sync` lub zintegrowany w API):** Odpowiada za komunikację w czasie rzeczywistym (SignalR) i potencjalną asynchroniczną wymianę danych (RabbitMQ).
*   **Interfejs Użytkownika (`ResourceManagementSystem.UI.Web`):** Aplikacja webowa Blazor Server do interakcji z systemem.
*   **Moduł Rdzenia (`ResourceManagementSystem.Core`):** Zawiera współdzielone modele (encje, DTO), interfejsy.

### Technologie
*   **Język:** C#
*   **Framework Backend:** ASP.NET Core (.NET 8)
*   **Framework Frontend:** Blazor Server (.NET 8)
*   **Baza Danych:** SQL Server
*   **ORM:** Entity Framework Core
*   **Komunikacja w czasie rzeczywistym:** SignalR
*   **Kolejkowanie Wiadomości:** RabbitMQ
*   **Uwierzytelnianie:** JWT (przechowywane w localStorage po stronie klienta UI)
*   **Testowanie:** xUnit, Moq, Playwright
*   **Kontrola Wersji:** Git, GitHub