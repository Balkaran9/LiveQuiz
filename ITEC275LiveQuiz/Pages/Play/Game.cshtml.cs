using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Play;

public class GameModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveGame? Game { get; set; }
    public LiveParticipant? Participant { get; set; }
    public LiveQuestion? CurrentQuestion { get; set; }
    public Question? Question { get; set; }
    public List<Answer> Answers { get; set; } = [];
    public bool AlreadyAnswered { get; set; }
    public bool GameEnded { get; set; }
    public LiveResponse? LastResponse { get; set; }

    [BindProperty]
    public int SelectedAnswerId { get; set; }

    [BindProperty]
    public int ElapsedMs { get; set; }

    [BindProperty]
    public int GameId { get; set; }

    public async Task<IActionResult> OnGetAsync(int gameId)
    {
        Game = await dbContext.LiveGames
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId);

        if (Game is null) return NotFound();

        var participantId = GetParticipantId(gameId);
        if (!participantId.HasValue)
        {
            return RedirectToPage("Join", new { code = Game.JoinCode });
        }

        Participant = await dbContext.LiveParticipants
            .FirstOrDefaultAsync(p => p.LiveParticipantId == participantId.Value && p.LiveGameId == gameId);

        if (Participant is null)
        {
            return RedirectToPage("Join");
        }

        GameEnded = Game.Status == "Ended";
        GameId = gameId;

        if (!GameEnded)
        {
            CurrentQuestion = await dbContext.LiveQuestions
                .Include(lq => lq.Question)
                .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);

            if (CurrentQuestion is not null)
            {
                Question = CurrentQuestion.Question;
                
                var answersQuery = dbContext.Answers.Where(a => a.QuestionId == CurrentQuestion.QuestionId);
                
                if (Question!.ShuffleAnswers)
                {
                    var allAnswers = await answersQuery.ToListAsync();
                    Answers = allAnswers.OrderBy(_ => Random.Shared.Next()).ToList();
                }
                else
                {
                    Answers = await answersQuery.OrderBy(a => a.AnswerId).ToListAsync();
                }

                AlreadyAnswered = await dbContext.LiveResponses
                    .AnyAsync(r => r.LiveQuestionId == CurrentQuestion.LiveQuestionId
                               && r.LiveParticipantId == Participant.LiveParticipantId);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var participantId = GetParticipantId(GameId);
        if (!participantId.HasValue) return RedirectToPage("Join");

        var liveQuestion = await dbContext.LiveQuestions
            .Include(lq => lq.Question)
            .FirstOrDefaultAsync(lq => lq.LiveGameId == GameId && lq.ClosedAt == null);

        if (liveQuestion is null) return RedirectToPage("Game", new { gameId = GameId });

        var alreadyAnswered = await dbContext.LiveResponses
            .AnyAsync(r => r.LiveQuestionId == liveQuestion.LiveQuestionId
                       && r.LiveParticipantId == participantId.Value);

        if (!alreadyAnswered)
        {
            var answer = await dbContext.Answers
                .FirstOrDefaultAsync(a => a.AnswerId == SelectedAnswerId
                                       && a.QuestionId == liveQuestion.QuestionId);

            if (answer is not null)
            {
                int points = 0;
                if (answer.IsCorrect)
                {
                    // Base points: 1000
                    // Speed bonus: up to 500 points (faster = more points)
                    var timeLimit = liveQuestion.Question!.TimeLimitSeconds * 1000;
                    var speedBonus = (int)Math.Max(0, 500 * (1 - (ElapsedMs / (double)timeLimit)));
                    points = 1000 + speedBonus;
                }

                var response = new LiveResponse
                {
                    LiveQuestionId = liveQuestion.LiveQuestionId,
                    LiveParticipantId = participantId.Value,
                    AnswerId = answer.AnswerId,
                    AnsweredAt = DateTime.UtcNow,
                    IsCorrect = answer.IsCorrect,
                    TimeMs = Math.Max(0, ElapsedMs),
                    PointsEarned = points
                };

                dbContext.LiveResponses.Add(response);
                await dbContext.SaveChangesAsync();
            }
        }

        return RedirectToPage("Game", new { gameId = GameId });
    }
}
