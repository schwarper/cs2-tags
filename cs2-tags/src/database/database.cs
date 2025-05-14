using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dapper;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using System.Data.Common;
using System.Reflection;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class Database
{
    private static bool UseMySQL;
    private static string GlobalDatabaseConnectionString = string.Empty;

    internal static void SetDatabase(Config config)
    {
        UseMySQL = config.DatabaseConnection.MySQL;

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        
        if (!UseMySQL)
        {
            string configDir = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", assemblyName);
            string dbFile = Path.Combine(configDir, $"{assemblyName}.db");
            
            try
            {
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                    Console.WriteLine($"Created directory: {configDir}");
                }
                
                string testFile = Path.Combine(configDir, ".write_test");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    Console.WriteLine("Directory is writable");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Directory is not writable: {ex.Message}");
                }
                
                if (File.Exists(dbFile))
                {
                    try
                    {
                        FileStream fs = File.Open(dbFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        fs.Close();
                        Console.WriteLine("Database file is writable");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WARNING: Database file is not writable: {ex.Message}");
                        
                        try
                        {
                            File.SetAttributes(dbFile, File.GetAttributes(dbFile) & ~FileAttributes.ReadOnly);
                            Console.WriteLine("Attempted to make database file writable");
                        }
                        catch (Exception permEx)
                        {
                            Console.WriteLine($"Failed to update file permissions: {permEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking/creating database directory: {ex.Message}");
            }
            
            GlobalDatabaseConnectionString = $"Data Source={dbFile}";
        }
        else
        {
            GlobalDatabaseConnectionString = new MySqlConnectionStringBuilder
            {
                Server = config.DatabaseConnection.Host,
                Database = config.DatabaseConnection.Name,
                UserID = config.DatabaseConnection.User,
                Password = config.DatabaseConnection.Password,
                Port = config.DatabaseConnection.Port,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 640,
                ConnectionIdleTimeout = 30,
                AllowZeroDateTime = true
            }.ConnectionString;
        }
        
        Console.WriteLine($"Database initialized: {(UseMySQL ? "MySQL" : "SQLite")}");
    }

    public static async Task<DbConnection> ConnectAsync()
    {
        DbConnection connection;

        if (UseMySQL)
        {
            connection = new MySqlConnection(GlobalDatabaseConnectionString);
        }
        else
        {
            SQLitePCL.Batteries.Init();
            connection = new SqliteConnection(GlobalDatabaseConnectionString);
        }

        await connection.OpenAsync();
        return connection;
    }

    public static void CreateDatabase(Config config)
    {
        SetDatabase(config);

        Task.Run(CreateDatabaseAsync);
    }

    public static async Task CreateDatabaseAsync()
    {
        try
        {
            using DbConnection connection = await ConnectAsync();

            if (UseMySQL)
            {
                await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS tags (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        SteamID BIGINT UNSIGNED NOT NULL,
                        ScoreTag VARCHAR(255),
                        ChatTag VARCHAR(255),
                        ChatColor VARCHAR(255),
                        NameColor VARCHAR(255),
                        Visibility BOOLEAN NOT NULL DEFAULT TRUE
                    );
                ");
                
                bool columnExists = false;
                try 
                {
                    var columns = await connection.QueryAsync<string>(@"
                        SELECT COLUMN_NAME 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'tags' AND COLUMN_NAME = 'IsExternal'
                    ");
                    
                    columnExists = columns.Any();
                    Console.WriteLine($"Checking for IsExternal column: {(columnExists ? "exists" : "does not exist")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking for IsExternal column: {ex.Message}");
                }
                
                if (!columnExists)
                {
                    try
                    {
                        await connection.ExecuteAsync(@"
                            ALTER TABLE tags ADD COLUMN IsExternal BOOLEAN NOT NULL DEFAULT FALSE
                        ");
                        Console.WriteLine("Added IsExternal column to tags table.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding IsExternal column: {ex.Message}");
                    }
                }
            }
            else
            {
                await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS tags (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SteamID BIGINT NOT NULL,
                        ScoreTag TEXT,
                        ChatTag TEXT,
                        ChatColor TEXT,
                        NameColor TEXT,
                        Visibility BOOLEAN NOT NULL DEFAULT 1
                    );
                ");
                
                bool columnExists = false;
                try 
                {
                    var tableInfo = await connection.QueryAsync<dynamic>("PRAGMA table_info(tags)");
                    
                    foreach (var column in tableInfo)
                    {
                        string name = column.name?.ToString() ?? "";
                        if (name.Equals("IsExternal", StringComparison.OrdinalIgnoreCase))
                        {
                            columnExists = true;
                            break;
                        }
                    }
                    Console.WriteLine($"Checking for SQLite IsExternal column: {(columnExists ? "exists" : "does not exist")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking SQLite column existence: {ex.Message}");
                }
                
                if (!columnExists)
                {
                    try
                    {
                        await connection.ExecuteAsync(@"
                            ALTER TABLE tags ADD COLUMN IsExternal BOOLEAN NOT NULL DEFAULT 0
                        ");
                        Console.WriteLine("Added IsExternal column to SQLite tags table.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding SQLite IsExternal column: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization error: {ex.Message}");
        }
    }

    public static void LoadPlayer(CCSPlayerController player)
    {
        if (player == null || !player.IsValid)
        {
            Console.WriteLine("Attempted to load an invalid player");
            return;
        }
        
        try
        {
            ulong steamId = player.SteamID;
            Server.NextFrame(async () => await LoadPlayerAsync(steamId, player));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preparing to load player: {ex.Message}");
        }
    }

    public static async Task LoadPlayerAsync(ulong steamId, CCSPlayerController player)
    {
        if (player == null || !player.IsValid)
        {
            Console.WriteLine($"Player became invalid during LoadPlayerAsync (SteamID: {steamId})");
            return;
        }
        
        try
        {
            using DbConnection connection = await ConnectAsync();

            Tag? result = (await connection.QueryAsync<Tag>(@"
                SELECT * FROM tags WHERE SteamID = @SteamID;", new { SteamID = steamId }
            )).SingleOrDefault();

            if (result == null)
            {
                await InsertNewPlayer(connection, steamId, player);
                return;
            }

            await RefreshPlayerTagsAsync(steamId, player, result);

            if (!PlayerTagsList.ContainsKey(steamId))
            {
                Tag playerTag = PlayerTagsList.TryGetValue(steamId, out Tag? existingTag)
                    ? existingTag
                    : result;

                PlayerTagsList[steamId] = playerTag;
                player.SetScoreTag(playerTag.ScoreTag);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading player tags (SteamID: {steamId}): {ex.Message}");

            if (!PlayerTagsList.ContainsKey(steamId))
            {
                if (player != null && player.IsValid)
                {
                    Tag defaultTag = player.GetTag();
                    PlayerTagsList[steamId] = defaultTag;
                    player.SetScoreTag(defaultTag.ScoreTag);
                }
            }
        }
    }

    public static async Task InsertNewPlayer(DbConnection connection, ulong steamId, CCSPlayerController player)
    {
        try
        {
            // Check player validity again
            if (player == null || !player.IsValid)
            {
                Console.WriteLine($"Player became invalid during InsertNewPlayer (SteamID: {steamId})");
                return;
            }
            
            Tag playerData = player.GetTag();
            
            playerData.IsExternal = false;
            
            PlayerTagsList[steamId] = playerData;
            player.SetScoreTag(playerData.ScoreTag);

            await connection.ExecuteAsync(@"
                INSERT INTO tags (SteamID, ScoreTag, ChatTag, ChatColor, NameColor, Visibility, IsExternal) 
                VALUES (@SteamID, @ScoreTag, @ChatTag, @ChatColor, @NameColor, @Visibility, @IsExternal);
            ", new
            {
                SteamID = steamId,
                playerData.ScoreTag,
                playerData.ChatTag,
                playerData.ChatColor,
                playerData.NameColor,
                playerData.Visibility,
                playerData.IsExternal
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting new player (SteamID: {steamId}): {ex.Message}");
        }
    }

    public static void SavePlayer(CCSPlayerController player)
    {
        if (player == null || !player.IsValid)
        {
            Console.WriteLine("Attempted to save an invalid player");
            return;
        }
        
        try 
        {
            ulong steamId = player.SteamID;
            
            if (PlayerTagsList.TryGetValue(steamId, out Tag? tags))
            {
                Tag tagsCopy = new()
                {
                    ScoreTag = tags.ScoreTag,
                    ChatTag = tags.ChatTag,
                    ChatColor = tags.ChatColor,
                    NameColor = tags.NameColor,
                    Visibility = tags.Visibility,
                    ChatSound = tags.ChatSound,
                    IsExternal = tags.IsExternal
                };
                
                Server.NextFrame(async () => await SavePlayerAsync(steamId, tagsCopy));
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Error preparing to save player: {ex.Message}");
        }
    }

    public static async Task SavePlayerAsync(ulong steamId, Tag playerData)
    {
        try
        {
            using DbConnection connection = await ConnectAsync();

            await connection.ExecuteAsync(@"
                UPDATE tags SET 
                    ScoreTag = @ScoreTag,
                    ChatTag = @ChatTag,
                    ChatColor = @ChatColor,
                    NameColor = @NameColor,
                    Visibility = @Visibility,
                    IsExternal = @IsExternal
                WHERE SteamID = @SteamID;
            ", new
            {
                playerData.ScoreTag,
                playerData.ChatTag,
                playerData.ChatColor,
                playerData.NameColor,
                playerData.Visibility,
                playerData.IsExternal,
                SteamID = steamId
            });

            PlayerTagsList.Remove(steamId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving player data (SteamID: {steamId}): {ex.Message}");
        }
    }

    public static async Task RefreshPlayerTagsAsync(ulong steamId, CCSPlayerController player, Tag dbTag)
    {
        if (player == null || !player.IsValid)
        {
            Console.WriteLine($"Player became invalid during RefreshPlayerTagsAsync (SteamID: {steamId})");
            return;
        }
        
        try
        {
            if (dbTag.IsExternal)
            {
                PlayerTagsList[steamId] = dbTag;
                player.SetScoreTag(dbTag.ScoreTag);
                return;
            }
            
            Tag currentPermissionTag = player.GetTag();

            bool needsUpdate = false;

            if (currentPermissionTag.ScoreTag != dbTag.ScoreTag)
                needsUpdate = true;
            if (currentPermissionTag.ChatTag != dbTag.ChatTag)
                needsUpdate = true;
            if (currentPermissionTag.ChatColor != dbTag.ChatColor)
                needsUpdate = true;
            if (currentPermissionTag.NameColor != dbTag.NameColor)
                needsUpdate = true;

            if (needsUpdate)
            {
                currentPermissionTag.Visibility = dbTag.Visibility;
                currentPermissionTag.ChatSound = dbTag.ChatSound;
                currentPermissionTag.IsExternal = false;
                
                Console.WriteLine($"Updating player tags due to permission change (SteamID: {steamId})");

                PlayerTagsList[steamId] = currentPermissionTag;
                player.SetScoreTag(currentPermissionTag.ScoreTag);
                
                using DbConnection connection = await ConnectAsync();
                await connection.ExecuteAsync(@"
                    UPDATE tags SET 
                        ScoreTag = @ScoreTag,
                        ChatTag = @ChatTag,
                        ChatColor = @ChatColor,
                        NameColor = @NameColor,
                        IsExternal = @IsExternal
                    WHERE SteamID = @SteamID;
                ", new
                {
                    currentPermissionTag.ScoreTag,
                    currentPermissionTag.ChatTag,
                    currentPermissionTag.ChatColor,
                    currentPermissionTag.NameColor,
                    currentPermissionTag.IsExternal,
                    SteamID = steamId
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing player tags (SteamID: {steamId}): {ex.Message}");
        }
    }
}
