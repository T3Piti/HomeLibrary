using HomeLibrary.Models.Dto;
using System.Runtime.CompilerServices;

namespace HomeLibrary.Models.Repository
{
  public interface ILibraryRepository : IDisposable
  {
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book> GetAsync(int id);
    Task<Book> AddAsync(Book book);
    Task UpdateAsync(EditBookViewModel book);
    Task<bool> DeleteAsync(int id);

  }
}
