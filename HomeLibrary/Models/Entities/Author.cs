using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Models.Entities
{
  public class Author
  {
    public int Id { get; set; }
    [StringLength(50)]
    public string FirstName { get; set; }
    [StringLength(50)]
    public string MiddleName { get; set; }
    [StringLength(50)]
    public string LastName { get; set; }

    public IList<Book> Books { get; set; }
  }
}