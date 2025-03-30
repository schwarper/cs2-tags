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
        string dbFile = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", assemblyName, $"{assemblyName}.db");

        GlobalDatabaseConnectionString = UseMySQL
            ? new MySqlConnectionStringBuilder
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
            }.ConnectionString
            : $"Data Source={dbFile}";
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

        CSSThread.RunOnMainThread(async () => await CreateDatabaseAsync());
    }

    public static async Task CreateDatabaseAsync()
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
                    Visibility BOOLEAN NOT NULL
                );
            ");
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
                    Visibility BOOLEAN NOT NULL
                );
            ");
        }
    }

    public static void LoadPlayer(CCSPlayerController player)
    {
        CSSThread.RunOnMainThread(async () =>
        {
            await LoadPlayerAsync(player);
        });
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

            player.UpdateTag(playerData);

            await connection.ExecuteAsync(@"
                INSERT INTO tags (SteamID, ScoreTag, ChatTag, ChatColor, NameColor, Visibility) 
                VALUES (@SteamID, @ScoreTag, @ChatTag, @ChatColor, @NameColor, @Visibility);
            ", new
            {
                player.SteamID,
                playerData.ScoreTag,
                playerData.ChatTag,
                playerData.ChatColor,
                playerData.NameColor,
                playerData.Visibility
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
            CSSThread.RunOnMainThread(async () =>
            {
                await SavePlayerAsync(player.SteamID, tags);
            });
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
                    Visibility = @Visibility
                WHERE SteamID = @SteamID;
            ", new
            {
                playerData.ScoreTag,
                playerData.ChatTag,
                playerData.ChatColor,
                playerData.NameColor,
                playerData.Visibility,
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

                Console.WriteLine($"Updating player {player.PlayerName} tags due to permission change");

                PlayerTagsList[player.SteamID] = currentPermissionTag;
                player.SetScoreTag(currentPermissionTag.ScoreTag);

                // Update this dumbass in db
                using DbConnection connection = await ConnectAsync();
                await connection.ExecuteAsync(@"
                    UPDATE tags SET 
                        ScoreTag = @ScoreTag,
                        ChatTag = @ChatTag,
                        ChatColor = @ChatColor,
                        NameColor = @NameColor
                    WHERE SteamID = @SteamID;
                ", new
                {
                    currentPermissionTag.ScoreTag,
                    currentPermissionTag.ChatTag,
                    currentPermissionTag.ChatColor,
                    currentPermissionTag.NameColor,
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