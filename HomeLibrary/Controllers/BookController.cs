using HomeLibrary.Models;
using HomeLibrary.Models.Entities;
using HomeLibrary.Models.Repository;
using HomeLibrary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HomeLibrary.Controllers
{
  [Route("books")]
  public class BookController : Controller
  {
    private readonly ILibraryRepository _libraryRepository;

    public BookController(ILibraryRepository libraryRepository)
    {
      _libraryRepository = libraryRepository;
    }

    // GET: /books
    [HttpGet("")]
    public async Task<IActionResult> Index(string? searchString)
    {
      var books = await _libraryRepository.GetByAuthorOrNameAsync(searchString);
      return View(books);
    }

    // GET: /books/5 — карточка книги (с формой редактирования)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
      var book = await _libraryRepository.GetAsync(id);
      if (book == null)
        return NotFound();

      var viewModel = MapBookToEditViewModel(book);

      return View(viewModel);
    }

    private static EditBookViewModel MapBookToEditViewModel(Book book)
    {
      var viewModel = new EditBookViewModel
      {
        Id = book.Id,
        Name = book.Name,
        YearPublished = book.YearPublished,
        TableOfContentsXml = book.TableOfContentsXml ?? string.Empty,
        // XML → HTML для редактора
        TableOfContentsHtml = TocConverter.XmlToHtml(book.TableOfContentsXml),
      };

      if (book.Authors != null)
        viewModel.Authors = new List<AuthorInput>();

      foreach (var author in book.Authors)
      {
        var vmAuthor = new AuthorInput()
        {
          FirstName = author.FirstName,
          MiddleName = author.MiddleName,
          LastName = author.LastName,
        };
        viewModel.Authors.Add(vmAuthor);
      }

      return viewModel;
    }

    // GET: /books/create
    [HttpGet("create")]
    public IActionResult Create()
    {
      return View(new CreateBookViewModel { Authors = new List<AuthorInput>() });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookViewModel viewModel)
    {
      if (!ModelState.IsValid)
        return View(viewModel);
      var book = await MapCreateBookViewModelToBook(viewModel);

      await _libraryRepository.AddAsync(book);
      return RedirectToAction(nameof(Index));
    }

    // POST: /books/5/edit — сохранение из карточки
    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBookViewModel viewModel)
    {
      if (id != viewModel.Id)
        return BadRequest();

      if (!ModelState.IsValid)
        return View("Details", viewModel);

      var book = MapEditBookViewModelToBook(viewModel);

      await _libraryRepository.UpdateAsync(book);
      return RedirectToAction(nameof(Index));
    }

    // POST: /books/5/delete
    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
      await _libraryRepository.DeleteAsync(id);
      return RedirectToAction(nameof(Index));
    }

    #region Private methods

    private static Book MapEditBookViewModelToBook(EditBookViewModel viewModel)
    {
      var book = new Book()
      {
        Name = viewModel.Name,
        YearPublished = viewModel.YearPublished,
        TableOfContentsXml = TocConverter.HtmlToXml(viewModel.TableOfContentsHtml ?? string.Empty)
      };

      foreach (var authorInput in viewModel.Authors)
      {
        var author = new Author()
        {
          FirstName = authorInput.FirstName,
          MiddleName = authorInput.MiddleName,
          LastName = authorInput.LastName
        };
        book.Authors.Add(author);
      }



      return book;
    }

    private static async Task<Book> MapCreateBookViewModelToBook(CreateBookViewModel viewModel)
    {
      var book = new Book()
      {
        Name = viewModel.Name,
        YearPublished = viewModel.YearPublished,
        Authors = new List<Author>()
      };

      foreach (var authorInput in viewModel.Authors)
      {
        var author = new Author()
        {
          FirstName = authorInput.FirstName,
          MiddleName = authorInput.MiddleName,
          LastName = authorInput.LastName
        };
        book.Authors.Add(author);
      }

      if (viewModel.TocFile != null && viewModel.TocFile.Length > 0)
      {
        using var reader = new StreamReader(viewModel.TocFile.OpenReadStream());
        book.TableOfContentsXml = await reader.ReadToEndAsync();
      }

      return book;
    }

    #endregion
  }
}
