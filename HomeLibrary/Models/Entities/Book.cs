using HomeLibrary.Models.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Models.Entities
{
  public class Book
  {
    public int Id { get; set; }

    [StringLength(200)]
    public string Name { get; set; }

    public string TableOfContentsXml { get; set; }

    [YearRange(0, ErrorMessage = "Укажите корректный год издания.")]
    public int? YearPublished { get; set; }

    public IList<Author> Authors { get; set; }
  }
}
