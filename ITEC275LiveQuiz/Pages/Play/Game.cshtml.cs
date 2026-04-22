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
    public int selectedAnswerId { get; set; }

    [BindProperty]
    public int elapsedMs { get; set; }

    [BindProperty]
    public int gameId { get; set; }

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
        // Add detailed logging
        Console.WriteLine($"=== OnPostAsync Called ===");
        Console.WriteLine($"GameId from route: {gameId}");
        Console.WriteLine($"selectedAnswerId: {selectedAnswerId}");
        Console.WriteLine($"elapsedMs: {elapsedMs}");
        
        var participantId = GetParticipantId(gameId);
        Console.WriteLine($"ParticipantId from session/cookie: {participantId}");
        
        if (!participantId.HasValue)
        {
            Console.WriteLine("ERROR: ParticipantId is null - redirecting to Join");
            return RedirectToPage("Join");
        }

        try
        {
            if (selectedAnswerId <= 0)
            {
                Console.WriteLine($"ERROR: Invalid selectedAnswerId: {selectedAnswerId}");
                return RedirectToPage("Game", new { gameId = gameId });
            }

            var liveQuestion = await dbContext.LiveQuestions
                .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);

            if (liveQuestion is null)
            {
                Console.WriteLine($"ERROR: No open live question found for gameId: {gameId}");
                return RedirectToPage("Game", new { gameId = gameId });
            }
            
            Console.WriteLine($"Found liveQuestion: {liveQuestion.LiveQuestionId}");

            var alreadyAnswered = await dbContext.LiveResponses
                .AnyAsync(r => r.LiveQuestionId == liveQuestion.LiveQuestionId
                           && r.LiveParticipantId == participantId.Value);

            if (alreadyAnswered)
            {
                Console.WriteLine("Already answered this question");
                return RedirectToPage("Game", new { gameId = gameId });
            }

            var answer = await dbContext.Answers
                .FirstOrDefaultAsync(a => a.AnswerId == selectedAnswerId
                                       && a.QuestionId == liveQuestion.QuestionId);

            if (answer is null)
            {
                Console.WriteLine($"ERROR: Answer not found for answerId: {selectedAnswerId}, questionId: {liveQuestion.QuestionId}");
                return RedirectToPage("Game", new { gameId = gameId });
            }
            
            Console.WriteLine($"Found answer: {answer.AnswerId}, IsCorrect: {answer.IsCorrect}");

            // Calculate points based on speed (1000-2000 points for correct answers)
            int pointsEarned = 0;
            if (answer.IsCorrect)
            {
                var timeLimitMs = (liveQuestion.Question?.TimeLimitSeconds ?? 30) * 1000;
                var timeMs = Math.Max(0, elapsedMs);
                var speedBonus = Math.Max(0, timeLimitMs - timeMs) / timeLimitMs;
                pointsEarned = (int)(1000 + (speedBonus * 1000));
            }

            var response = new LiveResponse
            {
                LiveQuestionId = liveQuestion.LiveQuestionId,
                LiveParticipantId = participantId.Value,
                AnswerId = answer.AnswerId,
                AnsweredAt = DateTime.UtcNow,
                IsCorrect = answer.IsCorrect,
                TimeMs = Math.Max(0, elapsedMs),
                PointsEarned = pointsEarned
            };

            dbContext.LiveResponses.Add(response);
            await dbContext.SaveChangesAsync();
            
            Console.WriteLine($"SUCCESS: Saved LiveResponse - QuestionId: {liveQuestion.LiveQuestionId}, ParticipantId: {participantId.Value}, AnswerId: {answer.AnswerId}, Points: {pointsEarned}");
            
            // Small delay to ensure database commit is complete
            await Task.Delay(100);

            return RedirectToPage("Game", new { gameId = gameId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION in OnPostAsync: {ex.Message}\n{ex.StackTrace}");
            return RedirectToPage("Game", new { gameId = gameId });
        }
    }
}
