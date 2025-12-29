using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SuiviEntrainementSportif.Models;

namespace SuiviEntrainementSportif.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.HeightCm).HasColumnType("decimal(5,2)"); 
                b.Property(u => u.WeightKg).HasColumnType("decimal(5,2)"); 
            });
        }

        public DbSet<Entrainement> Entrainements { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Objectif> Objectifs { get; set; }
        public DbSet<ObjectifSportif> ObjectifsSportifs { get; set; }

       
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
        public DbSet<DailyWorkout> DailyWorkouts { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
    }
}   
