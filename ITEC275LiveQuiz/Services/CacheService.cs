using Microsoft.Extensions.Caching.Memory;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Services;

public class CacheService(IMemoryCache cache, AppDbContext dbContext, ILogger<CacheService> logger)
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);
    private const string LeaderboardCacheKey = "leaderboard_game_{0}";
    private const string QuizCacheKey = "quiz_{0}";
    private const string PublicQuizzesCacheKey = "public_quizzes";

    public async Task<List<LeaderboardEntry>> GetOrSetLeaderboardAsync(
        int gameId, 
        Func<Task<List<LeaderboardEntry>>> factory)
    {
        var cacheKey = string.Format(LeaderboardCacheKey, gameId);
        
        if (cache.TryGetValue(cacheKey, out List<LeaderboardEntry>? cached) && cached is not null)
        {
            logger.LogInformation("Cache HIT for leaderboard game {GameId}", gameId);
            return cached;
        }

        logger.LogInformation("Cache MISS for leaderboard game {GameId}", gameId);
        var result = await factory();
        
        cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            Priority = CacheItemPriority.High
        });

        return result;
    }

    public async Task<Quiz?> GetOrSetQuizAsync(int quizId, Func<Task<Quiz?>> factory)
    {
        var cacheKey = string.Format(QuizCacheKey, quizId);
        
        if (cache.TryGetValue(cacheKey, out Quiz? cached) && cached is not null)
        {
            logger.LogInformation("Cache HIT for quiz {QuizId}", quizId);
            return cached;
        }

        logger.LogInformation("Cache MISS for quiz {QuizId}", quizId);
        var result = await factory();
        
        if (result is not null)
        {
            cache.Set(cacheKey, result, DefaultExpiration);
        }

        return result;
    }

    public async Task<List<Quiz>> GetOrSetPublicQuizzesAsync(
        string? searchTerm,
        Func<Task<List<Quiz>>> factory)
    {
        var cacheKey = $"{PublicQuizzesCacheKey}_{searchTerm ?? "all"}";
        
        if (cache.TryGetValue(cacheKey, out List<Quiz>? cached) && cached is not null)
        {
            logger.LogInformation("Cache HIT for public quizzes (search: {SearchTerm})", searchTerm ?? "none");
            return cached;
        }

        logger.LogInformation("Cache MISS for public quizzes");
        var result = await factory();
        
        cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        });

        return result;
    }

    public void InvalidateLeaderboard(int gameId)
    {
        var cacheKey = string.Format(LeaderboardCacheKey, gameId);
        cache.Remove(cacheKey);
        logger.LogInformation("Invalidated leaderboard cache for game {GameId}", gameId);
    }

    public void InvalidateQuiz(int quizId)
    {
        var cacheKey = string.Format(QuizCacheKey, quizId);
        cache.Remove(cacheKey);
        logger.LogInformation("Invalidated quiz cache for {QuizId}", quizId);
    }

    public void InvalidatePublicQuizzes()
    {
        cache.Remove(PublicQuizzesCacheKey);
        logger.LogInformation("Invalidated public quizzes cache");
    }

    public async Task<Dictionary<string, object>> GetCacheStatisticsAsync()
    {
        return new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow },
            { "cacheType", "InMemory" },
            { "status", "Active" }
        };
    }
}
