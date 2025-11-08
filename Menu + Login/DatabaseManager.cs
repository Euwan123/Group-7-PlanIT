using System;
using System.Collections.Generic;
using System.Drawing;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PlanIT2.TabTypes;

namespace PlanIT2.Database
{
    public class DatabaseManager
    {
        private string connectionString;

        // Constructor with default XAMPP values
        public DatabaseManager(string server = "127.0.0.1", string database = "planit",
                              string user = "root", string password = "")
        {
            connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};";
        }

        // Get MySqlConnection (for compatibility with existing Database.cs usage)
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // Test connection
        public bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }

        // Initialize database tables (run this once)
        public bool InitializeDatabase()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS tabs (
                            id INT PRIMARY KEY AUTO_INCREMENT,
                            title VARCHAR(255) NOT NULL,
                            type VARCHAR(50) NOT NULL,
                            content LONGTEXT,
                            tab_color VARCHAR(7) DEFAULT '#FFFFFF',
                            ai_history LONGTEXT,
                            is_deleted BOOLEAN DEFAULT FALSE,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                            INDEX idx_type (type),
                            INDEX idx_title (title),
                            INDEX idx_deleted (is_deleted)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

                    using (var cmd = new MySqlCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                return false;
            }
        }

        // Save a tab to database
        public int SaveTab(TabData tabData)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO tabs (title, type, content, tab_color, ai_history) 
                                   VALUES (@title, @type, @content, @color, @ai_history);
                                   SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", tabData.Title);
                        cmd.Parameters.AddWithValue("@type", tabData.Type);
                        cmd.Parameters.AddWithValue("@content", tabData.Content ?? "");
                        cmd.Parameters.AddWithValue("@color", ColorToHex(tabData.TabColor));
                        cmd.Parameters.AddWithValue("@ai_history", tabData.AiHistory ?? "");

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save failed: {ex.Message}");
                return -1;
            }
        }

        // Update existing tab
        public bool UpdateTab(int id, TabData tabData)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE tabs 
                                   SET title = @title, 
                                       type = @type, 
                                       content = @content, 
                                       tab_color = @color, 
                                       ai_history = @ai_history,
                                       updated_at = CURRENT_TIMESTAMP
                                   WHERE id = @id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@title", tabData.Title);
                        cmd.Parameters.AddWithValue("@type", tabData.Type);
                        cmd.Parameters.AddWithValue("@content", tabData.Content ?? "");
                        cmd.Parameters.AddWithValue("@color", ColorToHex(tabData.TabColor));
                        cmd.Parameters.AddWithValue("@ai_history", tabData.AiHistory ?? "");

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.Message}");
                return false;
            }
        }

        // Load all tabs
        public List<TabDataWithId> LoadAllTabs()
        {
            var tabs = new List<TabDataWithId>();

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, title, type, content, tab_color, ai_history, 
                                          created_at, updated_at 
                                   FROM tabs 
                                   WHERE is_deleted = FALSE 
                                   ORDER BY updated_at DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tabs.Add(new TabDataWithId
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Type = reader.GetString("type"),
                                Content = reader.IsDBNull(reader.GetOrdinal("content"))
                                    ? "" : reader.GetString("content"),
                                TabColor = HexToColor(reader.GetString("tab_color")),
                                AiHistory = reader.IsDBNull(reader.GetOrdinal("ai_history"))
                                    ? "" : reader.GetString("ai_history"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                UpdatedAt = reader.GetDateTime("updated_at")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load failed: {ex.Message}");
            }

            return tabs;
        }

        // Load single tab by ID
        public TabDataWithId LoadTab(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, title, type, content, tab_color, ai_history, 
                                          created_at, updated_at 
                                   FROM tabs 
                                   WHERE id = @id AND is_deleted = FALSE";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new TabDataWithId
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Type = reader.GetString("type"),
                                    Content = reader.IsDBNull(reader.GetOrdinal("content"))
                                        ? "" : reader.GetString("content"),
                                    TabColor = HexToColor(reader.GetString("tab_color")),
                                    AiHistory = reader.IsDBNull(reader.GetOrdinal("ai_history"))
                                        ? "" : reader.GetString("ai_history"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load single tab failed: {ex.Message}");
            }

            return null;
        }

        // Delete tab (soft delete)
        public bool DeleteTab(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE tabs SET is_deleted = TRUE WHERE id = @id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete failed: {ex.Message}");
                return false;
            }
        }

        // Permanently delete tab
        public bool PermanentlyDeleteTab(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM tabs WHERE id = @id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Permanent delete failed: {ex.Message}");
                return false;
            }
        }

        // Search tabs
        public List<TabDataWithId> SearchTabs(string searchTerm)
        {
            var tabs = new List<TabDataWithId>();

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, title, type, content, tab_color, ai_history, 
                                          created_at, updated_at 
                                   FROM tabs 
                                   WHERE is_deleted = FALSE 
                                   AND (title LIKE @search OR content LIKE @search)
                                   ORDER BY updated_at DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tabs.Add(new TabDataWithId
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Type = reader.GetString("type"),
                                    Content = reader.IsDBNull(reader.GetOrdinal("content"))
                                        ? "" : reader.GetString("content"),
                                    TabColor = HexToColor(reader.GetString("tab_color")),
                                    AiHistory = reader.IsDBNull(reader.GetOrdinal("ai_history"))
                                        ? "" : reader.GetString("ai_history"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search failed: {ex.Message}");
            }

            return tabs;
        }

        // Get tabs by type
        public List<TabDataWithId> GetTabsByType(string type)
        {
            var tabs = new List<TabDataWithId>();

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, title, type, content, tab_color, ai_history, 
                                          created_at, updated_at 
                                   FROM tabs 
                                   WHERE is_deleted = FALSE AND type = @type
                                   ORDER BY updated_at DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@type", type);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tabs.Add(new TabDataWithId
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Type = reader.GetString("type"),
                                    Content = reader.IsDBNull(reader.GetOrdinal("content"))
                                        ? "" : reader.GetString("content"),
                                    TabColor = HexToColor(reader.GetString("tab_color")),
                                    AiHistory = reader.IsDBNull(reader.GetOrdinal("ai_history"))
                                        ? "" : reader.GetString("ai_history"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get by type failed: {ex.Message}");
            }

            return tabs;
        }

        // Helper: Color to Hex
        private string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        // Helper: Hex to Color
        private Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#"))
                return Color.White;

            try
            {
                return ColorTranslator.FromHtml(hex);
            }
            catch
            {
                return Color.White;
            }
        }
    }

    // Extended TabData with ID and timestamps
    public class TabDataWithId : TabData
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Static helper class for simple access (for backward compatibility with existing code)
    public static class Database
    {
        private static string connectionString = "server=127.0.0.1;user=root;password=;database=planit;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // Access to full DatabaseManager features
        private static DatabaseManager _manager;
        public static DatabaseManager Manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = new DatabaseManager();
                }
                return _manager;
            }
        }
    }
}