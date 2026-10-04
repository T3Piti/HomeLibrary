using HomeLibrary.Models.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Models.ViewModels;

public class CreateBookViewModel
{
  [Required(ErrorMessage = "Название обязательно")]
  [Display(Name = "Название книги")]
  public string Name { get; set; } = string.Empty;

  [YearRange(0, ErrorMessage = "Укажите корректный год издания.")]
  [Display(Name = "Год издания")]
  public int YearPublished { get; set; }

  [Required]
  public IFormFile? TocFile { get; set; }

  public List<AuthorInput> Authors { get; set; } = new();
}
