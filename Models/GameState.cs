using LexiconWASMWordleApp.Enums;

namespace LexiconWASMWordleApp.Models
{
    public class GameState
    {
        public string TargetWord { get; set; } = "";
        public List<string> Guesses { get; set; } = new();
        public string CurrentGuess { get; set; } = "";
        public bool IsGameOver { get; set; }
        public bool IsWon { get; set; }
        public int MaxGuesses { get; set; } = 6;
        public string Message { get; set; } = "";
        public GameMode Mode { get; set; } = GameMode.Daily;
    }
}
