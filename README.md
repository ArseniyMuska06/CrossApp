# CrossApp
 Наскрізний проєкт з крос-платформного програмування.
 Предметна область: Склад. Сутності: Product, StockBatch, Warehouse, Movement.
 Призначення: облік залишків товарів по партіях.

## Запуск
Команди виконуються з кореня CrossApp.

```powershell
dotnet build
dotnet run --project src/Cli
```

 ## Середовище
 .NET SDK 8.0, Windows 11 x64

## Структура solution
- CrossApp.sln — файл рішення.
- src/Core/ — бібліотека збору інформації про середовище.
  - Core.csproj — налаштування бібліотеки.
  - EnvironmentInfo.cs — клас EnvironmentInfo та запис EnvironmentReport.
- src/Cli/ — консольний застосунок.
  - Cli.csproj — налаштування застосунку та посилання на Core.
  - Program.cs — виклик Core, форматування та виведення даних.

Напрямок залежності: Cli → Core.
Core збирає та повертає дані, а Cli виводить їх у консоль.

Заплановані каталоги Core:
- Dto/ — record-типи для передавання даних.
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