using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace NoisEvader
{
    public static class Database
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const string DbPath = @"Database.sqlite";

        public static void EnsureExists()
        {
            logger.Log(LogLevel.Info, "Making sure DB exists");

            var assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream("NoisEvader.CreateDb.sql");
            using StreamReader reader = new StreamReader(stream);
            string sql = reader.ReadToEnd();

            DoQuery(command =>
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            });
        }

        public static CachedLevelData GetLevelData(string xmlHash)
        {
            return DoQuery(command =>
            {
                command.CommandText = @"
                    WITH level_scores AS 
                       (SELECT s.percent, s.heart_gotten, s.mod_game_speed
                       FROM level_data l JOIN scores s
                       ON l.level_id = s.level_id
                       WHERE l.hash = $hash)
                    SELECT
                        d.*,
                        (SELECT percent
                            FROM level_scores
                            WHERE mod_game_speed >= 1
                            ORDER BY percent DESC LIMIT 1) AS best_score,
                        (SELECT COUNT(1)
                            FROM level_scores
                            WHERE heart_gotten = '1') AS heart_gotten,
                        IFNULL(s.invert_colors, 'false') AS invert_colors,
                        IFNULL(s.thirty_ticks, 'false') AS thirty_ticks,
                        IFNULL(s.naive_warp, 'false') AS naive_warp
                     FROM
                        level_data d LEFT JOIN level_settings s ON d.level_id = s.level_id
                     WHERE d.hash = $hash";
                command.Parameters.AddWithValue("$hash", xmlHash);

                using var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return null;

                var cld = new CachedLevelData();
                cld.XmlHash = xmlHash;
                while (reader.Read())
                {
                    cld.Info.Nick = reader.GetString(reader.GetOrdinal("nick"));
                    cld.Info.AudioFile = reader.GetString(reader.GetOrdinal("mp3_name"));
                    cld.Info.Enemies = reader.GetInt32(reader.GetOrdinal("enemies"));
                    cld.Info.Title = reader.GetString(reader.GetOrdinal("title"));
                    cld.Info.Artist = reader.GetString(reader.GetOrdinal("artist"));
                    cld.Info.Difficulty = reader.GetInt32(reader.GetOrdinal("difficulty"));
                    cld.Info.Designer = reader.GetString(reader.GetOrdinal("designer"));
                    cld.Info.HasHeart = reader.GetBoolean(reader.GetOrdinal("has_heart"));
                    cld.Info.AudioPreviewPoint = reader.GetInt32(reader.GetOrdinal("audio_preview_point"));
                    cld.Info.AdvancedFlag = reader.GetBoolean(reader.GetOrdinal("advanced_flag"));
                    var audioDurationOrdinal = reader.GetOrdinal("audio_duration");
                    cld.AudioDuration = reader.IsDBNull(audioDurationOrdinal)
                        ? null
                        : (TimeSpan?)TimeSpan.FromMilliseconds(reader.GetDouble(audioDurationOrdinal));
                    cld.Playcount = reader.GetInt32(reader.GetOrdinal("playcount"));
                    var scoreOrdinal = reader.GetOrdinal("best_score");
                    cld.BestScore = reader.IsDBNull(scoreOrdinal)
                        ? null
                        : reader.GetFloat(scoreOrdinal);
                    cld.HeartGotten = reader.GetInt32(reader.GetOrdinal("heart_gotten")) > 0;

                    var settings = new LevelSettings
                    {
                        InvertColors = reader.GetBoolean(reader.GetOrdinal("invert_colors")),
                        ThirtyTicks = reader.GetBoolean(reader.GetOrdinal("thirty_ticks")),
                        NaiveWarp = reader.GetBoolean(reader.GetOrdinal("naive_warp")),
                    };
                    cld.Settings = settings;
                }
                return cld;
            });
        }

        public static void UpdateLevelSettings(string xmlHash, LevelSettings settings) =>
            DoQuery(command =>
            {
                command.CommandText =
                    @"UPDATE
                        level_settings
                    SET
                        invert_colors = $invert_colors, 
                        thirty_ticks = $thirty_ticks,
                        naive_warp = $naive_warp
                    WHERE
                        level_id = (SELECT level_id FROM level_data WHERE hash = $hash);

                    INSERT OR IGNORE INTO
                        level_settings (level_id, invert_colors, thirty_ticks, naive_warp)
                    VALUES 
                        ((SELECT level_id FROM level_data WHERE hash = $hash),
                        $invert_colors, $thirty_ticks, $naive_warp)
                    ";
                command.Parameters.AddWithValue("$hash", xmlHash);
                command.Parameters.AddWithValue("$invert_colors", settings.InvertColors);
                command.Parameters.AddWithValue("$thirty_ticks", settings.ThirtyTicks);
                command.Parameters.AddWithValue("$naive_warp", settings.NaiveWarp);
                command.ExecuteNonQuery();
            });

        public static void InsertLevelData(CachedLevelData cld) =>
            DoQuery(command =>
            {
                command.CommandText =
                    @"INSERT INTO level_data 
                        (hash, nick, mp3_name, enemies, title, artist, difficulty, designer,
                        has_heart, audio_preview_point, advanced_flag,
                        audio_duration)
                    VALUES 
                        ($hash, $nick, $mp3_name, $enemies, $title, $artist, $difficulty, $designer,
                        $has_heart, $audio_preview_point, $advanced_flag,
                        $audio_duration)
                    ";
                // https://www.youtube.com/watch?v=itr10Q8Klik
                command.Parameters.AddWithValue("$hash", cld.XmlHash);
                command.Parameters.AddWithValue("$nick", cld.Info.Nick);
                command.Parameters.AddWithValue("$mp3_name", Path.GetFileName(cld.Info.AudioFile));
                command.Parameters.AddWithValue("$enemies", cld.Info.Enemies);
                command.Parameters.AddWithValue("$title", cld.Info.Title);
                command.Parameters.AddWithValue("$artist", cld.Info.Artist);
                command.Parameters.AddWithValue("$difficulty", cld.Info.Difficulty);
                command.Parameters.AddWithValue("$designer", cld.Info.Designer);
                command.Parameters.AddWithValue("$has_heart", cld.Info.HasHeart);
                command.Parameters.AddWithValue("$audio_preview_point", cld.Info.AudioPreviewPoint);
                command.Parameters.AddWithValue("$advanced_flag", cld.Info.AdvancedFlag);
                if (cld.AudioDuration is null)
                    command.Parameters.AddWithValue("$audio_duration", DBNull.Value);
                else
                    command.Parameters.AddWithValue("$audio_duration", cld.AudioDuration.Value.TotalMilliseconds);
                command.ExecuteNonQuery();
            });

        public static bool HasLevelData(string xmlHash) =>
            DoQuery(command =>
            {
                command.CommandText =
                @"SELECT 1
                FROM level_data
                WHERE hash = $hash";
                command.Parameters.AddWithValue("$hash", xmlHash);

                return command.ExecuteScalar() != null;
            });

        public static TimeSpan? GetAudioDuration(string xmlHash) =>
            DoQuery(command =>
            {
                command.CommandText =
                @"SELECT audio_duration
                FROM level_data
                WHERE hash = $hash";
                command.Parameters.AddWithValue("$hash", xmlHash);

                var duration = command.ExecuteScalar();
                if (duration is null)
                    return (TimeSpan?)null;
                else
                    return TimeSpan.FromMilliseconds((double)duration);
            });

        public static void UpdateAudioDuration(string xmlHash, TimeSpan? duration) =>
            DoQuery(command =>
            {
                command.CommandText =
                @"UPDATE level_data
                SET audio_duration = $audio_duration
                WHERE hash = $hash";
                command.Parameters.AddWithValue("$hash", xmlHash);
                if (duration is null || duration.Value == TimeSpan.Zero)
                    command.Parameters.AddWithValue("$audio_duration", DBNull.Value);
                else
                    command.Parameters.AddWithValue("$audio_duration", duration.Value.TotalMilliseconds);
                command.ExecuteNonQuery();
            });

        public static void SaveScore(Score score) =>
            DoQuery(command =>
            {
                command.CommandText =
                @"INSERT INTO scores
                    (level_id, 
                    time, percent, total_hits, heart_gotten, total_slomo,
                    mod_flags, mod_game_speed, mod_tick_rate,
                    replay_file)
                 VALUES
                    ((SELECT level_id FROM level_data WHERE hash = $hash),
                    $time, $percent, $total_hits, $heart_gotten, $total_slomo,
                    $mod_flags, $mod_game_speed, $mod_tick_rate,
                    $replay_file)
                ";
                command.Parameters.AddWithValue("$hash", score.XmlHash);
                command.Parameters.AddWithValue("$time", score.Time);
                command.Parameters.AddWithValue("$percent", score.Percent);
                command.Parameters.AddWithValue("$total_hits", score.TotalHits);
                command.Parameters.AddWithValue("$heart_gotten", score.HeartGotten);
                command.Parameters.AddWithValue("$total_slomo", score.TotalSlomoTime);
                command.Parameters.AddWithValue("$mod_flags", score.Mod.Flags);
                command.Parameters.AddWithValue("$mod_game_speed", score.Mod.GameSpeed);
                if (score.Mod.TickRate is { } tr)
                    command.Parameters.AddWithValue("$mod_tick_rate", tr);
                else
                    command.Parameters.AddWithValue("$mod_tick_rate", DBNull.Value);
                command.Parameters.AddWithValue("$replay_file", score.ReplayFile);
                command.ExecuteNonQuery();
            });

        public static List<Score> GetScores(string xmlHash) =>
            DoQuery(command =>
            {
                command.CommandText =
                @"SELECT *
                  FROM scores
                  WHERE level_id = (SELECT level_id FROM level_data WHERE hash = $hash)";
                command.Parameters.AddWithValue("$hash", xmlHash);

                var reader = command.ExecuteReader();
                var scores = new List<Score>();
                while (reader.Read())
                {
                    var score = new Score()
                    {
                        XmlHash = xmlHash,
                        Time = reader.GetDateTime(reader.GetOrdinal("time")),
                        Percent = reader.GetFloat(reader.GetOrdinal("percent")),
                        TotalHits = (uint)reader.GetInt32(reader.GetOrdinal("total_hits")),
                        HeartGotten = reader.GetBoolean(reader.GetOrdinal("heart_gotten")),
                        TotalSlomoTime = reader.GetFloat(reader.GetOrdinal("total_slomo")),
                        ReplayFile = reader.GetString(reader.GetOrdinal("replay_file")),
                    };
                    score.Mod = new Mod()
                    {
                        Flags = (ModFlags)reader.GetInt32(reader.GetOrdinal("mod_flags")),
                        GameSpeed = reader.GetFloat(reader.GetOrdinal("mod_game_speed")),
                        TickRate = reader.IsDBNull(reader.GetOrdinal("mod_tick_rate"))
                            ? (float?)null
                            : reader.GetFloat(reader.GetOrdinal("mod_tick_rate")),
                    };
                    scores.Add(score);
                }

                return scores;
            });

        public static void IncrementPlaycount(string xmlHash) =>
            DoQuery(command =>
            {
                command.CommandText =
                @"UPDATE level_data
                SET playcount = playcount + 1
                WHERE hash = $hash";
                command.Parameters.AddWithValue("$hash", xmlHash);
                command.ExecuteNonQuery();
            });

        private static T DoQuery<T>(Func<SqliteCommand, T> query)
        {
            var sb = new SqliteConnectionStringBuilder();
            sb.DataSource = DbPath;

            using var connection = new SqliteConnection(sb.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();

            var result = query.Invoke(command);

            //Debug.WriteLine(command.CommandText);
            return result;
        }

        private static void DoQuery(Action<SqliteCommand> query)
        {
            var sb = new SqliteConnectionStringBuilder();
            sb.DataSource = DbPath;

            using var connection = new SqliteConnection(sb.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();

            query.Invoke(command);

            //Debug.WriteLine(command.CommandText);
        }
    }
}
