using LexiconWASMWordleApp.Enums;
using LexiconWASMWordleApp.Models;

namespace LexiconWASMWordleApp.Services
{
    public class GameService : IGameService
    {
        public GameState State { get; private set; } = new();
        public List<List<TileState>> Board { get; private set; } = new();
        public Dictionary<char, TileStatus> KeyboardStatus { get; private set; } = new();
        public bool HardModeEnabled { get; set; } = false;

        public event Action? OnStateChanged;

        public GameService()
        {
            ResetBoard();
        }

        public void StartNewGame(GameMode mode = GameMode.Daily)
        {
            State = new GameState
            {
                TargetWord = mode == GameMode.Daily ? WordList.GetDailyWord() : WordList.GetRandomWord(),
                Mode = mode
            };

            ResetBoard();
            NotifyStateChanged();
        }

        public bool AddLetter(char letter)
        {
            if (State.IsGameOver || State.CurrentGuess.Length >= 5) return false;

            State.CurrentGuess += char.ToUpper(letter);
            UpdateCurrentRowTiles();
            NotifyStateChanged();
            return true;
        }

        public bool RemoveLetter()
        {
            if (State.IsGameOver || State.CurrentGuess.Length == 0) return false;

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

            // Hard Mode checks
            if (HardModeEnabled && State.Guesses.Count > 0)
            {
                var (isValidHardMode, hardModeError) = ValidateHardMode(State.CurrentGuess);
                if (!isValidHardMode) return (false, hardModeError);
            }

            var guess = State.CurrentGuess;
            State.Guesses.Add(guess);
            State.CurrentGuess = "";

            var result = EvaluateGuess(guess, State.TargetWord);
            var rowIdx = State.Guesses.Count - 1;

            for (int i = 0; i < 5; i++)
            {
                Board[rowIdx][i].Letter = guess[i];
                Board[rowIdx][i].Status = result[i];
            }

            // Update Keyboard Status
            for (int i = 0; i < 5; i++)
            {
                var ch = guess[i];
                var currentStatus = KeyboardStatus[ch];

                if (result[i] == TileStatus.Correct)
                {
                    KeyboardStatus[ch] = TileStatus.Correct;
                }
                else if (result[i] == TileStatus.Present && currentStatus != TileStatus.Correct)
                {
                    KeyboardStatus[ch] = TileStatus.Present;
                }
                else if (result[i] == TileStatus.Absent && currentStatus == TileStatus.Empty)
                {
                    KeyboardStatus[ch] = TileStatus.Absent;
                }
            }

            bool won = string.Equals(guess, State.TargetWord, StringComparison.OrdinalIgnoreCase);

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

        private (bool isValid, string error) ValidateHardMode(string currentGuess)
        {
            var lastRowIdx = State.Guesses.Count - 1;
            var lastGuess = State.Guesses[lastRowIdx];

            // 1. Must use correct letters in the exact same positions
            for (int i = 0; i < 5; i++)
            {
                if (Board[lastRowIdx][i].Status == TileStatus.Correct && currentGuess[i] != lastGuess[i])
                {
                    return (false, $"Letter {lastGuess[i]} must be in position {i + 1}");
                }
            }

            // 2. Must use present letters somewhere in the new guess
            for (int i = 0; i < 5; i++)
            {
                if (Board[lastRowIdx][i].Status == TileStatus.Present && !currentGuess.Contains(lastGuess[i]))
                {
                    return (false, $"Guess must contain letter {lastGuess[i]}");
                }
            }

            return (true, "");
        }

        private TileStatus[] EvaluateGuess(string guess, string target)
        {
            var result = new TileStatus[5];

            // Normalize both to uppercase char arrays
            var targetChars = target.ToUpperInvariant().ToCharArray();
            var guessChars = guess.ToUpperInvariant().ToCharArray();
            var used = new bool[5];

            // Pass 1: Exact matches (Green / Correct)
            for (int i = 0; i < 5; i++)
            {
                if (guessChars[i] == targetChars[i])
                {
                    result[i] = TileStatus.Correct;
                    used[i] = true;
                }
            }

            // Pass 2: Misplaced letters (Yellow / Present)
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

        private void ResetBoard()
        {
            Board = Enumerable.Range(0, 6)
                .Select(_ => Enumerable.Range(0, 5)
                    .Select(_ => new TileState())
                    .ToList())
                .ToList();

            KeyboardStatus = new Dictionary<char, TileStatus>();
            for (char c = 'A'; c <= 'Z'; c++)
                KeyboardStatus[c] = TileStatus.Empty;
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
