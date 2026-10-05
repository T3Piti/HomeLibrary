using HomeLibrary.Models.DTO;
using HomeLibrary.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace HomeLibrary.Data
{
  public class AppDbContext : DbContext
  {
    #region Entities

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<BookAuthors> BookAuthors { get; set; } = null!;

    #endregion

    #region DTOs
    
    public DbSet<BookWithAuthorsRow> BookWithAuthorsRows { get; set; } = null!;
    public DbSet<BookIdResult> BookIdResults { get; set; } = null!;

    #endregion

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      ConfigureBookEntity(modelBuilder);
      ConfigureAuthorEntity(modelBuilder);
      ConfigureManyToManyBookAuthors(modelBuilder);
      ConfigureKeylessDtos(modelBuilder);
    }

    private void ConfigureKeylessDtos(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<BookWithAuthorsRow>(b =>
      {
        var xmlToBytesConverter = new ValueConverter<string?, byte[]>(
          xml => xml != null ? Encoding.UTF8.GetBytes(xml) : null,
          bytes => bytes != null ? Encoding.UTF8.GetString(bytes) : null);

        b.Property(x => x.TableOfContentsXml)
          .HasColumnName("TableOfContentsXml")
          .HasConversion(xmlToBytesConverter)
          .HasColumnType("VARBINARY(MAX)");
        b.HasNoKey();
      });

      modelBuilder.Entity<BookIdResult>().HasNoKey();
    }

    private void ConfigureManyToManyBookAuthors(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Book>()
          .HasMany(b => b.Authors)
          .WithMany(a => a.Books)
          .UsingEntity<BookAuthors>(
              j => j.HasOne(ba => ba.Author)
                    .WithMany()
                    .HasForeignKey(ba => ba.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict),
              j => j.HasOne(ba => ba.Book)
                    .WithMany()
                    .HasForeignKey(ba => ba.BookId)
                    .OnDelete(DeleteBehavior.Cascade),
              j =>
              {
                j.ToTable("BookAuthors");
                j.HasKey(ba => new { ba.BookId, ba.AuthorId });
                j.HasIndex(ba => ba.AuthorId);
              }
          );
    }

    private void ConfigureAuthorEntity(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Author>(a =>
      {
        a.ToTable("Authors");
        a.HasKey(x => x.Id);
        a.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
        a.Property(x => x.MiddleName).HasMaxLength(50).IsRequired();
        a.Property(x => x.LastName).HasMaxLength(50).IsRequired();
      });
    }

    private void ConfigureBookEntity(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Book>(b =>
      {
        b.ToTable("Books");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.YearPublished);

        var xmlToBytesConverter = new ValueConverter<string?, byte[]>(
            xml => xml != null ? Encoding.UTF8.GetBytes(xml) : null,
            bytes => bytes != null ? Encoding.UTF8.GetString(bytes) : null
        );

        b.Property(x => x.TableOfContentsXml)
            .HasColumnName("TableOfContentsXml")
            .HasConversion(xmlToBytesConverter)
            .HasColumnType("VARBINARY(MAX)");
      });
    }
  }
}
