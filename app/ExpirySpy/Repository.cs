using Microsoft.Data.Sqlite;

namespace ExpirySpy;

public class Repository
{
    private readonly string _connectionString;

    public Repository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    // ---------- Категории ----------

    public long AddCategory(ResourceCategory category)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Categories (Name, ParentId, DefaultQuantity, DefaultUnit, DefaultMinQuantity)
VALUES ($name, $parentId, $dq, $du, $dmq);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("$name", category.Name);
        cmd.Parameters.AddWithValue("$parentId", (object?)category.ParentId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$dq", (object?)category.DefaultQuantity ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$du", (object?)category.DefaultUnit ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$dmq", (object?)category.DefaultMinQuantity ?? DBNull.Value);

        var id = (long)(cmd.ExecuteScalar() ?? 0L);
        category.Id = id;
        return id;
    }

    public List<ResourceCategory> GetAllCategories()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, ParentId, DefaultQuantity, DefaultUnit, DefaultMinQuantity FROM Categories ORDER BY Name;";

        using var reader = cmd.ExecuteReader();
        var list = new List<ResourceCategory>();
        while (reader.Read())
        {
            list.Add(new ResourceCategory
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                ParentId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                DefaultQuantity = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                DefaultUnit = reader.IsDBNull(4) ? null : reader.GetString(4),
                DefaultMinQuantity = reader.IsDBNull(5) ? null : reader.GetDouble(5)
            });
        }

        return list;
    }

    public ResourceCategory? GetCategoryById(long id)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, ParentId, DefaultQuantity, DefaultUnit, DefaultMinQuantity FROM Categories WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new ResourceCategory
        {
            Id = reader.GetInt64(0),
            Name = reader.GetString(1),
            ParentId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
            DefaultQuantity = reader.IsDBNull(3) ? null : reader.GetDouble(3),
            DefaultUnit = reader.IsDBNull(4) ? null : reader.GetString(4),
            DefaultMinQuantity = reader.IsDBNull(5) ? null : reader.GetDouble(5)
        };
    }

    public void UpdateCategoryName(long id, string newName)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Categories SET Name = $name WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$name", newName);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    // ---------- Ресурсы ----------

    public long AddResource(ResourceItem item)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Resources (CategoryId, Name, PurchaseDate, ExpiryDate, Quantity, Unit, MinQuantity)
VALUES ($catId, $name, $pd, $ed, $q, $u, $mq);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("$catId", item.CategoryId);
        cmd.Parameters.AddWithValue("$name", item.Name);
        cmd.Parameters.AddWithValue("$pd", item.PurchaseDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$ed", item.ExpiryDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$q", item.Quantity);
        cmd.Parameters.AddWithValue("$u", item.Unit);
        cmd.Parameters.AddWithValue("$mq", item.MinQuantity);

        var id = (long)(cmd.ExecuteScalar() ?? 0L);
        item.Id = id;
        return id;
    }

    public List<ResourceItem> GetAllResources()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT Id, CategoryId, Name, PurchaseDate, ExpiryDate, Quantity, Unit, MinQuantity
FROM Resources
ORDER BY ExpiryDate;
";

        using var reader = cmd.ExecuteReader();
        var list = new List<ResourceItem>();
        while (reader.Read())
        {
            list.Add(ReadResource(reader));
        }

        return list;
    }

    public List<(ResourceItem Item, string CategoryName)> GetResourcesWithCategories()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT r.Id, r.CategoryId, r.Name, r.PurchaseDate, r.ExpiryDate, r.Quantity, r.Unit, r.MinQuantity,
       c.Name
FROM Resources r
JOIN Categories c ON c.Id = r.CategoryId
ORDER BY r.ExpiryDate;
";

        using var reader = cmd.ExecuteReader();
        var list = new List<(ResourceItem, string)>();
        while (reader.Read())
        {
            var item = new ResourceItem
            {
                Id = reader.GetInt64(0),
                CategoryId = reader.GetInt64(1),
                Name = reader.GetString(2),
                PurchaseDate = DateTime.Parse(reader.GetString(3)),
                ExpiryDate = DateTime.Parse(reader.GetString(4)),
                Quantity = reader.GetDouble(5),
                Unit = reader.GetString(6),
                MinQuantity = reader.GetDouble(7)
            };
            var categoryName = reader.GetString(8);
            list.Add((item, categoryName));
        }

        return list;
    }

    public List<ResourceItem> GetResourcesByState(ResourceState state, DateTime now)
    {
        // Берём все и фильтруем в памяти — для простоты.
        var all = GetAllResources();
        return all.Where(r => r.GetState(now) == state).ToList();
    }

    public List<ResourceItem> GetLowOnStockResources()
    {
        var all = GetAllResources();
        return all.Where(r => r.IsLowOnStock()).ToList();
    }

    public void DeleteResource(long id)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Resources WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void DeleteCategory(long id)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Categories WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void UpdateResourceQuantity(long id, double newQuantity)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Resources SET Quantity = $q WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$q", newQuantity);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static ResourceItem ReadResource(SqliteDataReader reader)
    {
        return new ResourceItem
        {
            Id = reader.GetInt64(0),
            CategoryId = reader.GetInt64(1),
            Name = reader.GetString(2),
            PurchaseDate = DateTime.Parse(reader.GetString(3)),
            ExpiryDate = DateTime.Parse(reader.GetString(4)),
            Quantity = reader.GetDouble(5),
            Unit = reader.GetString(6),
            MinQuantity = reader.GetDouble(7)
        };
    }
}


