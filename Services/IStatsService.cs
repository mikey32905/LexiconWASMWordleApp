using LexiconWASMWordleApp.Enums;
using LexiconWASMWordleApp.Models;

namespace LexiconWASMWordleApp.Services
{
    public interface IStatsService
    {
        Task<bool> HasPlayedTodayAsync();
        Task<GameStats> LoadStatsAsync();
        Task MarkDailyPlayedAsync();
        Task RecordGameAsync(bool won, int guessCount, GameMode mode = GameMode.Daily, bool isHardMode = false);
        Task ResetStatsAsync();
        Task SaveStatsAsync(GameStats stats);
    }
}