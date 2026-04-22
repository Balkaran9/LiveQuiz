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
        var fromSession = HttpContext.Session.GetInt32($"Participant_{gameId}");
        if (fromSession.HasValue) return fromSession;

        // Fallback to cookie (survives container restarts on Railway)
        var cookieKey = $"Participant_{gameId}";
        if (HttpContext.Request.Cookies.TryGetValue(cookieKey, out var cookieVal)
            && int.TryParse(cookieVal, out var fromCookie))
        {
            // Restore into session for next time
            HttpContext.Session.SetInt32(cookieKey, fromCookie);
            return fromCookie;
        }

        return null;
    }

    protected void SetParticipantId(int gameId, int participantId)
    {
        // Store in both session AND cookie
        HttpContext.Session.SetInt32($"Participant_{gameId}", participantId);

        HttpContext.Response.Cookies.Append($"Participant_{gameId}", participantId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = false,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });
    }
}
