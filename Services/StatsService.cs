using LexiconWASMWordleApp.Enums;
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
        private const string DailyBoardKey = "lexicon_daily_board_state";

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

        public async Task RecordGameAsync(bool won, int guessCount, GameMode mode = GameMode.Daily, bool isHardMode = false)
        {
            var stats = await LoadStatsAsync();
            var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

            stats.GamesPlayed++;

            if (mode == GameMode.Daily)
            {
                await MarkDailyPlayedAsync();

                // Calculate streak continuity
                if (won)
                {
                    stats.GamesWon++;

                    if (isHardMode) stats.HardModeWins++;

                    // Coin reward: more coins for faster solves
                    int coinsEarned = (7 - guessCount) * 15 + (isHardMode ? 25 : 0);
                    stats.LexCoins += Math.Max(coinsEarned, 10);

                    // Check if yesterday was played
                    if (DateTime.TryParse(stats.LastDailyWonDate, out var lastWonDate))
                    {
                        var diff = (DateTime.UtcNow.Date - lastWonDate.Date).TotalDays;
                        if (diff == 1)
                        {
                            stats.CurrentStreak++;
                        }
                        else if (diff > 1)
                        {
                            stats.CurrentStreak = 1; // Skipped a day
                        }
                    }
                    else
                    {
                        stats.CurrentStreak = 1; // First daily win
                    }

                    stats.LastDailyWonDate = today;
                    stats.MaxStreak = Math.Max(stats.CurrentStreak, stats.MaxStreak);

                    if (stats.GuessDistribution.ContainsKey(guessCount))
                        stats.GuessDistribution[guessCount]++;
                }
                else
                {
                    stats.CurrentStreak = 0;
                }

                stats.LastDailyPlayedDate = today;
            }
            else // Freeplay Mode
            {
                if (won)
                {
                    stats.GamesWon++;
                    stats.LexCoins += 5; // Small casual reward
                    if (stats.GuessDistribution.ContainsKey(guessCount))
                        stats.GuessDistribution[guessCount]++;
                }
            }

            await SaveStatsAsync(stats);
        }

        public async Task<bool> HasPlayedTodayAsync()
        {
            try
            {
                var val = await _js.InvokeAsync<string?>("localStorage.getItem", DailyKey);
                return val == DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            }
            catch { }
            return false;
        }

        public async Task MarkDailyPlayedAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", DailyKey, DateTime.UtcNow.Date.ToString("yyyy-MM-dd"));
            }
            catch { }
        }

        public async Task ResetStatsAsync()
        {
            await SaveStatsAsync(new GameStats());
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", DailyKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", DailyBoardKey);
            }
            catch { }
        }

        //public Task RecordGameAsync(bool won, int guessCount)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
