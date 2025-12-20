using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuiviEntrainementSportif.Models;

namespace SuiviEntrainementSportif.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Entrainement> Entrainements { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Objectif> Objectifs { get; set; }
        public DbSet<ObjectifSportif> ObjectifsSportifs { get; set; }
    }
}   
