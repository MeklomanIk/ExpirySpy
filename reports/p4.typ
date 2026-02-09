= Кодирование и отладка программы

== Конвенция именования

В проекте применяется стандартная конвенция именования C\# (.NET):

- *Классы и перечисления* — PascalCase: `Repository`, `ResourceItem`, `ResourceState`, `Database`
- *Методы* — PascalCase: `GetAllCategories()`, `AddResource()`, `EnsureCreated()`
- *Свойства* — PascalCase: `Name`, `ExpiryDate`, `CategoryId`
- *Приватные поля* — camelCase с префиксом `_`: `_connectionString`
- *Локальные переменные и параметры* — camelCase: `repo`, `category`, `newName`, `conn`
- *Константы* — PascalCase: `DatabaseFileName`
- *Пространство имён* — совпадает с названием проекта: `ExpirySpy`

== Стиль оформления программного кода

=== Отступы и форматирование
- Используются 4 пробела для отступов (стандарт C\#)
- Фигурные скобки на новой строке для классов и методов
- Однострочные выражения допускаются для простых методов: `public bool IsLowOnStock() => Quantity <= MinQuantity;`

=== Комментирование
- XML-документация (`/// <summary>`) для публичных свойств и методов моделей
- Разделительные комментарии для логических блоков: `// ---------- Категории ----------`
- Пояснительные комментарии для сложной логики

=== Разбиение на файлы
Код разбит по принципу «один класс — один файл»:

В таблице ниже представлена структура исходных файлов проекта и их основное содержимое.

#figure(caption: "Структура файлов проекта", supplement: "Таблица", table(
  columns: (1fr, 2fr),
  table.header([*Файл*], [*Содержимое*]),
  [Program.cs], [Точка входа, главное меню, вспомогательные функции UI],
  [Models.cs], [Модели данных: `ResourceCategory`, `ResourceItem`, `ResourceState`],
  [Repository.cs], [Класс доступа к данным (паттерн Repository)],
  [Database.cs], [Инициализация БД, создание таблиц],
))

== Разработанные модули и библиотеки

=== Внутренние модули

Приложение состоит из нескольких модулей, каждый из которых отвечает за определённую функциональность. В таблице ниже перечислены все внутренние модули проекта с указанием их типа и назначения.

#figure(caption: "Модули приложения ExpirySpy", supplement: "Таблица", table(
  columns: (1fr, 1fr, 2fr),
  table.header([*Модуль*], [*Тип*], [*Назначение*]),
  [Database], [static class], [Управление подключением к SQLite, создание схемы БД],
  [Repository], [class], [CRUD-операции с категориями и ресурсами],
  [ResourceCategory], [class], [Модель категории с иерархией (ParentId)],
  [ResourceItem], [class], [Модель ресурса с датами и количеством],
  [ResourceState], [enum], [Состояния ресурса: Expired, ExpiringSoon, Available],
))

=== Внешние библиотеки

Для работы с базой данных SQLite используется официальный NuGet-пакет от Microsoft. В таблице указаны все внешние зависимости проекта.

#figure(caption: "Используемые NuGet-пакеты", supplement: "Таблица", table(
  columns: (1fr, 1fr, 2fr),
  table.header([*Пакет*], [*Версия*], [*Назначение*]),
  [Microsoft.Data.Sqlite], [9.0.0], [Провайдер SQLite для .NET, работа с локальной БД],
))

== Разработанные функции

=== Класс Database (Database.cs)

Статический класс `Database` отвечает за инициализацию базы данных и предоставление строки подключения. В таблице приведены его методы.

#figure(caption: "Методы класса Database", supplement: "Таблица", table(
  columns: 5,
  table.header("№", "Прототип", "Входные
 параметры", "Выходные
 параметры", "Назначение"),
  "1", "string GetConnectionString()", "—", "string", "Возвращает строку подключения к базе данных SQLite",
  "2", "void EnsureCreated()", "—", "—", "Создаёт таблицы Categories и Resources, если они не существуют",
))

=== Класс Repository (Repository.cs)

Класс `Repository` реализует паттерн «Репозиторий» и инкапсулирует все операции с базой данных: создание, чтение, обновление и удаление категорий и ресурсов. В таблице перечислены все публичные и приватные методы класса.

#figure(caption: "Методы класса Repository", supplement: "Таблица", table(
  columns: (auto, 1fr, 1fr, 1fr, 1fr),
  table.header("№", "Прототип", "Входные
 параметры", "Выходные
 параметры", "Назначение"),
  "1",
  "Repository(string connectionString)",
  "connectionString: строка подключения",
  "—",
  "Конструктор, инициализирует репозиторий",

  "2", "SqliteConnection OpenConnection()", "—", "SqliteConnection", "Открывает и возвращает соединение с БД",
  "3",
  "long AddCategory(
  ResourceCategory category)",
  "category: объект категории",
  "long (Id)",
  "Добавляет категорию в БД, возвращает её Id",

  "4", "List<ResourceCategory> GetAllCategories()", "—", "List<ResourceCategory>", "Возвращает список всех категорий",
  "5",
  "ResourceCategory? GetCategoryById(\nlong id)",
  "id: идентификатор категории",
  "ResourceCategory или null",
  "Возвращает категорию по Id",

  "6",
  "void UpdateCategoryName(\nlong id, string newName)",
  "id: Id категории; newName: новое имя",
  "—",
  "Обновляет название категории",

  "7",
  "long AddResource(\nResourceItem item)",
  "item: объект ресурса",
  "long (Id)",
  "Добавляет ресурс в БД, возвращает его Id",

  "8", "List<ResourceItem> GetAllResources()", "—", "List<ResourceItem>", "Возвращает список всех ресурсов",
  "9",
  "List<(ResourceItem, string)> GetResources\nWithCategories()",
  "—",
  "List<(ResourceItem, string)>",
  "Возвращает ресурсы с названиями категорий",

  "10",
  "List<ResourceItem> GetResourcesByState(\nResourceState state, DateTime now)",
  "state: состояние; now: текущая дата",
  "List<ResourceItem>",
  "Возвращает ресурсы по состоянию (просрочен, скоро истечёт и т.д.)",

  "11", "List<ResourceItem> GetLowOnStock\nResources()", "—", "List<ResourceItem>", "Возвращает ресурсы с малым остатком",
  "12", "void DeleteResource(long id)", "id: Id ресурса", "—", "Удаляет ресурс из БД",
  "13", "void DeleteCategory(long id)", "id: Id категории", "—", "Удаляет категорию из БД",
))

=== Модели данных (Models.cs)

Класс `ResourceItem` представляет ресурс (продукт или товар) и содержит методы для определения его состояния. В таблице описаны методы экземпляра класса.

#figure(caption: "Методы класса ResourceItem", supplement: "Таблица", table(
  columns: 5,
  table.header("№", "Прототип", "Входные
 параметры", "Выходные
 параметры", "Назначение"),
  "1",
  "ResourceState GetState(DateTime now)",
  "now: текущая дата",
  "ResourceState",
  "Определяет состояние ресурса: просрочен, скоро истечёт, доступен",

  "2", "bool IsLowOnStock()", "—", "bool", "Проверяет, мало ли ресурса на складе (Quantity ≤ MinQuantity)",
))

#pagebreak()
=== Главный модуль (Program.cs)

Файл `Program.cs` содержит точку входа приложения и все функции пользовательского интерфейса: главное меню, вывод данных, добавление и удаление записей. В таблицах ниже перечислены все статические функции модуля.

#figure(caption: "Функции главного модуля Program.cs", supplement: "Таблица", table(
  columns: 5,
  table.header("№", "Прототип", "Входные
 параметры", "Выходные
 параметры", "Назначение"),
  "1", "void RunMainMenu(
Repository repo)", "repo: репозиторий", "—", "Главный цикл меню приложения",
  "2",
  "void SeedCategoriesIfEmpty(
Repository repo)",
  "repo: репозиторий",
  "—",
  "Заполняет БД начальными категориями, если пусто",

  "3",
  "void ShowCategories(
Repository repo)",
  "repo: репозиторий",
  "—",
  "Выводит дерево категорий с количеством ресурсов",

  "4", "void AddCategory(
Repository repo)", "repo: репозиторий", "—", "Добавляет новую категорию через консоль",
  "5",
  "void ShowResources(
Repository repo)",
  "repo: репозиторий",
  "—",
  "Выводит все ресурсы, сгруппированные по категориям",

  "6", "void AddResource(
Repository repo)", "repo: репозиторий", "—", "Добавляет новый ресурс через консоль",
  "7",
  "void ShowResourcesByState(
Repository repo, ResourceState state)",
  "repo: репозиторий; state: состояние",
  "—",
  "Выводит ресурсы по состоянию (просрочены, скоро истекут)",

  "8", "void ShowLowOnStock(
Repository repo)", "repo: репозиторий", "—", "Выводит ресурсы с малым остатком",
  "9",
  "void Normalize\nCategoryNames(
Repository repo)",
  "repo: репозиторий",
  "—",
  "Приводит названия категорий к единому регистру",

  "10",
  "void ShowOverview(
Repository repo)",
  "repo: репозиторий",
  "—",
  "Выводит сводку по категориям и состоянию ресурсов",

  "11",
  "void ChangeResourceQuantity(
Repository repo)",
  "repo: репозиторий",
  "—",
  "Изменяет количество выбранного ресурса",

  "12", "void DeleteResource(
Repository repo)", "repo: репозиторий", "—", "Удаляет выбранный ресурс",
  "13", "void DeleteCategory(
Repository repo)", "repo: репозиторий", "—", "Удаляет выбранную категорию",
))

Продолжение таблицы функций модуля `Program.cs`: вспомогательные функции для заполнения тестовых данных, форматирования вывода и работы с консолью.

#figure(caption: "Функции модуля Program.cs. Часть 2", supplement: "Таблица", table(
  columns: 5,
  table.header("№", "Прототип", "Входные
 параметры", "Выходные
 параметры", "Назначение"),
  "14",
  "void SeedSampleResources\nIfEmpty(
Repository repo)",
  "repo: репозиторий",
  "—",
  "Заполняет БД тестовыми ресурсами, если пусто",

  "15", "void Pause()", "—", "—", "Ожидает нажатия Enter для продолжения",
  "16",
  "void PrintResourceTable(...)",
  "rows: список ресурсов; title: заголовок; clearBefore: очищать консоль",
  "—",
  "Выводит таблицу ресурсов в консоль",

  "17",
  "string Truncate(string value, int maxLength)",
  "value: строка; maxLength: макс. длина",
  "string",
  "Обрезает строку до указанной длины",

  "18",
  "string NormalizeUnit(string unit)",
  "unit: единица измерения",
  "string",
  "Нормализует единицу измерения (trim)",
))

== Приёмы оптимизации программного кода

=== Оптимизация объёма кода
- *Паттерн using* — автоматическое освобождение ресурсов (`using var conn = OpenConnection();`) вместо явного `try-finally`
- *Expression-bodied members* — однострочные методы и свойства: `public bool IsLowOnStock() => Quantity <= MinQuantity;`
- *Raw string literals* — многострочные строки меню без escape-последовательностей (`"""..."""`)
- *Null-coalescing* — компактная обработка null: `(object?)category.ParentId ?? DBNull.Value`
- *LINQ* — декларативные запросы вместо циклов: `resources.GroupBy(r => r.CategoryId).ToDictionary(...)`
- *Локальные функции* — вложенные функции для инкапсуляции логики: `void AddSubCategories(...)` внутри `SeedCategoriesIfEmpty`

=== Оптимизация производительности
- *Ленивая загрузка данных* — категории и ресурсы загружаются только при необходимости
- *Параметризованные SQL-запросы* — защита от SQL-инъекций и переиспользование планов запросов
- *Локальная фильтрация* — фильтрация по состоянию выполняется в памяти после одного запроса к БД
- *Минимизация подключений* — каждый метод репозитория открывает и закрывает соединение атомарно

== Использованные средства отладки

В процессе разработки использовались различные инструменты для отладки и тестирования приложения. В таблице ниже перечислены основные средства и их применение.

#figure(caption: "Средства отладки", supplement: "Таблица", table(
  columns: (1fr, 2fr),
  table.header([*Средство*], [*Применение*]),
  [Visual Studio / Rider Debugger], [Пошаговая отладка, точки останова, просмотр переменных],
  [Console.WriteLine()], [Вывод промежуточных значений при разработке],
  [dotnet run], [Запуск приложения для функционального тестирования],
  [SQLite Browser], [Просмотр и редактирование данных в файле expiryspy.db],
  [Nullable reference types], [Статический анализ на этапе компиляции (`<Nullable>enable</Nullable>`)],
  [Implicit usings], [Упрощение импортов, уменьшение шаблонного кода],
))
