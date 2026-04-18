using Microsoft.AspNetCore.SignalR;

namespace ITEC275LiveQuiz.Hubs;

public class GameHub : Hub
{
    public async Task JoinGame(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{gameId}");
    }

    public async Task LeaveGame(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game_{gameId}");
    }

    public async Task NotifyParticipantJoined(string gameId, string nickname)
    {
        await Clients.Group($"game_{gameId}").SendAsync("ParticipantJoined", nickname);
    }

    public async Task NotifyQuestionStarted(string gameId, int questionNumber, int totalQuestions)
    {
        await Clients.Group($"game_{gameId}").SendAsync("QuestionStarted", questionNumber, totalQuestions);
    }

    public async Task NotifyQuestionClosed(string gameId)
    {
        await Clients.Group($"game_{gameId}").SendAsync("QuestionClosed");
    }

    public async Task UpdateResponseCount(string gameId, int responseCount, int totalParticipants)
    {
        await Clients.Group($"game_{gameId}").SendAsync("ResponseCountUpdated", responseCount, totalParticipants);
    }

    public async Task UpdateLeaderboard(string gameId, object leaderboardData)
    {
        await Clients.Group($"game_{gameId}").SendAsync("LeaderboardUpdated", leaderboardData);
    }

    public async Task NotifyGameEnded(string gameId)
    {
        await Clients.Group($"game_{gameId}").SendAsync("GameEnded");
    }
}
