namespace LexiconWASMWordleApp.Models
{
    public class GameStats
    {
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }
        public Dictionary<int, int> GuessDistribution { get; set; } = new()
    {
        { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }
    };

        public double WinPercentage => GamesPlayed == 0 ? 0 : Math.Round((double)GamesWon / GamesPlayed * 100, 1);

    }
}
