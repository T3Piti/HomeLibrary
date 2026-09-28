using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Attributes
{
  public class YearRangeAttribute : ValidationAttribute
  {
    private readonly int _minYear;

    public YearRangeAttribute(int minYear) =>
      _minYear = minYear;

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      if (value is int year)
      {
        int currentYear = DateTime.Now.Year;
        if (year < _minYear || year >= currentYear)
        {
          return new ValidationResult($"Год должен быть от {_minYear} до {currentYear}.");
        }
      }
      return ValidationResult.Success;
    }
  }
}
