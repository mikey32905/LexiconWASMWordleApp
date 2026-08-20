using LexiconWASMWordleApp.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace LexiconWASMWordleApp.Services
{
    public class StatsService : IStatsService
    {
        private readonly IJSRuntime _js;
        private const string StatsKey = "lexicon_stats";
        private const string DailyKey = "lexicon_daily_played";

        public StatsService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<GameStats> LoadStatsAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string?>("localStorage.getItem", StatsKey);
                if (!string.IsNullOrEmpty(json))
                {
                    var stats = JsonSerializer.Deserialize<GameStats>(json);
                    return stats ?? new GameStats();
                }
            }
            catch { }
            return new GameStats();
        }

        public async Task SaveStatsAsync(GameStats stats)
        {
            try
            {
                var json = JsonSerializer.Serialize(stats);
                await _js.InvokeVoidAsync("localStorage.setItem", StatsKey, json);
            }
            catch { }
        }

        public async Task RecordGameAsync(bool won, int guessCount)
        {
            var stats = await LoadStatsAsync();
            stats.GamesPlayed++;
            if (won)
            {
                stats.GamesWon++;
                stats.CurrentStreak++;
                if (stats.CurrentStreak > stats.MaxStreak)
                    stats.MaxStreak = stats.CurrentStreak;

                if (stats.GuessDistribution.ContainsKey(guessCount))
                    stats.GuessDistribution[guessCount]++;
            }
            else
            {
                stats.CurrentStreak = 0;
            }
            await SaveStatsAsync(stats);
        }

        public async Task<bool> HasPlayedTodayAsync()
        {
            try
            {
                var val = await _js.InvokeAsync<string?>("localStorage.getItem", DailyKey);
                return val == DateTime.Today.ToString("yyyy-MM-dd");
            }
            catch { }
            return false;
        }

        public async Task MarkDailyPlayedAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", DailyKey, DateTime.Today.ToString("yyyy-MM-dd"));
            }
            catch { }
        }

        public async Task ResetStatsAsync()
        {
            await SaveStatsAsync(new GameStats());
        }
    }
}
