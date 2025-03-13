using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dapper;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using System.Data.Common;
using System.Reflection;
using static Tags.ConfigManager;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class Database
{
    private static bool UseMySQL => Config.DatabaseConnection.MySQL;
    public static string GlobalDatabaseConnectionString { get; set; } = string.Empty;

    public static void SetConnectionString(DatabaseConnection config)
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        string dbFile = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", assemblyName, $"{assemblyName}.db");

        GlobalDatabaseConnectionString = UseMySQL
            ? new MySqlConnectionStringBuilder
            {
                Server = config.Host,
                Database = config.Name,
                UserID = config.User,
                Password = config.Password,
                Port = config.Port,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 640,
                ConnectionIdleTimeout = 30,
                AllowZeroDateTime = true
            }.ConnectionString
            : $"Data Source={dbFile}";

        Console.WriteLine(dbFile);
    }

    public static async Task<DbConnection> ConnectAsync()
    {
        SQLitePCL.Batteries.Init();
        DbConnection connection = UseMySQL ? new MySqlConnection(GlobalDatabaseConnectionString) : new SqliteConnection(GlobalDatabaseConnectionString);
        await connection.OpenAsync();
        return connection;
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
        using DbConnection connection = await ConnectAsync();

        Tag? result = (await connection.QueryAsync<Tag>(@"
            SELECT * FROM tags WHERE SteamID = @SteamID;", new { player.SteamID }
        )).SingleOrDefault();

        if (result == null)
        {
            await InsertNewPlayer(connection, player);
            return;
        }

        player.UpdateTag(result);
    }

    public static async Task InsertNewPlayer(DbConnection connection, CCSPlayerController player)
    {
        Tag playerData = player.GetTag();
        player.UpdateTag(playerData);

        await connection.ExecuteAsync(@"
            INSERT INTO tags (SteamID, ScoreTag, ChatTag, ChatColor, NameColor, Visibility) 
            VALUES (@SteamID, @ScoreTag, @ChatTag, @ChatColor, @NameColor, 1);
        ", new
        {
            player.SteamID,
            playerData.ScoreTag,
            playerData.ChatTag,
            playerData.ChatColor,
            playerData.NameColor
        });
    }

    public static void SavePlayer(CCSPlayerController player)
    {
        CSSThread.RunOnMainThread(async () =>
        {
            await SavePlayerAsync(player);
        });
    }

    public static async Task SavePlayerAsync(CCSPlayerController player)
    {
        Tag playerData = PlayerTagsList[player];

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
            player.SteamID
        });

        PlayerTagsList.Remove(player);
    }
}