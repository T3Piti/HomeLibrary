using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HomeLibrary.Models.Dto;

public class EditBookViewModel
{
  public int Id { get; set; }

  [StringLength(200)]
  public string Name { get; set; } = null!;

  public string TableOfContentsXml { get; set; } = string.Empty;
  public string TableOfContentsHtml { get; set; } = string.Empty;

  public int? YearPublished { get; set; }

  /// <summary>
  /// JSON-строка: [{"firstName":"...","lastName":"...","middleName":"..."}]
  /// </summary>
  public string AuthorsJson { get; set; } = "[]";
}

public class AuthorInput
{
  public string FirstName { get; set; } = null!;
  public string LastName { get; set; } = string.Empty;
  public string MiddleName { get; set; } = null!;
}
