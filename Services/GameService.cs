using LexiconWASMWordleApp.Enums;
using LexiconWASMWordleApp.Models;

namespace LexiconWASMWordleApp.Services
{
    public class GameService : IGameService
    {
        public GameState State { get; private set; } = new();
        public List<List<TileState>> Board { get; private set; } = new();
        public Dictionary<char, TileStatus> KeyboardStatus { get; private set; } = new();

        public event Action? OnStateChanged;

        public GameService()
        {
            // Initialize the board with empty tiles to prevent null reference errors
            Board = Enumerable.Range(0, 6)
                .Select(_ => Enumerable.Range(0, 5)
                    .Select(_ => new TileState())
                    .ToList())
                .ToList();

            // Initialize keyboard status
            KeyboardStatus = new Dictionary<char, TileStatus>();
            for (char c = 'A'; c <= 'Z'; c++)
                KeyboardStatus[c] = TileStatus.Empty;
        }

        public void StartNewGame(GameMode mode = GameMode.Daily)
        {
            State = new GameState
            {
                TargetWord = mode == GameMode.Daily ? WordList.GetDailyWord() : WordList.GetRandomWord(),
                Mode = mode
            };

            Board = Enumerable.Range(0, 6)
                .Select(_ => Enumerable.Range(0, 5)
                    .Select(_ => new TileState())
                    .ToList())
                .ToList();

            KeyboardStatus = new Dictionary<char, TileStatus>();
            for (char c = 'A'; c <= 'Z'; c++)
                KeyboardStatus[c] = TileStatus.Empty;

            NotifyStateChanged();
        }

        public bool AddLetter(char letter)
        {
            if (State.IsGameOver) return false;
            if (State.CurrentGuess.Length >= 5) return false;

            State.CurrentGuess += char.ToUpper(letter);
            UpdateCurrentRowTiles();
            NotifyStateChanged();
            return true;
        }

        public bool RemoveLetter()
        {
            if (State.IsGameOver) return false;
            if (State.CurrentGuess.Length == 0) return false;

            State.CurrentGuess = State.CurrentGuess[..^1];
            UpdateCurrentRowTiles();
            NotifyStateChanged();
            return true;
        }

        public (bool success, string message) SubmitGuess()
        {
            if (State.IsGameOver) return (false, "Game over");
            if (State.CurrentGuess.Length < 5) return (false, "Not enough letters");
            if (!WordList.IsValidGuess(State.CurrentGuess)) return (false, "Not in word list");

            var guess = State.CurrentGuess;
            State.Guesses.Add(guess);
            State.CurrentGuess = "";

            // Evaluate the guess
            var result = EvaluateGuess(guess, State.TargetWord);
            var rowIdx = State.Guesses.Count - 1;

            for (int i = 0; i < 5; i++)
            {
                Board[rowIdx][i].Letter = guess[i];
                Board[rowIdx][i].Status = result[i];
                Board[rowIdx][i].IsRevealing = true;
            }

            // Update keyboard
            for (int i = 0; i < 5; i++)
            {
                var ch = guess[i];
                var existing = KeyboardStatus[ch];
                if (result[i] == TileStatus.Correct)
                    KeyboardStatus[ch] = TileStatus.Correct;
                else if (result[i] == TileStatus.Present && existing != TileStatus.Correct)
                    KeyboardStatus[ch] = TileStatus.Present;
                else if (result[i] == TileStatus.Absent && existing == TileStatus.Empty)
                    KeyboardStatus[ch] = TileStatus.Absent;
            }

            bool won = guess == State.TargetWord;
            if (won)
            {
                State.IsGameOver = true;
                State.IsWon = true;
                State.Message = GetWinMessage(State.Guesses.Count);
            }
            else if (State.Guesses.Count >= State.MaxGuesses)
            {
                State.IsGameOver = true;
                State.IsWon = false;
                State.Message = State.TargetWord;
            }

            NotifyStateChanged();
            return (true, won ? State.Message : "");
        }

        private TileStatus[] EvaluateGuess(string guess, string target)
        {
            var result = new TileStatus[5];
            var targetChars = target.ToCharArray();
            var guessChars = guess.ToCharArray();
            var used = new bool[5];

            // First pass: correct
            for (int i = 0; i < 5; i++)
            {
                if (guessChars[i] == targetChars[i])
                {
                    result[i] = TileStatus.Correct;
                    used[i] = true;
                }
            }

            // Second pass: present
            for (int i = 0; i < 5; i++)
            {
                if (result[i] == TileStatus.Correct) continue;
                bool found = false;
                for (int j = 0; j < 5; j++)
                {
                    if (!used[j] && guessChars[i] == targetChars[j])
                    {
                        found = true;
                        used[j] = true;
                        break;
                    }
                }
                result[i] = found ? TileStatus.Present : TileStatus.Absent;
            }

            return result;
        }

        private void UpdateCurrentRowTiles()
        {
            var rowIdx = State.Guesses.Count;
            if (rowIdx >= 6) return;

            for (int i = 0; i < 5; i++)
            {
                if (i < State.CurrentGuess.Length)
                {
                    Board[rowIdx][i].Letter = State.CurrentGuess[i];
                    Board[rowIdx][i].Status = TileStatus.Filled;
                }
                else
                {
                    Board[rowIdx][i].Letter = ' ';
                    Board[rowIdx][i].Status = TileStatus.Empty;
                }
            }
        }

        private string GetWinMessage(int guesses) => guesses switch
        {
            1 => "LEGENDARY!",
            2 => "BRILLIANT!",
            3 => "IMPRESSIVE!",
            4 => "SOLID!",
            5 => "CLOSE CALL!",
            6 => "PHEW! MADE IT!",
            _ => "NICE!"
        };

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
    }
}
