namespace amped_bookmark;

using Microsoft.EntityFrameworkCore;

public class BookmarkDb : DbContext
{
    public BookmarkDb(DbContextOptions<BookmarkDb> options)
        : base(options) { }

    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(b => new {b.Owner, b.Id});
                entity.Property(b => b.Uri).HasConversion<string>();
                entity.Property(b => b.Title).IsRequired();
            }
        );
    }
}