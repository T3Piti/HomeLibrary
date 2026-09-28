using HomeLibrary.Models;
using HomeLibrary.Models.Dto;
using HomeLibrary.Models.Repository;
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
    public async Task<IActionResult> Index()
    {
      var books = await _libraryRepository.GetAllAsync();
      return View(books);
    }

    // GET: /books/5 — карточка книги (с формой редактирования)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
      var book = await _libraryRepository.GetAsync(id);
      if (book == null)
        return NotFound();

      var authors = book.Authors?
          .Select(a => new AuthorInput
          {
            FirstName = a.FirstName,
            LastName = a.LastName,
            MiddleName = a.MiddleName ?? string.Empty
          })
          .ToList() ?? new List<AuthorInput>();

      var viewModel = new EditBookViewModel
      {
        Id = book.Id,
        Name = book.Name,
        YearPublished = book.YearPublished,
        TableOfContentsXml = book.TableOfContentsXml ?? string.Empty,
        // XML → HTML для редактора
        TableOfContentsHtml = TocConverter.XmlToHtml(book.TableOfContentsXml),
        AuthorsJson = JsonSerializer.Serialize(authors, new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })
      };

      return View(viewModel);
    }

    // GET: /books/create
    [HttpGet("create")]
    public IActionResult Create()
    {
      return View(new Book { Authors = new List<Author>() });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book, IFormFile? tocFile)
    {
      if (tocFile != null && tocFile.Length > 0)
      {
        using var reader = new StreamReader(tocFile.OpenReadStream());
        book.TableOfContentsXml = await reader.ReadToEndAsync();
      }

      await _libraryRepository.AddAsync(book);
      return RedirectToAction(nameof(Index));
    }

    // POST: /books/5/edit — сохранение из карточки
    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBookViewModel vm)
    {
      if (id != vm.Id)
        return BadRequest();

      if (!ModelState.IsValid)
        return View("Details", vm);

      // HTML из редактора → XML для БД
      vm.TableOfContentsXml = TocConverter.HtmlToXml(vm.TableOfContentsHtml ?? string.Empty);

      await _libraryRepository.UpdateAsync(vm);
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
  }
}
