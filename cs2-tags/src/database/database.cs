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
            Tag playerDefaultTag = player.GetTag();
            string playerName = player.PlayerName;

            Server.NextFrame(async () =>
            {
                try
                {
                    using DbConnection connection = await ConnectAsync();

                    Tag? result = (await connection.QueryAsync<Tag>(@"
                        SELECT * FROM tags WHERE SteamID = @SteamID;",
                        new { SteamID = steamId }
                    )).SingleOrDefault();

                    if (result == null)
                    {
                        await InsertNewPlayerAsync(connection, steamId, new Tag
                        {
                            Visibility = true,
                            ChatSound = true,
                            IsExternal = false
                        });

                        Server.NextFrame(() =>
                        {
                            if (player != null && player.IsValid)
                            {
                                PlayerTagsList[steamId] = playerDefaultTag;
                                Tags.Api.TagsUpdatedPre(player, playerDefaultTag);
                                player.SetScoreTag(playerDefaultTag.ScoreTag);
                                Tags.Api.TagsUpdatedPost(player, playerDefaultTag);
                            }
                        });
                        return;
                    }

                    Tag finalTag = new()
                    {
                        Visibility = result.Visibility,
                        ChatSound = result.ChatSound,
                        IsExternal = result.IsExternal
                    };

                    Server.NextFrame(() =>
                    {
                        if (player != null && player.IsValid)
                        {
                            PlayerTagsList[steamId] = finalTag;
                            if (finalTag.Visibility)
                            {
                                Tags.Api.TagsUpdatedPre(player, finalTag);
                                player.SetScoreTag(finalTag.ScoreTag);
                                Tags.Api.TagsUpdatedPost(player, finalTag);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading player tags (SteamID: {steamId}): {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preparing to load player: {ex.Message}");
        }
    }
    
    private static async Task InsertNewPlayerAsync(DbConnection connection, ulong steamId, Tag playerData)
    {
        try
        {
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
            
            Console.WriteLine($"Successfully inserted new player (SteamID: {steamId})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting new player (SteamID: {steamId}): {ex.Message}");
        }
    }


    public static async Task InsertNewPlayer(DbConnection connection, ulong steamId, CCSPlayerController player)
    {
        try
        {
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
            return;

        try
        {
            ulong steamId = player.SteamID;

            if (PlayerTagsList.TryGetValue(steamId, out Tag? tags))
            {
                Tag tagsCopy = new()
                {
                    Visibility = tags.Visibility,
                    ChatSound = tags.ChatSound,
                    IsExternal = tags.IsExternal
                };

                Server.NextFrame(async () =>
                {
                    try
                    {
                        using DbConnection connection = await ConnectAsync();

                        await connection.ExecuteAsync(@"
                            UPDATE tags SET 
                                Visibility = @Visibility,
                                ChatSound = @ChatSound,
                                IsExternal = @IsExternal
                            WHERE SteamID = @SteamID;
                        ", new
                        {
                            tagsCopy.Visibility,
                            tagsCopy.ChatSound,
                            tagsCopy.IsExternal,
                            SteamID = steamId
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving player data (SteamID: {steamId}): {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preparing to save player: {ex.Message}");
        }
    }
}
