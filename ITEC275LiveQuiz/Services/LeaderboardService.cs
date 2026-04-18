using ITEC275LiveQuiz.Data;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Services;

public class LeaderboardService(AppDbContext dbContext)
{
    public Task<List<LeaderboardEntry>> GetLeaderboardAsync(int gameId)
    {
        return dbContext.LiveParticipants
            .Where(p => p.LiveGameId == gameId)
            .Select(p => new LeaderboardEntry
            {
                LiveParticipantId = p.LiveParticipantId,
                Nickname = p.Nickname,
                TotalCorrect = p.Responses.Count(r => r.IsCorrect),
                TotalIncorrect = p.Responses.Count(r => !r.IsCorrect),
                AvgTimeMs = p.Responses.Average(r => (double?)r.TimeMs) ?? 0,
                Score = p.Responses.Sum(r => (int?)r.PointsEarned) ?? 0,
                TotalTimeMs = p.Responses.Sum(r => (int?)r.TimeMs) ?? 0
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.TotalTimeMs)
            .ThenBy(x => x.Nickname)
            .ToListAsync();
    }
}

public class LeaderboardEntry
{
    public int LiveParticipantId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int TotalCorrect { get; set; }
    public int TotalIncorrect { get; set; }
    public double AvgTimeMs { get; set; }
    public int Score { get; set; }
    public int TotalTimeMs { get; set; }
}
