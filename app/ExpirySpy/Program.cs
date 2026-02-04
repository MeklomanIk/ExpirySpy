using ExpirySpy;

Database.EnsureCreated();
var repository = new Repository(Database.GetConnectionString());

SeedCategoriesIfEmpty(repository);
NormalizeCategoryNames(repository);
SeedSampleResourcesIfEmpty(repository);

RunMainMenu(repository);

static void RunMainMenu(Repository repo)
{
    while (true)
    {
        Console.Clear();
        ShowOverview(repo);
        Console.WriteLine("""
                          Действия:
                          1. Показать дерево категорий
                          2. Показать все ресурсы
                          3. Показать ресурсы с истекающим сроком годности
                          4. Показать просроченные ресурсы
                          5. Показать ресурсы с малым остатком
                          6. Изменить количество ресурса
                            7. Добавить категорию
                          8. Добавить ресурс
                          9. Удалить ресурс
                          10. Удалить категорию
                          0. Выход
                          """);
        Console.WriteLine("Выберите действие:");
        var input = Console.ReadLine();

        switch (input)
        {
            case "1":
                ShowCategories(repo);
                break;
            case "2":
                ShowResources(repo);
                break;
            case "3":
                ShowResourcesByState(repo, ResourceState.ExpiringSoon);
                break;
            case "4":
                ShowResourcesByState(repo, ResourceState.Expired);
                break;
            case "5":
                ShowLowOnStock(repo);
                break;
            case "6":
                ChangeResourceQuantity(repo);
                break;
            case "7":
                AddCategory(repo);
                break;
            case "8":
                AddResource(repo);
                break;
            case "9":
                DeleteResource(repo);
                break;
            case "10":
                DeleteCategory(repo);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Неизвестная команда.");
                Pause();
                break;
        }
    }
}

static void SeedCategoriesIfEmpty(Repository repo)
{
    if (repo.GetAllCategories().Count > 0) return;

    // Корневые категории
    repo.AddCategory(new ResourceCategory { Name = "еда" });
    repo.AddCategory(new ResourceCategory { Name = "медикаменты" });
    repo.AddCategory(new ResourceCategory { Name = "средства для уборки" });
    repo.AddCategory(new ResourceCategory { Name = "одежда" });

    // Подкатегории еды
    void AddSubCategories(string parentName, string[] children)
    {
        foreach (var name in children)
        {
            repo.AddCategory(new ResourceCategory
                { Name = name, ParentId = new ResourceCategory { Name = parentName }.Id });
        }
    }

    AddSubCategories("еда", new[]
    {
        "овощи", "фрукты", "молочные продукты", "крупы и макароны", "мясо, рыба, яйца", "замороженные продукты",
        "маринады", "соусы", "орехи и сухофрукты", "чай и сборы", "специи"
    });
    AddSubCategories("медикаменты",
        new[] { "витамины и добавки", "обезболивающие", "средства от простуды", "перевязочные материалы" });
    AddSubCategories("средства для уборки",
        new[] { "моющие средства", "губки и тряпки", "мешки", "гигиенические средства", "бумажные изделия" });
    AddSubCategories("одежда", new[] { "обувь", "уличная одежда", "домашняя одежда", "официальная одежда" });
}

static void ShowCategories(Repository repo)
{
    Console.Clear();
    Console.WriteLine("Категории:");
    var categories = repo.GetAllCategories();
    var resources = repo.GetAllResources();
    var countsByCategory = resources
        .GroupBy(r => r.CategoryId)
        .ToDictionary(g => g.Key, g => g.Count());
    if (categories.Count == 0)
        Console.WriteLine("Категорий пока нет.");
    else
    {
        // Группируем по родителю и выводим в виде дерева.
        // Для корневых категорий (ParentId == null) используем ключ 0.
        const long rootKey = 0;
        var byParent = categories
            .GroupBy(c => c.ParentId ?? rootKey)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name).ToList());

        int GetTotalCount(long categoryId)
        {
            countsByCategory.TryGetValue(categoryId, out var directCount);
            var total = directCount;

            if (byParent.TryGetValue(categoryId, out var children))
            {
                foreach (var child in children)
                {
                    total += GetTotalCount(child.Id);
                }
            }

            return total;
        }

        void PrintTree(long parentId, string prefix, bool isRootLevel)
        {
            if (!byParent.TryGetValue(parentId, out var children))
                return;

            foreach (var c in children)
            {
                var totalCount = GetTotalCount(c.Id);
                if (isRootLevel)
                    Console.WriteLine($"| {c.Name} ({totalCount})");
                else
                    Console.WriteLine($"{prefix}- {c.Name} ({totalCount})");

                PrintTree(c.Id, prefix + "  ", false);
            }
        }

        PrintTree(rootKey, "| ", true);
    }

    Pause();
}

static void AddCategory(Repository repo)
{
    Console.Clear();
    Console.WriteLine("Новая категория");

    Console.Write("Название: ");
    var name = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Название обязательно.");
        Pause();
        return;
    }

    Console.Write("Id родительской категории (пусто, если нет): ");
    var parentInput = Console.ReadLine();
    long? parentId = null;
    if (!string.IsNullOrWhiteSpace(parentInput) && long.TryParse(parentInput, out var pid))
        parentId = pid;

    Console.Write("Количество по умолчанию (пусто, если нет): ");
    var dqInput = Console.ReadLine();
    double? dq = null;
    if (!string.IsNullOrWhiteSpace(dqInput) && double.TryParse(dqInput, out var dqd))
        dq = dqd;

    Console.Write("Единица измерения по умолчанию (пусто, если нет): ");
    var du = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(du))
        du = null;

    Console.Write("Минимальное количество по умолчанию (пусто, если нет): ");
    var dmqInput = Console.ReadLine();
    double? dmq = null;
    if (!string.IsNullOrWhiteSpace(dmqInput) && double.TryParse(dmqInput, out var dmqd))
        dmq = dmqd;

    var cat = new ResourceCategory
    {
        Name = name.Trim(),
        ParentId = parentId,
        DefaultQuantity = dq,
        DefaultUnit = du,
        DefaultMinQuantity = dmq
    };

    repo.AddCategory(cat);
    Console.WriteLine("Категория добавлена.");
    Pause();
}

static void ShowResources(Repository repo)
{
    var list = repo.GetResourcesWithCategories();
    if (list.Count == 0)
    {
        Console.Clear();
        Console.WriteLine("Ресурсов пока нет.");
        Pause();
        return;
    }

    // Группируем по категориям и выводим блоками
    var groups = list
        .OrderBy(t => t.CategoryName)
        .ThenBy(t => t.Item.ExpiryDate)
        .GroupBy(t => t.CategoryName);

    var first = true;
    foreach (var group in groups)
    {
        var count = group.Count();
        var title = $"{group.Key} (ресурсов:{count})";
        PrintResourceTable(group.Select(g => (g.Item, g.CategoryName)), title, clearBefore: first);
        first = false;
    }

    Pause();
}

static void AddResource(Repository repo)
{
    Console.Clear();
    Console.WriteLine("Новый ресурс");

    var categories = repo.GetAllCategories();
    if (categories.Count == 0)
    {
        Console.WriteLine("Сначала создайте хотя бы одну категорию.");
        Pause();
        return;
    }

    Console.WriteLine("Доступные категории:");
    foreach (var c in categories)
    {
        Console.WriteLine($"{c.Id}. {c.Name}");
    }

    Console.Write("Id категории: ");
    if (!long.TryParse(Console.ReadLine(), out var catId) || categories.All(c => c.Id != catId))
    {
        Console.WriteLine("Неверная категория.");
        Pause();
        return;
    }

    var cat = categories.First(c => c.Id == catId);

    Console.Write("Название ресурса: ");
    var name = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Название обязательно.");
        Pause();
        return;
    }

    Console.Write($"Дата покупки (yyyy-MM-dd, по умолчанию сегодня {DateTime.Now:yyyy-MM-dd}): ");
    var pdInput = Console.ReadLine();
    DateTime purchaseDate;
    if (string.IsNullOrWhiteSpace(pdInput))
        purchaseDate = DateTime.Now.Date;
    else if (!DateTime.TryParse(pdInput, out purchaseDate))
    {
        Console.WriteLine("Неверный формат даты.");
        Pause();
        return;
    }

    Console.Write("Срок годности (yyyy-MM-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out var expiryDate))
    {
        Console.WriteLine("Неверный формат даты.");
        Pause();
        return;
    }

    Console.Write($"Количество (по умолчанию {cat.DefaultQuantity?.ToString() ?? "1"}): ");
    var qInput = Console.ReadLine();
    double quantity;
    if (string.IsNullOrWhiteSpace(qInput))
        quantity = cat.DefaultQuantity ?? 1;
    else if (!double.TryParse(qInput, out quantity))
    {
        Console.WriteLine("Неверное количество.");
        Pause();
        return;
    }

    Console.Write($"Единицы измерения (по умолчанию {cat.DefaultUnit ?? "шт"}): ");
    var uInput = Console.ReadLine();
    var unitRaw = string.IsNullOrWhiteSpace(uInput) ? (cat.DefaultUnit ?? "шт") : uInput.Trim();
    var unit = NormalizeUnit(unitRaw);

    Console.Write($"Минимальное количество (по умолчанию {cat.DefaultMinQuantity?.ToString() ?? "1"}): ");
    var mqInput = Console.ReadLine();
    double minQuantity;
    if (string.IsNullOrWhiteSpace(mqInput))
        minQuantity = cat.DefaultMinQuantity ?? 1;
    else if (!double.TryParse(mqInput, out minQuantity))
    {
        Console.WriteLine("Неверное количество.");
        Pause();
        return;
    }

    var item = new ResourceItem
    {
        CategoryId = catId,
        Name = name.Trim(),
        PurchaseDate = purchaseDate,
        ExpiryDate = expiryDate,
        Quantity = quantity,
        Unit = unit,
        MinQuantity = minQuantity
    };

    repo.AddResource(item);
    Console.WriteLine("Ресурс добавлен.");
    Pause();
}

static void ShowResourcesByState(Repository repo, ResourceState state)
{
    Console.Clear();
    var now = DateTime.Now;
    var list = repo.GetResourcesByState(state, now);

    var title = state switch
    {
        ResourceState.Expired => "Просроченные ресурсы",
        ResourceState.ExpiringSoon => "Ресурсы с истекающим сроком годности",
        _ => "Ресурсы"
    };

    if (list.Count == 0)
    {
        Console.Clear();
        Console.WriteLine(title + ":");
        Console.WriteLine("Нет ресурсов в этом состоянии.");
        Pause();
        return;
    }

    var categories = repo.GetAllCategories()
        .ToDictionary(c => c.Id, c => c.Name);

    var rows = list
        .Select(item =>
            (item,
                categories.TryGetValue(item.CategoryId, out var name)
                    ? name
                    : "Неизвестная категория"))
        .ToList();

    PrintResourceTable(rows, title, clearBefore: true);

    Pause();
}

static void ShowLowOnStock(Repository repo)
{
    Console.Clear();
    Console.WriteLine("Ресурсы с малым остатком:");
    var list = repo.GetLowOnStockResources();
    if (list.Count == 0)
    {
        Console.WriteLine("Таких ресурсов нет.");
    }
    else
    {
        var categories = repo.GetAllCategories()
            .ToDictionary(c => c.Id, c => c.Name);

        var rows = list
            .Select(item =>
                (item,
                    categories.TryGetValue(item.CategoryId, out var name)
                        ? name
                        : "Неизвестная категория"))
            .ToList();

        PrintResourceTable(rows, null, clearBefore: true);
    }

    Pause();
}

static void NormalizeCategoryNames(Repository repo)
{
    var categories = repo.GetAllCategories();
    foreach (var c in categories)
    {
        if (string.IsNullOrWhiteSpace(c.Name))
            continue;

        var name = c.Name.Trim();
        var first = name[0];
        var upperFirst = char.ToUpper(first);
        var normalized = upperFirst + name[1..];

        if (normalized != c.Name)
        {
            repo.UpdateCategoryName(c.Id, normalized);
        }
    }
}

static void ShowOverview(Repository repo)
{
    Console.WriteLine("=== ExpirySpy ===");

    var categories = repo.GetAllCategories();
    var resources = repo.GetAllResources();

    // ---- Таблица категорий ----
    Console.WriteLine();
    Console.WriteLine("Родительские категории:");

    if (categories.Count == 0)
    {
        Console.WriteLine("Категорий пока нет.");
    }
    else
    {
        const long rootKey = 0;
        var byParent = categories
            .GroupBy(c => c.ParentId ?? rootKey)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name).ToList());

        var directCounts = resources
            .GroupBy(r => r.CategoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        int GetTotal(long categoryId)
        {
            directCounts.TryGetValue(categoryId, out var direct);
            var total = direct;

            if (byParent.TryGetValue(categoryId, out var children))
            {
                foreach (var ch in children)
                        total += GetTotal(ch.Id);
            }

            return total;
        }

        Console.WriteLine("| {0,-24} | {1,8} |", "Категория", "Ресурсов");
        Console.WriteLine("|{0}|", new string('-', 24 + 2) + "|" + new string('-', 8 + 2));

        if (byParent.TryGetValue(rootKey, out var roots))
        {
            foreach (var root in roots)
            {
                var total = GetTotal(root.Id);
                Console.WriteLine("| {0,-24} | {1,8} |",
                    Truncate(root.Name, 24),
                    total);
            }
        }
    }

    // ---- Таблица по ресурсам в целом ----
    Console.WriteLine();
    Console.WriteLine("Общее состояние ресурсов:");

    if (resources.Count == 0)
    {
        Console.WriteLine("Ресурсов пока нет.");
    }
    else
    {
        var now = DateTime.Now;
        var total = resources.Count;
        var expired = resources.Count(r => r.GetState(now) == ResourceState.Expired);
        var soon = resources.Count(r => r.GetState(now) == ResourceState.ExpiringSoon);
        var ok = resources.Count(r => r.GetState(now) == ResourceState.Available);
        var low = resources.Count(r => r.IsLowOnStock());

        Console.WriteLine("| {0,-14} | {1,5} |", "Состояние", "Кол-во");
        Console.WriteLine("|{0}|", new string('-', 14 + 2) + "|" + new string('-', 5 + 2));
        Console.WriteLine("| {0,-14} | {1,5} |", "Всего", total);
        Console.WriteLine("| {0,-14} | {1,5} |", "Просрочены", expired);
        Console.WriteLine("| {0,-14} | {1,5} |", "Скоро истечёт", soon);
        Console.WriteLine("| {0,-14} | {1,5} |", "Доступны", ok);
        Console.WriteLine("| {0,-14} | {1,5} |", "Мало на складе", low);
    }

    Console.WriteLine();
}

static void ChangeResourceQuantity(Repository repo)
{
    var list = repo.GetResourcesWithCategories();
    if (list.Count == 0)
    {
        Console.WriteLine("Ресурсов пока нет.");
        Pause();
        return;
    }

    PrintResourceTable(list, "Все ресурсы", clearBefore: true);

    Console.WriteLine();
    Console.Write("Введите ID ресурса, количество которого нужно изменить: ");
    if (!long.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("Неверный ID.");
        Pause();
        return;
    }

    var existing = list.FirstOrDefault(x => x.Item.Id == id);
    if (existing.Item == null)
    {
        Console.WriteLine("Ресурс с таким ID не найден.");
        Pause();
        return;
    }

    Console.Write($"Осталось продукта ({existing.Item.Unit})? (текущее {existing.Item.Quantity:0.##}): ");
    var qtyInput = Console.ReadLine();
    if (!double.TryParse(qtyInput, out var newQty))
    {
        Console.WriteLine("Неверное количество.");
        Pause();
        return;
    }

    repo.UpdateResourceQuantity(id, newQty);
    Console.WriteLine("Количество обновлено.");
    Pause();
}

static void DeleteResource(Repository repo)
{
    var list = repo.GetResourcesWithCategories();
    if (list.Count == 0)
    {
        Console.WriteLine("Ресурсов пока нет.");
        Pause();
        return;
    }

    PrintResourceTable(list, "Удаление ресурса", clearBefore: true);

    Console.WriteLine();
    Console.Write("Введите ID ресурса, который нужно удалить: ");
    if (!long.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("Неверный ID.");
        Pause();
        return;
    }

    var existing = list.FirstOrDefault(x => x.Item.Id == id);
    if (existing.Item == null)
    {
        Console.WriteLine("Ресурс с таким ID не найден.");
        Pause();
        return;
    }

    Console.Write($"Точно удалить ресурс \"{existing.Item.Name}\"? (y/n): ");
    var confirm = Console.ReadLine();
    if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Удаление отменено.");
        Pause();
        return;
    }

    repo.DeleteResource(id);
    Console.WriteLine("Ресурс удалён.");
    Pause();
}

static void DeleteCategory(Repository repo)
{
    Console.Clear();
    Console.WriteLine("Удаление категории");

    var categories = repo.GetAllCategories();
    if (categories.Count == 0)
    {
        Console.WriteLine("Категорий пока нет.");
        Pause();
        return;
    }

    var resources = repo.GetAllResources();
    var countsByCategory = resources
        .GroupBy(r => r.CategoryId)
        .ToDictionary(g => g.Key, g => g.Count());

    Console.WriteLine("| {0,4} | {1,-24} | {2,8} |", "ID", "Категория", "Ресурсов");
    Console.WriteLine("|{0}|",
        new string('-', 4 + 2) + "|" + new string('-', 24 + 2) + "|" + new string('-', 8 + 2));
    foreach (var c in categories.OrderBy(c => c.Name))
    {
        countsByCategory.TryGetValue(c.Id, out var count);
        Console.WriteLine("| {0,4} | {1,-24} | {2,8} |",
            c.Id,
            Truncate(c.Name, 24),
            count);
    }

    Console.WriteLine();
    Console.Write("Введите ID категории, которую нужно удалить: ");
    if (!long.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("Неверный ID.");
        Pause();
        return;
    }

    var category = categories.FirstOrDefault(c => c.Id == id);
    if (category == null)
    {
        Console.WriteLine("Категория с таким ID не найдена.");
        Pause();
        return;
    }

    var hasChildren = categories.Any(c => c.ParentId == id);
    countsByCategory.TryGetValue(id, out var directCount);

    if (hasChildren || directCount > 0)
    {
        Console.WriteLine("Нельзя удалить категорию: у неё есть подкатегории и/или ресурсы.");
        Console.WriteLine("Сначала удалите связанные ресурсы и подкатегории.");
        Pause();
        return;
    }

    Console.Write($"Точно удалить категорию \"{category.Name}\"? (y/n): ");
    var confirm = Console.ReadLine();
    if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Удаление отменено.");
        Pause();
        return;
    }

    repo.DeleteCategory(id);
    Console.WriteLine("Категория удалена.");
    Pause();
}

static void SeedSampleResourcesIfEmpty(Repository repo)
{
    var existing = repo.GetAllResources();
    if (existing.Count > 0)
        return;

    var categories = repo.GetAllCategories();

    ResourceCategory? FindCategory(string name) =>
        categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    var today = DateTime.Today;

    void AddSample(string categoryName, string resourceName, int daysToExpire, double quantity, string unit,
        double minQuantity)
    {
        var cat = FindCategory(categoryName);
        if (cat is null) return;

        var item = new ResourceItem
        {
            CategoryId = cat.Id,
            Name = resourceName,
            PurchaseDate = today.AddDays(-3),
            ExpiryDate = today.AddDays(daysToExpire),
            Quantity = quantity,
            Unit = NormalizeUnit(unit),
            MinQuantity = minQuantity
        };

        repo.AddResource(item);
    }

    // Еда
    AddSample("Овощи", "Картофель", 20, 5, "кг", 1);
    AddSample("Овощи", "Морковь", 15, 2, "кг", 0.5);
    AddSample("Фрукты", "Яблоки", 10, 1.5, "кг", 0.5);
    AddSample("Фрукты", "Бананы", 5, 1, "кг", 0.5);
    AddSample("Молочные продукты", "Молоко", 3, 2, "л", 1);
    AddSample("Молочные продукты", "Йогурт", 7, 6, "шт", 2);
    AddSample("Крупы и макароны", "Рис", 120, 1, "кг", 0.25);
    AddSample("Крупы и макароны", "Гречка", 180, 1, "кг", 0.25);
    AddSample("Мясо, рыба, яйца", "Курица", 4, 2, "кг", 0.5);
    AddSample("Мясо, рыба, яйца", "Яйца", 14, 20, "шт", 6);
    AddSample("Замороженные продукты", "Замороженные овощи", 180, 3, "уп", 1);
    AddSample("Маринады", "Маринованные огурцы", 365, 2, "банка", 1);
    AddSample("Соусы", "Майонез", 90, 1, "уп", 0.3);
    AddSample("Соусы", "Соевый соус", 365, 1, "бут.", 0.2);
    AddSample("Орехи и сухофрукты", "Миндаль", 180, 0.5, "кг", 0.1);
    AddSample("Чай и сборы", "Чёрный чай", 365, 1, "уп", 0.2);
    AddSample("Специи", "Соль", 365, 1, "кг", 0.2);
    AddSample("Специи", "Перец", 365, 0.1, "кг", 0.02);

    // Средства для уборки
    AddSample("Моющие средства", "Средство для посуды", 365, 1, "бут.", 0.2);
    AddSample("Губки и тряпки", "Губки кухонные", 365, 10, "шт", 2);
    AddSample("Мешки", "Мешки для мусора", 365, 30, "шт", 10);
    AddSample("Гигиенические средства", "Зубная паста", 365, 2, "тюбик", 1);
    AddSample("Бумажные изделия", "Туалетная бумага", 365, 16, "рулон", 4);

    // Одежда
    AddSample("Обувь", "Кроссовки", 365, 1, "пара", 0);
    AddSample("Уличная одежда", "Куртка", 365, 1, "шт", 0);
    AddSample("Домашняя одежда", "Футболка домашняя", 365, 3, "шт", 1);
    AddSample("Официальная одежда", "Рубашка", 365, 2, "шт", 1);
}

static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("Нажмите Enter, чтобы продолжить...");
    Console.ReadLine();
}

static void PrintResourceTable(IEnumerable<(ResourceItem Item, string CategoryName)> rows, string? title,
    bool clearBefore)
{
    var list = rows.ToList();
    if (list.Count == 0)
        return;

    var now = DateTime.Now;

    if (clearBefore)
        Console.Clear();

    if (!string.IsNullOrWhiteSpace(title))
    {
        Console.WriteLine();
        Console.WriteLine($"| {title} |");
    }

    // Заголовок в стиле Markdown-таблицы (но без строгих ограничений)
    Console.WriteLine("| {0,3} | {1,-22} | {2,-16} | {3,8} | {4,-4} | {5,10} | {6,10} | {7,5} | {8,-10} | {9,-4} |",
        "ID", "Название", "Категория", "Кол-во", "Ед.", "Куплено", "Годен до", "Дней", "Состояние", "Мало");
    Console.WriteLine("|{0}|",
        new string('-', 3 + 2) + "|" +
        new string('-', 22 + 2) + "|" +
        new string('-', 16 + 2) + "|" +
        new string('-', 8 + 2) + "|" +
        new string('-', 4 + 2) + "|" +
        new string('-', 10 + 2) + "|" +
        new string('-', 10 + 2) + "|" +
        new string('-', 5 + 2) + "|" +
        new string('-', 10 + 2) + "|" +
        new string('-', 4 + 2));

    foreach (var (item, categoryName) in list)
    {
        var state = item.GetState(now);
        var stateLabel = state switch
        {
            ResourceState.Expired => "ПРОСРОЧЕН",
            ResourceState.ExpiringSoon => "Скоро",
            _ => "OK"
        };

        var daysLeft = (item.ExpiryDate.Date - now.Date).TotalDays;
        var daysText = ((int)daysLeft).ToString();
        
        var lowFlag = item.IsLowOnStock() ? "Да" : "";

        Console.WriteLine(
            "| {0,3} | {1,-22} | {2,-16} | {3,8:0.##} | {4,-4} | {5:yyyy-MM-dd} | {6:yyyy-MM-dd} | {7,5} | {8,-10} | {9,-4} |",
            item.Id,
            Truncate(item.Name, 22),
            Truncate(categoryName, 16),
            item.Quantity,
            Truncate(item.Unit, 4),
            item.PurchaseDate,
            item.ExpiryDate,
            daysText,
            stateLabel,
            lowFlag);
    }
}

static string Truncate(string value, int maxLength)
{
    if (string.IsNullOrEmpty(value)) return value;
    return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
}

static string NormalizeUnit(string unit)
{
    if (string.IsNullOrWhiteSpace(unit))
        return unit;
    return unit.Trim();
}