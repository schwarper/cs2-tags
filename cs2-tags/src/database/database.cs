using CounterStrikeSharp.API.Core;
using Dapper;
using MySqlConnector;
using static Tags.ConfigManager;
using static Tags.Tags;
using static TagsApi.Tags;

namespace Tags;

public static class Database
{
    public static string GlobalDatabaseConnectionString { get; set; } = string.Empty;

    public static void SetConnectionString(DatabaseConnection config) =>
        GlobalDatabaseConnectionString = new MySqlConnectionStringBuilder
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
        }.ConnectionString;

    public static async Task<MySqlConnection> ConnectAsync()
    {
        MySqlConnection connection = new(GlobalDatabaseConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static async Task CreateDatabaseAsync()
    {
        using MySqlConnection connection = await ConnectAsync();

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

    public static async Task LoadPlayer(CCSPlayerController player)
    {
        using MySqlConnection connection = await ConnectAsync();

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

    public static async Task InsertNewPlayer(MySqlConnection connection, CCSPlayerController player)
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

    public static async Task SavePlayer(CCSPlayerController player)
    {
        Tag playerData = PlayerTagsList[player];

        using MySqlConnection connection = await ConnectAsync();

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