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

        var username = Input.Username.Trim();
        var exists = await dbContext.Users.AnyAsync(u => u.Username == username);
        if (exists)
        {
            ModelState.AddModelError(string.Empty, "That username is already taken.");
            return Page();
        }

        var user = new User
        {
            Username = username,
            Email = Input.Email?.Trim(),
            FullName = Input.FullName?.Trim() ?? string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Username", user.Username);
        return RedirectToPage("/Host/Dashboard");
    }

    public class InputModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

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








