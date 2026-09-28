using HomeLibrary.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Models.Dto
{
  public class Book
  {
    public int Id { get; set; }

    //[Required(ErrorMessage = "Укажите автора")]
    [StringLength(200)]
    public string Name { get; set; }

    public string TableOfContentsXml { get; set; }

    [YearRange(0, ErrorMessage = "Укажите корректный год издания.")]
    public int? YearPublished { get; set; }

    //[Required(ErrorMessage = "Укажите автора")]
    public IList<Author> Authors { get; set; }
  }
}
