PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: level_data
CREATE TABLE IF NOT EXISTS
	level_data (level_id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE NOT NULL, hash TEXT UNIQUE NOT NULL, nick TEXT, mp3_name TEXT, enemies INT, title TEXT, artist TEXT, difficulty INT, designer TEXT, has_heart BOOLEAN, audio_preview_point INT, advanced_flag BOOLEAN, audio_duration DOUBLE, playcount INT NOT NULL DEFAULT (0));

-- Table: level_settings
CREATE TABLE IF NOT EXISTS
	level_settings (settings_id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE NOT NULL, level_id REFERENCES level_data (level_id) NOT NULL UNIQUE, invert_colors BOOLEAN DEFAULT (false), thirty_ticks BOOLEAN DEFAULT (false), naive_warp BOOLEAN DEFAULT (false));

-- Table: scores
CREATE TABLE IF NOT EXISTS 
	scores (score_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, level_id INTEGER REFERENCES level_data (level_id) NOT NULL, time DATETIME, percent DOUBLE, total_hits INT, heart_gotten BOOLEAN, mod_flags INT, mod_game_speed DOUBLE, mod_tick_rate DOUBLE, replay_file TEXT);

-- Index: hash_idx
CREATE UNIQUE INDEX IF NOT EXISTS 
	hash_idx ON level_data (level_id COLLATE NOCASE);

-- Index: scores_level_id_idx
CREATE INDEX IF NOT EXISTS 
	scores_level_id_idx ON scores (level_id);

-- Index: settings_level_id_idx
CREATE UNIQUE INDEX IF NOT EXISTS 
	settings_level_id_idx ON level_settings (level_id);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
