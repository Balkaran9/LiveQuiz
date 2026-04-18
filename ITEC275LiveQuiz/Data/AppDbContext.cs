using ITEC275LiveQuiz.Models;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<LiveGame> LiveGames => Set<LiveGame>();
    public DbSet<LiveParticipant> LiveParticipants => Set<LiveParticipant>();
    public DbSet<LiveQuestion> LiveQuestions => Set<LiveQuestion>();
    public DbSet<LiveResponse> LiveResponses => Set<LiveResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<LiveGame>()
            .HasIndex(g => g.JoinCode)
            .IsUnique();

        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.OwnerUser)
            .WithMany(u => u.OwnedQuizzes)
            .HasForeignKey(q => q.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LiveGame>()
            .HasOne(g => g.HostUser)
            .WithMany(u => u.HostedGames)
            .HasForeignKey(g => g.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LiveGame>()
            .HasOne(g => g.Quiz)
            .WithMany(q => q.LiveGames)
            .HasForeignKey(g => g.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Question>()
            .HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveParticipant>()
            .HasOne(p => p.LiveGame)
            .WithMany(g => g.Participants)
            .HasForeignKey(p => p.LiveGameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveQuestion>()
            .HasOne(lq => lq.LiveGame)
            .WithMany(g => g.LiveQuestions)
            .HasForeignKey(lq => lq.LiveGameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveQuestion>()
            .HasOne(lq => lq.Question)
            .WithMany(q => q.LiveQuestions)
            .HasForeignKey(lq => lq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LiveResponse>()
            .HasOne(r => r.LiveQuestion)
            .WithMany(q => q.Responses)
            .HasForeignKey(r => r.LiveQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LiveResponse>()
            .HasOne(r => r.LiveParticipant)
            .WithMany(p => p.Responses)
            .HasForeignKey(r => r.LiveParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LiveResponse>()
            .HasOne(r => r.Answer)
            .WithMany(a => a.LiveResponses)
            .HasForeignKey(r => r.AnswerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
