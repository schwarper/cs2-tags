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
        SQLitePCL.Batteries.Init();
        DbConnection connection = UseMySQL ? new MySqlConnection(GlobalDatabaseConnectionString) : new SqliteConnection(GlobalDatabaseConnectionString);
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
        Server.NextFrame(async () => await LoadPlayerAsync(player));
    }

    public static async Task LoadPlayerAsync(CCSPlayerController player)
    {
        try
        {
            using DbConnection connection = await ConnectAsync();

            Tag? result = (await connection.QueryAsync<Tag>(@"
                SELECT * FROM tags WHERE SteamID = @SteamID;", new { player.SteamID }
            )).SingleOrDefault();

            if (result == null)
            {
                await InsertNewPlayer(connection, player);
                return;
            }

            // Check if player's permissions have changed and refresh tags if needed
            await RefreshPlayerTagsAsync(player, result);

            // Make sure player has tags in the dictionary (uses updated tags from RefreshPlayerTagsAsync if changed)
            if (!PlayerTagsList.ContainsKey(player.SteamID))
            {
                Tag playerTag = PlayerTagsList.TryGetValue(player.SteamID, out Tag? existingTag)
                    ? existingTag
                    : result;

                PlayerTagsList[player.SteamID] = playerTag;
                player.SetScoreTag(playerTag.ScoreTag);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading player tags: {ex.Message}");

            if (!PlayerTagsList.ContainsKey(player.SteamID))
            {
                Tag defaultTag = player.GetTag();
                PlayerTagsList[player.SteamID] = defaultTag;
                player.SetScoreTag(defaultTag.ScoreTag);
            }
        }
    }

    public static async Task InsertNewPlayer(DbConnection connection, CCSPlayerController player)
    {
        try
        {
            Tag playerData = player.GetTag();
            
            // By default, tags from config-based permissions are not external
            playerData.IsExternal = false;
            
            PlayerTagsList[player.SteamID] = playerData;
            player.SetScoreTag(playerData.ScoreTag);

            await connection.ExecuteAsync(@"
                INSERT INTO tags (SteamID, ScoreTag, ChatTag, ChatColor, NameColor, Visibility, IsExternal) 
                VALUES (@SteamID, @ScoreTag, @ChatTag, @ChatColor, @NameColor, @Visibility, @IsExternal);
            ", new
            {
                player.SteamID,
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
            Console.WriteLine($"Error inserting new player: {ex.Message}");
        }
    }

    public static void SavePlayer(CCSPlayerController player)
    {
        if (PlayerTagsList.TryGetValue(player.SteamID, out Tag? tags))
        {
            var steamid = player.SteamID;
            Server.NextFrame(async () => await SavePlayerAsync(steamid, tags));
        }
    }

    public static async Task SavePlayerAsync(ulong SteamID, Tag playerData)
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
                SteamID
            });

            PlayerTagsList.Remove(SteamID);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving player data: {ex.Message}");
        }
    }

    public static async Task RefreshPlayerTagsAsync(CCSPlayerController player, Tag dbTag)
    {
        try
        {
            // If this is an external tag, don't refresh based on permissions
            if (dbTag.IsExternal)
            {
                PlayerTagsList[player.SteamID] = dbTag;
                player.SetScoreTag(dbTag.ScoreTag);
                return;
            }
            
            // Get what the player's tag SHOULD be based on current permissions
            Tag currentPermissionTag = player.GetTag();

            // Check if the role-based tags don't match what's in the database
            bool needsUpdate = false;

            // Check each tag type to see if it needs updating
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
                
                Console.WriteLine($"Updating player {player.PlayerName} tags due to permission change");

                PlayerTagsList[player.SteamID] = currentPermissionTag;
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
                    player.SteamID
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing player tags: {ex.Message}");
        }
    }
}