using Microsoft.Data.Sqlite;

namespace ExpirySpy;

public static class Database
{
    private const string DatabaseFileName = "expiryspy.db";

    public static string GetConnectionString()
    {
        return $"Data Source={DatabaseFileName}";
    }

    public static void EnsureCreated()
    {
        using var connection = new SqliteConnection(GetConnectionString());
        connection.Open();

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    ParentId INTEGER NULL,
    DefaultQuantity REAL NULL,
    DefaultUnit TEXT NULL,
    DefaultMinQuantity REAL NULL,
    FOREIGN KEY (ParentId) REFERENCES Categories(Id)
);
";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Resources (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    PurchaseDate TEXT NOT NULL,
    ExpiryDate TEXT NOT NULL,
    Quantity REAL NOT NULL,
    Unit TEXT NOT NULL,
    MinQuantity REAL NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
";
            cmd.ExecuteNonQuery();
        }
    }
}


