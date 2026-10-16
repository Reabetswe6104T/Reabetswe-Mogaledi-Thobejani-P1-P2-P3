using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;
namespace RaceDayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Result> Results { get; set; }
        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
            mb.Entity<Participant>().HasIndex(p => p.IDNumber).IsUnique();
            mb.Entity<Participant>().HasIndex(p => p.UserID).IsUnique();
            mb.Entity<Organiser>().HasIndex(o => o.UserID).IsUnique();
            mb.Entity<Participant>().HasOne(p => p.User).WithOne(u => u.Participant).HasForeignKey<Participant>(p => p.UserID).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Organiser>().HasOne(o => o.User).WithOne(u => u.Organiser).HasForeignKey<Organiser>(o => o.UserID).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Event>().HasOne(e => e.Organiser).WithMany(o => o.Events).HasForeignKey(e => e.OrganiserID).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Category>().HasOne(c => c.Event).WithMany(e => e.Categories).HasForeignKey(c => c.EventID).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Entry>().HasOne(e => e.Participant).WithMany(p => p.Entries).HasForeignKey(e => e.ParticipantID).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Entry>().HasOne(e => e.Event).WithMany(ev => ev.Entries).HasForeignKey(e => e.EventID).OnDelete(DeleteBehavior.NoAction);
            mb.Entity<Entry>().HasOne(e => e.Category).WithMany(c => c.Entries).HasForeignKey(e => e.CategoryID).OnDelete(DeleteBehavior.NoAction);
            mb.Entity<Result>().HasOne(r => r.Entry).WithOne(e => e.Result).HasForeignKey<Result>(r => r.EntryID).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
