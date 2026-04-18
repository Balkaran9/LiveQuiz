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

        Game = await dbContext.LiveGames
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

        if (Game is null)
        {
            return NotFound();
        }

        LiveQuestion = await dbContext.LiveQuestions
            .Include(lq => lq.Question)
            .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);

        if (LiveQuestion is null)
        {
            var openedQuestionIds = await dbContext.LiveQuestions
                .Where(lq => lq.LiveGameId == gameId)
                .Select(lq => lq.QuestionId)
                .ToListAsync();

            var questionQuery = dbContext.Questions
                .Where(q => q.QuizId == Game.QuizId && !openedQuestionIds.Contains(q.QuestionId));

            if (Game.Quiz!.ShuffleQuestions)
            {
                var allAvailable = await questionQuery.ToListAsync();
                Question = allAvailable.OrderBy(_ => Random.Shared.Next()).FirstOrDefault();
            }
            else
            {
                Question = await questionQuery.OrderBy(q => q.SortOrder).FirstOrDefaultAsync();
            }

            if (Question is null)
            {
                NoMoreQuestions = true;
                return Page();
            }

            LiveQuestion = new LiveQuestion
            {
                LiveGameId = gameId,
                QuestionId = Question.QuestionId,
                OpenedAt = DateTime.UtcNow
            };

            Game.Status = "InProgress";
            dbContext.LiveQuestions.Add(LiveQuestion);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            Question = LiveQuestion.Question;
        }

        Question ??= await dbContext.Questions.FirstOrDefaultAsync(q => q.QuestionId == LiveQuestion.QuestionId);
        Answers = await dbContext.Answers
            .Where(a => a.QuestionId == LiveQuestion.QuestionId)
            .OrderBy(a => a.AnswerId)
            .ToListAsync();

        ResponseCount = await dbContext.LiveResponses
            .CountAsync(r => r.LiveQuestionId == LiveQuestion.LiveQuestionId);
        ParticipantCount = await dbContext.LiveParticipants
            .CountAsync(p => p.LiveGameId == gameId);

        return Page();
    }
}
