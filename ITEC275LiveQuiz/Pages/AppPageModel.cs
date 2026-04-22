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
        // Try session first
        var sessionId = HttpContext.Session.GetInt32($"Participant_{gameId}");
        if (sessionId.HasValue)
            return sessionId;

        // Fallback to cookie if session is lost (Railway container restart)
        var cookieName = $"Participant_{gameId}";
        if (HttpContext.Request.Cookies.TryGetValue(cookieName, out var cookieValue) && 
            int.TryParse(cookieValue, out var participantId))
        {
            // Restore to session
            HttpContext.Session.SetInt32($"Participant_{gameId}", participantId);
            return participantId;
        }

        return null;
    }

    protected void SetParticipantId(int gameId, int participantId)
    {
        // Store in session
        HttpContext.Session.SetInt32($"Participant_{gameId}", participantId);

        // Also store in persistent cookie (expires in 24 hours)
        HttpContext.Response.Cookies.Append(
            $"Participant_{gameId}",
            participantId.ToString(),
            new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddHours(24),
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
            }
        );
    }
}
