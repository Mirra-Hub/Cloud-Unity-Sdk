# Auth

`AuthenticationService` — аутентификация и управление сессиями.

## Методы входа

| Метод | Описание |
|-------|----------|
| `LoginGuestAsync(createAccount)` | Гостевой вход |
| `LoginDeviceAsync(deviceId, createAccount)` | По ID устройства |
| `LoginEmailAsync(email, password, createAccount)` | По email/пароль |
| `LoginUsernameAsync(username, password, createAccount)` | По username/пароль |
| `LoginVkGamesAsync(userId, createAccount)` | VK Games |
| `LoginYandexGamesAsync(userId, createAccount)` | Yandex Games |
| `LoginGooglePlayAsync(userId, createAccount)` | Google Play |
| `LoginAppleGameCenterAsync(userId, createAccount)` | Apple Game Center |
| `LoginGoogleAsync(userId, createAccount)` | Google |
| `LoginAppleAsync(userId, createAccount)` | Apple |
| `LoginYandexAsync(userId, createAccount)` | Yandex |

Все Login-методы возвращают `AsyncOperation<RestApiResult<GetAuthDataDto>>`.
Параметр `createAccount` (по умолчанию `true`) — создать аккаунт, если не существует.

## OpenID

- `LoginOpenIdAsync(providerId, options)` — вход через OpenID провайдер
- `LoginGoogleOpenIdAsync(options)` / `LoginYandexOpenIdAsync(options)` — сокращённые версии
- `BeginOpenIdLoginUrlAsync(providerId, successUrl)` — получить URL для OpenID
- `StartOpenIdLoginAsync(providerId, successUrl)` / `CompleteOpenIdLoginAsync(openIdKey)` — двухшаговый OpenID

## Привязка аккаунтов (Link)

Аналогично Login-методам, но с префиксом `Link*` и `createAccount = false` по умолчанию.
`ResolveLinkConflictAsync(providerType, targetAccountId)` — разрешение конфликта при привязке.

## Сессии

- `InitializeAsync()` — инициализация с сохранённым refresh token
- `RefreshSessionAsync()` — обновление сессии
- `LogoutAsync()` — выход из текущей сессии
- `LogoutAllAsync()` — выход из всех сессий

## Свойства

- `AuthToken` — текущий токен
- `SessionId` — ID текущей сессии
- `IsAuth` — авторизован ли

## События

- `OnLogin` — успешный вход
- `OnAuthConflict` — конфликт при привязке
- `OnSessionRefreshed` — сессия обновлена
- `OnSessionExpired` — сессия истекла

## Code
- `Core/Services/Auth/*`
