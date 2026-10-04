using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Models.ViewModels
{
  public class AuthorInput
  {
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string MiddleName { get; set; } = string.Empty;

    public string? LastName { get; set; }
  }
}
