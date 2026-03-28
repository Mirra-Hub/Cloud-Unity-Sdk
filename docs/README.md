# Cloud Unity SDK — Документация

Unity SDK для интеграции с MirraCloud.

## Архитектура

```
MirraCloudSDK
├── Core/
│   ├── General/          — инициализация, async operations
│   ├── RestApiClient/    — HTTP клиент
│   ├── Realtime/         — WebSocket (чаты)
│   ├── Storage/          — локальное хранилище
│   ├── Logger/           — логирование
│   ├── Json/             — сериализация
│   └── Services/         — 18 сервисов (см. ниже)
├── Editor/               — редактор-утилиты
└── Example/              — примеры интеграции
```

Все сервисы используют паттерн `AsyncOperation<RestApiResult<T>>` для асинхронных вызовов.

## Сервисы

### Authentication & Accounts
- [Auth](services/Auth.md) — аутентификация (guest, email, device, OAuth, OpenID)
- [PlayerAccount](services/PlayerAccount.md) — управление аккаунтом и профилями

### Game Config
- [Economy](services/Economy.md) — валюты, предметы, энергии, инвентарь
- [Entities](services/Entities.md) — конфигурации сущностей (templates, components)
- [RemoteConfig](services/RemoteConfig.md) — удалённая конфигурация
- [Segments](services/Segments.md) — сегменты игроков
- [Deployment](services/Deployment.md) — резолв веток для окружения

### Player Data
- [CloudSave](services/CloudSave.md) — облачные сохранения (player, global, custom)
- [DailyRewards](services/DailyRewards.md) — ежедневные награды

### Competitive
- [Leaderboard](services/Leaderboard.md) — таблицы лидеров
- [Tournaments](services/Tournaments.md) — турниры с лигами
- [Challenges](services/Challenges.md) — челленджи (гонка к цели)

### Social
- [Friends](services/Friends.md) — друзья, заявки, баны
- [Groups](services/Groups.md) — группы/кланы с ролями и правами
- [Chats](services/Chats.md) — чаты (REST + WebSocket realtime)

### Content & Code
- [AssetsStorage](services/AssetsStorage.md) — загрузка ассетов (текстуры, аудио, бандлы)
- [CloudCode](services/CloudCode.md) — выполнение серверных скриптов
- [Analytics](services/Analytics.md) — аналитика и метрики
