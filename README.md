# CrossApp
 Наскрізний проєкт з крос-платформного програмування.
 Предметна область: Склад. Сутності: Product, StockBatch, Warehouse, Movement.
 Призначення: облік залишків товарів по партіях.

## Запуск
Команди виконуються з кореня CrossApp.

```powershell
dotnet build
dotnet run --project src/Cli -f net8.0
dotnet run --project src/Cli -f net10.0
```

 ## Середовище
 .NET SDK 8.0, Windows 11 x64

## Структура solution
- CrossApp.sln — файл рішення.
- src/Core/ — бібліотека збору інформації про середовище та імпорту даних.
  - Core.csproj — налаштування бібліотеки.
  - EnvironmentInfo.cs — клас EnvironmentInfo та запис EnvironmentReport.
  - Dto/ — типи ProductDto, WarehouseDto та ImportResult<T>.
  - Import/ — імпортери ProductCsvImporter, ProductJsonImporter
    та MixedCsvImporter.
- src/Cli/ — консольний застосунок.
  - Cli.csproj — налаштування застосунку та посилання на Core.
  - Program.cs — обробка аргументів, вибір імпортера, виведення
    даних, помилок і статистики.
- data/ — тестові CSV- та JSON-файли.

Напрямок залежності: Cli → Core.
Core збирає інформацію про середовище та виконує імпорт даних.
Cli відповідає за взаємодію з користувачем і консольний вивід.

Заплановані каталоги Core:
- Domain/ — сутності з поведінкою та інваріантами.
- Storage/ — реалізації сховищ.

## Публікація
Команди виконуються з кореня CrossApp.

Self-contained:
```powershell
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -o publish/self-contained
```

Framework-dependent:
```powershell
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o publish/framework-dependent
```

Запуск self-contained публікації з її каталогу:
```powershell
cd .\publish\self-contained
.\Cli.exe
```

Запуск framework-dependent публікації з її каталогу, починаючи з кореня CrossApp:
```powershell
cd .\publish\framework-dependent
.\Cli.exe
```

## Порівняння режимів публікації — ЛР №2
| RID | Режим | Розмір publish | Потрібен встановлений runtime |
|---|---|---|---|
| win-x64 | self-contained | 70,68 МБ | Ні |
| win-x64 | framework-dependent | 0,18 МБ | Так, .NET 8 |

Self-contained містить застосунок, його залежності та .NET Runtime.
Тому публікація займає більше місця, але працює без попереднього
встановлення runtime.

Framework-dependent містить застосунок і його залежності без
.NET Runtime. Тому публікація має менший розмір, але потребує
встановленого сумісного .NET Runtime 8.

 ## Додаткове завдання - ЛР №1
 Розміри self-contained публікацій:
 - win-x64 — 70.4 МБ
 - linux-x64 — 70.5 МБ

 ## Додаткове завдання — ЛР №2
Усі публікації створено для RID win-x64 у конфігурації Release.

| Варіант публікації | Кількість файлів | Розмір, МБ |
|---|---:|---:|
| Self-contained | 189 | 70,68 |
| Framework-dependent | 7 | 0,18 |
| Self-contained + SingleFile | 3 | 64,3 |
| Self-contained + Trimming | 28 | 18,1 |

Попередження під час публікації з trimming: не було помічено мною.

SingleFile об’єднує керовані компоненти застосунку в один виконуваний
файл. Нативні бібліотеки та файли налагодження можуть залишатися окремо.

Trimming зменшує розмір публікації шляхом видалення невикористаного коду.

## ЛР №3 — імпорт даних

Реалізовано імпорт товарів із CSV/JSON, імпорт товарів і складів
за префіксами P/W та виведення статистики й помилок.

### Формат файлів

Кодування всіх файлів — UTF-8.

- CSV: роздільник `;`, заголовок `id;sku;name;unit;quantity`.
  Кількість — ціле невід’ємне число.
- JSON: масив товарів із властивостями `id`, `sku`, `name`,
  `unit`, `quantity` та необов’язковою `note`.
- Змішаний CSV: без заголовка, роздільник `;`.
  Товар — `P;id;sku;name;unit;quantity`,
  склад — `W;id;name;address`. Адреса може бути порожньою.

`valid.csv` містить коректні дані. У `sample.csv`, `sample.json`
і `mixed.csv` навмисно додано помилкові записи.

### Запуск із кореня CrossApp

```powershell
dotnet run --project src/Cli -f net10.0 -- data/valid.csv
dotnet run --project src/Cli -f net10.0 -- data/sample.csv
dotnet run --project src/Cli -f net10.0 -- data/sample.json
dotnet run --project src/Cli -f net10.0 -- data/mixed.csv --mixed
```

Без шляху використовується `data/sample.csv`.