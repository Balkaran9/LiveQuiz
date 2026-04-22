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
            .AsNoTracking()
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId);

        if (Game is null) return NotFound();

        var participantId = GetParticipantId(gameId);
        if (!participantId.HasValue)
        {
            return RedirectToPage("Join", new { code = Game.JoinCode });
        }

        Participant = await dbContext.LiveParticipants
            .AsNoTracking()
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
                .AsNoTracking()
                .Include(lq => lq.Question)
                .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);

            if (CurrentQuestion is not null)
            {
                Question = CurrentQuestion.Question;
                Answers = await dbContext.Answers
                    .AsNoTracking()
                    .Where(a => a.QuestionId == CurrentQuestion.QuestionId)
                    .OrderBy(a => a.AnswerId)
                    .ToListAsync();

                AlreadyAnswered = await dbContext.LiveResponses
                    .AsNoTracking()
                    .AnyAsync(r => r.LiveQuestionId == CurrentQuestion.LiveQuestionId
                               && r.LiveParticipantId == Participant.LiveParticipantId);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int gameId)
    {
        GameId = gameId;
        var participantId = GetParticipantId(gameId);
        if (!participantId.HasValue) return RedirectToPage("Join");

        try
        {
            var liveQuestion = await dbContext.LiveQuestions
                .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);

            if (liveQuestion is null) return RedirectToPage("Game", new { gameId = gameId });

            var alreadyAnswered = await dbContext.LiveResponses
                .AnyAsync(r => r.LiveQuestionId == liveQuestion.LiveQuestionId
                           && r.LiveParticipantId == participantId.Value);

            if (!alreadyAnswered && SelectedAnswerId > 0)
            {
                var answer = await dbContext.Answers
                    .FirstOrDefaultAsync(a => a.AnswerId == SelectedAnswerId
                                           && a.QuestionId == liveQuestion.QuestionId);

                if (answer is not null)
                {
                    var response = new LiveResponse
                    {
                        LiveQuestionId = liveQuestion.LiveQuestionId,
                        LiveParticipantId = participantId.Value,
                        AnswerId = answer.AnswerId,
                        AnsweredAt = DateTime.UtcNow,
                        IsCorrect = answer.IsCorrect,
                        TimeMs = Math.Max(0, ElapsedMs)
                    };

                    dbContext.LiveResponses.Add(response);
                    await dbContext.SaveChangesAsync();
                    
                    // Small delay to ensure database commit is complete
                    await Task.Delay(100);
                }
            }

            return RedirectToPage("Game", new { gameId = gameId });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
            return RedirectToPage("Game", new { gameId = gameId });
        }
    }
}
