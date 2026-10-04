using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Models.ViewModels;

public class EditBookViewModel
{
  public int Id { get; set; }

  [StringLength(200)]
  public string Name { get; set; } = null!;

  public string TableOfContentsXml { get; set; } = string.Empty;
  public string TableOfContentsHtml { get; set; } = string.Empty;

  public int? YearPublished { get; set; }

  public IList<AuthorInput> Authors { get; set; }
}
