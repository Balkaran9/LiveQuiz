using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITEC275LiveQuiz.Pages;

public abstract class AppPageModel : PageModel
{
    protected int? GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId");
    }

    protected IActionResult RedirectToLogin()
    {
        return RedirectToPage("/Account/Login");
    }

    protected int? GetParticipantId(int gameId)
    {
        return HttpContext.Session.GetInt32($"Participant_{gameId}");
    }

    protected void SetParticipantId(int gameId, int participantId)
    {
        HttpContext.Session.SetInt32($"Participant_{gameId}", participantId);
    }
}
