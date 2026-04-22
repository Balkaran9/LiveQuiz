using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class NextQuestionModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveGame? Game { get; set; }
    public LiveQuestion? LiveQuestion { get; set; }
    public Question? Question { get; set; }
    public List<Answer> Answers { get; set; } = [];
    public bool NoMoreQuestions { get; set; }
    public int ResponseCount { get; set; }
    public int ParticipantCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int gameId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        try
        {
            Game = await dbContext.LiveGames
                .AsNoTracking()
                .Include(g => g.Quiz)
                .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

            if (Game is null)
            {
                return NotFound();
            }

            if (Game.Status == "Ended")
            {
                return RedirectToPage("Leaderboard", new { gameId = gameId });
            }

            LiveQuestion = await dbContext.LiveQuestions
                .AsNoTracking()
                .Include(lq => lq.Question)
                .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);

            if (LiveQuestion is null)
            {
                var openedQuestionIds = await dbContext.LiveQuestions
                    .AsNoTracking()
                    .Where(lq => lq.LiveGameId == gameId)
                    .Select(lq => lq.QuestionId)
                    .ToListAsync();

                Question = await dbContext.Questions
                    .AsNoTracking()
                    .Where(q => q.QuizId == Game.QuizId && !openedQuestionIds.Contains(q.QuestionId))
                    .OrderBy(q => q.SortOrder)
                    .FirstOrDefaultAsync();

                if (Question is null)
                {
                    NoMoreQuestions = true;
                    return Page();
                }

                var gameToUpdate = await dbContext.LiveGames
                    .FirstOrDefaultAsync(g => g.LiveGameId == gameId);

                if (gameToUpdate is not null)
                {
                    gameToUpdate.Status = "InProgress";
                }

                LiveQuestion = new LiveQuestion
                {
                    LiveGameId = gameId,
                    QuestionId = Question.QuestionId,
                    OpenedAt = DateTime.UtcNow
                };

                dbContext.LiveQuestions.Add(LiveQuestion);
                await dbContext.SaveChangesAsync();

                Game = await dbContext.LiveGames
                    .AsNoTracking()
                    .Include(g => g.Quiz)
                    .FirstOrDefaultAsync(g => g.LiveGameId == gameId);
            }
            else
            {
                Question = LiveQuestion.Question;
            }

            if (Question is null && LiveQuestion is not null)
            {
                Question = await dbContext.Questions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuestionId == LiveQuestion.QuestionId);
            }

            if (Question is null)
            {
                return RedirectToPage("Lobby", new { gameId = gameId });
            }

            Answers = await dbContext.Answers
                .AsNoTracking()
                .Where(a => a.QuestionId == Question.QuestionId)
                .OrderBy(a => a.AnswerId)
                .ToListAsync();

            ResponseCount = await dbContext.LiveResponses
                .AsNoTracking()
                .Where(r => r.LiveQuestionId == LiveQuestion!.LiveQuestionId)
                .CountAsync();

            ParticipantCount = await dbContext.LiveParticipants
                .AsNoTracking()
                .Where(p => p.LiveGameId == gameId)
                .CountAsync();

            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("Lobby", new { gameId = gameId });
        }
    }
}
