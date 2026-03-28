# AssetsStorage

`AssetsStorageService` — загрузка ассетов из Cloud.

## Методы

- `LoadConfigAsync()` → `AssetStorageStructureDto` — загрузка структуры хранилища
- `GetAssetsFromType(assetType)` → `List<AssetDto>` — фильтрация по типу

### Загрузка контента
- `LoadTextFromId(id, textFileType)` → `TextFile` — текстовый файл
- `LoadTextureFromId(id, readable)` → `Texture2D` — текстура
- `LoadSpriteFromId(id, readable)` → `Sprite` — спрайт
- `LoadAudioFromId(id, audioType)` → `AudioClip` — аудио
- `LoadAssetBundleFromId(id)` → `AssetBundle` — ассет-бандл

## Свойства

- `Assets` — `IReadOnlyList<Asset>` загруженные ассеты
- `Folders` — `IReadOnlyList<Folder>` структура папок

## Code
- `Core/Services/Asset Storage/*`
