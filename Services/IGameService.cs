using LexiconWASMWordleApp.Enums;
using LexiconWASMWordleApp.Models;

namespace LexiconWASMWordleApp.Services
{
    public interface IGameService
    {
        List<List<TileState>> Board { get; }
        Dictionary<char, TileStatus> KeyboardStatus { get; }
        GameState State { get; }
        bool HardModeEnabled { get; set; }

        event Action? OnStateChanged;

        bool AddLetter(char letter);
        bool RemoveLetter();
        void StartNewGame(GameMode mode = GameMode.Daily);
        (bool success, string message) SubmitGuess();
    }
}