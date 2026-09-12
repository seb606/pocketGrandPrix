using UnityEngine;
using System.Collections.Generic;

namespace PocketGP {
[System.Serializable]
public class ScoreEntry {
    public string Name;
    public string Country;
    public float Time;
    public int Track;
    public int Score;
}

[System.Serializable]
public class LeaderboardData {
    public List<ScoreEntry> Entries = new List<ScoreEntry>();
}

public static class GPLeaderboard {
    const string PrefsKey = "PocketGP_Leaderboard_v2";
    public static readonly string[] Countries = {
        "🇫🇷 France", "🇧🇪 Belgique", "🇨🇭 Suisse", "🇨🇦 Canada",
        "🇺🇸 USA", "🇬🇧 UK", "🇩🇪 Allemagne", "🇪🇸 Espagne",
        "🇮🇹 Italie", "🇯🇵 Japon", "🇧🇷 Brésil", "🌍 Monde"
    };

    static LeaderboardData data;

    static void Load() {
        if (data != null) return;
        string json = PlayerPrefs.GetString(PrefsKey, "");
        if (!string.IsNullOrEmpty(json)) {
            try {
                data = JsonUtility.FromJson<LeaderboardData>(json);
            } catch {
                data = null;
            }
        }
        if (data == null || data.Entries == null || data.Entries.Count == 0) {
            InitDefaultScores();
        }
    }

    static void InitDefaultScores() {
        data = new LeaderboardData();
        // Quelques scores d'arcade pour donner du challenge au lancement
        data.Entries.Add(new ScoreEntry { Name = "TurboMax", Country = "🇫🇷 France", Time = 48.20f, Track = 0, Score = 500 });
        data.Entries.Add(new ScoreEntry { Name = "Speedy", Country = "🇧🇪 Belgique", Time = 51.45f, Track = 0, Score = 400 });
        data.Entries.Add(new ScoreEntry { Name = "Viper", Country = "🇨🇭 Suisse", Time = 54.10f, Track = 0, Score = 300 });

        data.Entries.Add(new ScoreEntry { Name = "DriftKing", Country = "🇯🇵 Japon", Time = 53.80f, Track = 1, Score = 600 });
        data.Entries.Add(new ScoreEntry { Name = "Shadow", Country = "🇺🇸 USA", Time = 57.20f, Track = 1, Score = 400 });
        data.Entries.Add(new ScoreEntry { Name = "Rocket", Country = "🇬🇧 UK", Time = 59.90f, Track = 1, Score = 300 });

        data.Entries.Add(new ScoreEntry { Name = "Champion", Country = "🇫🇷 France", Time = 56.40f, Track = 2, Score = 700 });
        data.Entries.Add(new ScoreEntry { Name = "Lightning", Country = "🇩🇪 Allemagne", Time = 59.15f, Track = 2, Score = 500 });
        data.Entries.Add(new ScoreEntry { Name = "Comet", Country = "🇨🇦 Canada", Time = 62.30f, Track = 2, Score = 400 });

        data.Entries.Add(new ScoreEntry { Name = "DuneRider", Country = "🇪🇸 Espagne", Time = 58.70f, Track = 3, Score = 650 });
        data.Entries.Add(new ScoreEntry { Name = "Mirage", Country = "🌍 Monde", Time = 61.20f, Track = 3, Score = 450 });
        data.Entries.Add(new ScoreEntry { Name = "Nomad", Country = "🇧🇷 Brésil", Time = 63.80f, Track = 3, Score = 350 });

        data.Entries.Add(new ScoreEntry { Name = "Blizzard", Country = "🇨🇭 Suisse", Time = 64.80f, Track = 4, Score = 700 });
        data.Entries.Add(new ScoreEntry { Name = "Yeti", Country = "🇨🇦 Canada", Time = 68.10f, Track = 4, Score = 550 });
        data.Entries.Add(new ScoreEntry { Name = "Avalanche", Country = "🇫🇷 France", Time = 71.40f, Track = 4, Score = 400 });

        data.Entries.Add(new ScoreEntry { Name = "NeonKnight", Country = "🇯🇵 Japon", Time = 55.30f, Track = 5, Score = 800 });
        data.Entries.Add(new ScoreEntry { Name = "CyberRacer", Country = "🇺🇸 USA", Time = 57.90f, Track = 5, Score = 600 });
        data.Entries.Add(new ScoreEntry { Name = "Vapor", Country = "🇬🇧 UK", Time = 60.50f, Track = 5, Score = 500 });
        Save();
    }

    public static void Save() {
        if (data == null) return;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PrefsKey, json);
        PlayerPrefs.Save();
    }

    public static List<ScoreEntry> GetTop(int trackFilter = -1) {
        Load();
        var list = new List<ScoreEntry>();
        foreach (var e in data.Entries) {
            if (trackFilter < 0 || e.Track == trackFilter) {
                list.Add(e);
            }
        }
        list.Sort((a, b) => a.Time.CompareTo(b.Time));
        if (list.Count > 10) list.RemoveRange(10, list.Count - 10);
        return list;
    }

    public static void AddEntry(string name, string country, float time, int track, int score) {
        Load();
        if (string.IsNullOrEmpty(name)) name = "Pilote";
        name = name.Trim();
        if (name.Length > 12) name = name.Substring(0, 12);

        data.Entries.Add(new ScoreEntry {
            Name = name,
            Country = country,
            Time = time,
            Track = track,
            Score = score
        });

        data.Entries.Sort((a, b) => a.Time.CompareTo(b.Time));
        // Conserver les meilleurs
        if (data.Entries.Count > 30) data.Entries.RemoveRange(30, data.Entries.Count - 30);
        Save();
    }
}
}
