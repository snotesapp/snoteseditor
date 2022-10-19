using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Data
{
    public class SNotesDBContext : DbContext
    {

        public DbSet<Project> Projects { get; set; }
        public DbSet<Note> Note { get; set; }
        public DbSet<NotesCollection> NotesCollection { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<NoteCard> NoteCards { get; set; }
        public DbSet<NoteImage> NoteImage { get; set; }
        public DbSet<NotePath> NotePath { get; set; }
        public const string DatabaseName = "sNotes.db3";
        string databasPath;


        public SNotesDBContext(DbContextOptions<SNotesDBContext> options):base(options)
        {
            //SQLitePCL.Batteries_V2.Init();
            //this.Database.EnsureCreated();

            // this.Database.Migrate();


        }

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            #region Multi-Platforms db Path


            //  databasPath = DependencyService.Get<IPath>().GetDatabasePath(DatabaseName);
            //   optionsBuilder.UseSqlite($"Filename={databasPath}");


            #endregion

            #region Migrate for Local DataBase
            //Uncomment this and Comment DependencyService

            optionsBuilder.UseSqlite($"Filename={DatabaseName}");

            #endregion

        }
        */


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Project>().Property(b => b.Name).HasMaxLength(100);
            modelBuilder.Entity<Card>().HasOne(b => b.Project).WithMany(p => p.Cards).HasForeignKey(b => b.ProjectFK);
            modelBuilder.Entity<Card>().Property(b => b.Title).HasMaxLength(100);
            modelBuilder.Entity<Card>().Property(b => b.Resume).HasMaxLength(1500);
           // modelBuilder.Entity<Card>().Property(csl => csl.Selected).HasDefaultValue(true);
            modelBuilder.Entity<NotesCollection>().HasOne(ig => ig.Project).WithMany(p => p.NotesCollection).HasForeignKey(ig => ig.ProjectFK);
            modelBuilder.Entity<NotesCollection>().Property(b => b.Title).HasMaxLength(100);
            modelBuilder.Entity<NotesCollection>().Property(isl => isl.Selected).HasDefaultValue(false);
            modelBuilder.Entity<NotesCollection>().Ignore(ed => ed.Editable);

            modelBuilder.Entity<Note>().HasOne(i => i.NotesCollection).WithMany(ig => ig.Note).HasForeignKey(i => i.NotesCollectionFK);
            modelBuilder.Entity<Note>().Property(b => b.Text).HasMaxLength(1500);
            modelBuilder.Entity<Note>().Ignore(sl => sl.Selected);
            modelBuilder.Entity<Note>().Ignore(th => th.Thumbnail);

            modelBuilder.Entity<NoteCard>().HasKey(c => new { c.CardID, c.NoteID });
            modelBuilder.Entity<NoteCard>().HasOne(cd => cd.Card).WithMany(c => c.NoteCards).HasForeignKey(cd => cd.CardID);
            modelBuilder.Entity<NoteCard>().HasOne(cd => cd.Note).WithMany(i => i.NoteCards).HasForeignKey(cd => cd.NoteID);
            modelBuilder.Entity<NoteCard>().Ignore(dv => dv.IsDragOver);
   

            modelBuilder.Entity<Card>().HasOne(c => c.Parent).WithMany().HasForeignKey(p => p.ParentID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Card>().Ignore(ed => ed.Editable);

            modelBuilder.Entity<NoteImage>().HasOne(n => n.Note).WithMany(i => i.Images).HasForeignKey(nf => nf.NoteFK);
            modelBuilder.Entity<NoteImage>().Ignore(bn => bn.Bounds);
            modelBuilder.Entity<NoteImage>().Ignore(lc => lc.Location);
            modelBuilder.Entity<NoteImage>().Ignore(sl => sl.Selected);

            modelBuilder.Entity<NotePath>().HasOne(n => n.Note).WithMany(i => i.NotePaths).HasForeignKey(nf => nf.NoteFK);
        }

    }
}
