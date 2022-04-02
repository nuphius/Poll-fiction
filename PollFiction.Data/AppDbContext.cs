using Microsoft.EntityFrameworkCore;
using PollFiction.Data.Model;
using System;

namespace PollFiction.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<PollGuest> PollGuests { get; set; }
        public DbSet<GuestChoice> GuestChoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .HasMany<Poll>(g => g.Polls)
                        .WithOne(s => s.User)
                        .HasForeignKey(s => s.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Poll>()
                        .HasMany<Choice>(g => g.Choices)
                        .WithOne(s => s.Poll)
                        .HasForeignKey(s => s.PollId)
                        .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<PollGuest>().HasKey(sc => new { sc.PollId, sc.GuestId });

            modelBuilder.Entity<PollGuest>()
                        .HasOne<Poll>(sc => sc.Poll)
                        .WithMany(s => s.PollGuests)
                        .HasForeignKey(sc => sc.PollId)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<PollGuest>()
                        .HasOne<Guest>(sc => sc.Guest)
                        .WithMany(s => s.PollGuests)
                        .HasForeignKey(sc => sc.GuestId)
                        .OnDelete(DeleteBehavior.Cascade);

           // modelBuilder.Entity<GuestChoice>().HasKey(sc => new { sc.ChoiceId, sc.GuestId });

            modelBuilder.Entity<GuestChoice>()
                        .HasOne<Choice>(sc => sc.Choice)
                        .WithMany(s => s.GuestChoices)
                        .HasForeignKey(sc => sc.ChoiceId)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<GuestChoice>()
                        .HasOne<Guest>(sc => sc.Guest)
                        .WithMany(s => s.GuestChoices)
                        .HasForeignKey(sc => sc.GuestId)
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
