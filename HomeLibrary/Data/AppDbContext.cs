using HomeLibrary.Models.DTO;
using HomeLibrary.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace HomeLibrary.Data
{
  public class AppDbContext : DbContext
  {
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<BookIdResult> BookIdResults { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {
      Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      var xmlToBytesConverter = new ValueConverter<string, byte[]>(
           xml => XmlStringToBytes(xml),
           bytes => BytesToXmlString(bytes) 
      );

      modelBuilder.Entity<Book>()
        .Property(e => e.TableOfContentsXml)
        .HasConversion(xmlToBytesConverter!);

      modelBuilder.Entity<Book>()
          .HasMany(b => b.Authors)
          .WithMany(a => a.Books)
          .UsingEntity<Dictionary<string, object>>(
              "BookAuthor",
              ba => ba.HasOne<Author>()
                      .WithMany()
                      .HasForeignKey("AuthorId")
                      .OnDelete(DeleteBehavior.Cascade),
              ba => ba.HasOne<Book>()
                      .WithMany()
                      .HasForeignKey("BookId")
                      .OnDelete(DeleteBehavior.Cascade)
          );
      modelBuilder.Entity<BookIdResult>()
        .HasNoKey()
        .ToTable("BookIdResults", t => t.ExcludeFromMigrations());

      modelBuilder.Entity<BookWithAuthorsRow>()
        .HasNoKey()
        .ToTable("BookWithAuthorsRows", t => t.ExcludeFromMigrations());

      modelBuilder.Entity<BookWithAuthorsRow>()
        .Property(e => e.TableOfContentsXml)
        .HasConversion(xmlToBytesConverter!);

      modelBuilder.Entity<BookWithAuthorsRow>()
        .HasNoKey()
        .ToTable("BookWithAuthorsRows", t => t.ExcludeFromMigrations());

    }

    private byte[] XmlStringToBytes(string xml)
    {
      if (string.IsNullOrWhiteSpace(xml)) return Array.Empty<byte>();
      return Encoding.UTF8.GetBytes(xml);
    }

    private string BytesToXmlString(byte[] bytes)
    {
      if (bytes == null || bytes.Length == 0) return string.Empty;
      return Encoding.UTF8.GetString(bytes);
    }

  }
}
