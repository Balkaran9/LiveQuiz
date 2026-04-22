using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Account;

public class RegisterModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Password != Input.ConfirmPassword)
        {
            ModelState.AddModelError("Input.ConfirmPassword", "Passwords do not match.");
            return Page();
        }

        var username = Input.Username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(username))
        {
            ModelState.AddModelError("Input.Username", "Username is required.");
            return Page();
        }

        var exists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username);
        
        if (exists)
        {
            ModelState.AddModelError(string.Empty, "That username is already taken.");
            return Page();
        }

        var fullName = Input.FullName?.Trim();
        var email = Input.Email?.Trim();

        var user = new User
        {
            Username = username,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var errorMessage = "An error occurred while creating your account. ";
            if (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
            {
                errorMessage += "That username is already taken.";
            }
            else
            {
                errorMessage += "Please try again or contact support.";
            }
            ModelState.AddModelError(string.Empty, errorMessage);
            return Page();
        }

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Username", user.Username);
        if (!string.IsNullOrEmpty(user.FullName))
        {
            HttpContext.Session.SetString("FullName", user.FullName);
        }
        
        return RedirectToPage("/Host/Dashboard");
    }

    public class InputModel
    {
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}








