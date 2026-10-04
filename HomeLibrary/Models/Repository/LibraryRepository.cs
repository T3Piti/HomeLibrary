using HomeLibrary.Data;
using HomeLibrary.Models.DTO;
using HomeLibrary.Models.Entities;
using HomeLibrary.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;
using System.Text.Json;

namespace HomeLibrary.Models.Repository
{
  public class LibraryRepository : ILibraryRepository
  {
    private readonly AppDbContext _context;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
      PropertyNameCaseInsensitive = true
    };

    public LibraryRepository(AppDbContext context) =>
      _context = context;

    public async Task<Book> AddAsync(Book book)
    {
      var parameters = new List<SqlParameter>
      {
          new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = book.Name ?? (object)DBNull.Value },
          new SqlParameter("@YearPublished", SqlDbType.Int) { Value = book.YearPublished },
          new SqlParameter("@TableOfContentsXml", SqlDbType.VarBinary)
          {
              Value = string.IsNullOrEmpty(book.TableOfContentsXml)
                  ? (object)DBNull.Value
                  : Encoding.UTF8.GetBytes(book.TableOfContentsXml)
          }
      };

      var authorsTable = new DataTable { Columns = { "FirstName", "LastName", "MiddleName" } };
      if (book.Authors?.Any() == true)
      {
        foreach (var a in book.Authors)
        {
          authorsTable.Rows.Add(
              a.FirstName,
              a.LastName,
              string.IsNullOrEmpty(a.MiddleName) ? (object)DBNull.Value : a.MiddleName
          );
        }
      }

      parameters.Add(new SqlParameter("@Authors", SqlDbType.Structured)
      {
        Value = authorsTable,
        TypeName = "dbo.AuthorList"
      });

      var bookId = (await _context.BookIdResults
          .FromSqlRaw("EXEC dbo.AddBook @Name, @YearPublished, @TableOfContentsXml, @Authors", parameters.ToArray())
          .AsAsyncEnumerable()
          .Select(x => x.BookId)
          .FirstOrDefaultAsync());

      book.Id = bookId;
      return await GetAsync(bookId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
      var param = new SqlParameter("@Id", SqlDbType.Int) { Value = id };

      var result = await _context.Database.ExecuteSqlRawAsync(
          "EXEC dbo.DeleteBook @Id",
          new[] { param });
      return result > 1 ? true : false;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
      var rows = await _context.Set<BookWithAuthorsRow>()
        .FromSqlRaw("EXEC dbo.GetAllBooks")
        .ToListAsync();

      // Группируем плоский результат в книги с коллекцией авторов
      var books = MapRowsToBooks(rows);

      return books;
    }

    public async Task<IEnumerable<Book>> GetByAuthorOrNameAsync(string? searchString)
    {

      var rows = await _context.Set<BookWithAuthorsRow>()
        .FromSqlRaw("EXEC dbo.FindBooksByAuthorOrName @SearchString",
          new SqlParameter("SearchString", (object?)searchString ?? (object?)DBNull.Value))
        .AsNoTracking()
        .ToListAsync();

      // Группируем плоский результат в книги с коллекцией авторов
      var books = MapRowsToBooks(rows);

      return books;
    }

    public async Task<Book> GetAsync(int id)
    {
      var param = new SqlParameter("@Id", SqlDbType.Int) { Value = id };

      var rows = await _context.Set<BookWithAuthorsRow>()
          .FromSqlRaw("EXEC dbo.GetBookById @Id", param)
          .ToListAsync();
      return MapRowsToBooks(rows).First();

    }

    public async Task UpdateAsync(Book book)
    {
      var parameters = new List<SqlParameter>
      {
          new SqlParameter("@Id", SqlDbType.Int) { Value = book.Id },
          new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = book.Name ?? (object)DBNull.Value },
          new SqlParameter("@YearPublished", SqlDbType.Int) { Value = book.YearPublished ?? (object)DBNull.Value },
          new SqlParameter("@TableOfContentsXml", SqlDbType.VarBinary)
          {
              Value = string.IsNullOrEmpty(book.TableOfContentsXml)
                  ? (object)DBNull.Value
                  : Encoding.UTF8.GetBytes(book.TableOfContentsXml)
          }
      };

      var authorsTable = new DataTable();
      authorsTable.Columns.Add("MiddleName", typeof(string));
      authorsTable.Columns.Add("FirstName", typeof(string));
      authorsTable.Columns.Add("LastName", typeof(string));

      foreach (var author in book.Authors)
      {
        authorsTable.Rows.Add(author.MiddleName, author.FirstName, author.LastName ?? string.Empty);
      }

      parameters.Add(new SqlParameter("@Authors", SqlDbType.Structured)
      {
        Value = authorsTable,
        TypeName = "dbo.AuthorList"
      });

      await _context.Database.ExecuteSqlRawAsync(
          "EXEC dbo.UpdateBook @Id, @Name, @YearPublished, @TableOfContentsXml, @Authors",
          parameters.ToArray());
    }

    private List<Book> MapRowsToBooks(List<BookWithAuthorsRow> rows)
    {
      return rows
          .GroupBy(r => r.Id)
          .Select(g => new Book
          {
            Id = g.Key,
            Name = g.First().Name,
            YearPublished = g.First().YearPublished,
            TableOfContentsXml = g.First().TableOfContentsXml,
            Authors = g
              .Where(r => r.AuthorId.HasValue)
              .Select(r => new Author
              {
                Id = r.AuthorId!.Value,
                FirstName = r.AuthorFirstName!,
                LastName = r.AuthorLastName!,
                MiddleName = r.AuthorMiddleName ?? string.Empty
              })
              .ToList()
          })
          .ToList();
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
