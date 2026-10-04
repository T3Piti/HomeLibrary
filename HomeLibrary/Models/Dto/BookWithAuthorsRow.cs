namespace HomeLibrary.Models.DTO
{
  public class BookWithAuthorsRow
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? YearPublished { get; set; }
    public string? TableOfContentsXml { get; set; }

    public int? AuthorId { get; set; }
    public string? AuthorFirstName { get; set; }
    public string? AuthorLastName { get; set; }
    public string? AuthorMiddleName { get; set; }
  }
}
