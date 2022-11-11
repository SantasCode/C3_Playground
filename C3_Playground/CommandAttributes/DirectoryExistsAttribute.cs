using System.ComponentModel.DataAnnotations;

namespace C3_Playground.CommandAttributes
{
    public class DirectoryExistsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string directory && (Directory.Exists(directory)))
                return ValidationResult.Success;
            return new ValidationResult($"The Directory '{value}' does not exist.");
        }
    }
}
