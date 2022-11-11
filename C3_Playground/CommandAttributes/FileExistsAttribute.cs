using System.ComponentModel.DataAnnotations;

namespace C3_Playground.CommandAttributes
{
    public class FileExistsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string directory && (File.Exists(directory)))
                return ValidationResult.Success;
            return new ValidationResult($"The File '{value}' does not exist.");
        }
    }
}
