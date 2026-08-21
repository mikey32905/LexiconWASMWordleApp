namespace LexiconWASMWordleApp.Models
{
    public class GameStats
    {
        // General Totals
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }

        // Streak Date Tracking (UTC-based date string e.g. "2026-08-20")
        public string? LastDailyPlayedDate { get; set; }
        public string? LastDailyWonDate { get; set; }

        // Currency / Gamification
        public int LexCoins { get; set; } = 0;
        public int HardModeWins { get; set; } = 0;

        // Guess Distribution (1-6 guesses)
        public Dictionary<int, int> GuessDistribution { get; set; } = new()
        {
            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }
        };

        // Computed Properties
        public double WinPercentage => GamesPlayed == 0
            ? 0
            : Math.Round((double)GamesWon / GamesPlayed * 100, 1);

        public double AverageGuesses => GamesWon == 0
            ? 0
            : Math.Round(GuessDistribution.Sum(pair => pair.Key * pair.Value) / (double)GamesWon, 2);
    }
}
