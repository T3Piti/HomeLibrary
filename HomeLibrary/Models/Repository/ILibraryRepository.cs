using HomeLibrary.Models.Entities;
using HomeLibrary.Models.ViewModels;
using System.Runtime.CompilerServices;

namespace HomeLibrary.Models.Repository
{
  public interface ILibraryRepository : IDisposable
  {
    Task<IEnumerable<Book>> GetAllAsync();
    Task<IEnumerable<Book>> GetByAuthorOrNameAsync(string searchString);
    Task<Book> GetAsync(int id);
    Task<Book> AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task<bool> DeleteAsync(int id);

  }
}
