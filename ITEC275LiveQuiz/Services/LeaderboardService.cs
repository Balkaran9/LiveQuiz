using ITEC275LiveQuiz.Data;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Services;

public class LeaderboardService(AppDbContext dbContext)
{
    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int gameId)
    {
        var participants = await dbContext.LiveParticipants
            .AsNoTracking()
            .Where(p => p.LiveGameId == gameId)
            .Include(p => p.Responses)
            .ThenInclude(r => r.LiveQuestion)
            .ThenInclude(lq => lq!.Question)
            .ToListAsync();

        var entries = participants.Select(p =>
        {
            var correctResponses = p.Responses.Where(r => r.IsCorrect).ToList();
            var score = 0;

            foreach (var response in correctResponses)
            {
                var timeLimit = response.LiveQuestion?.Question?.TimeLimitSeconds ?? 30;
                var timeLimitMs = timeLimit * 1000;
                var timeBonus = Math.Max(0, (timeLimitMs - response.TimeMs) / (double)timeLimitMs);
                var points = (int)(1000 * (1 + timeBonus));
                score += points;
            }

            return new LeaderboardEntry
            {
                LiveParticipantId = p.LiveParticipantId,
                Nickname = p.Nickname,
                TotalCorrect = correctResponses.Count,
                TotalIncorrect = p.Responses.Count - correctResponses.Count,
                AvgTimeMs = p.Responses.Any() ? p.Responses.Average(r => (double)r.TimeMs) : 0,
                Score = score,
                TotalTimeMs = p.Responses.Any() ? p.Responses.Sum(r => r.TimeMs) : 0
            };
        })
        .OrderByDescending(x => x.Score)
        .ThenBy(x => x.TotalTimeMs)
        .ThenBy(x => x.Nickname)
        .ToList();

        return entries;
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
