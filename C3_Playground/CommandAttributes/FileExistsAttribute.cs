using System.ComponentModel.DataAnnotations;

namespace C3_Playground.CommandAttributes
{
    public class FileExistsAttribute : ValidationAttribute
    {
        private readonly bool _acceptsNull;
        public FileExistsAttribute(bool acceptsNull = false) { _acceptsNull = acceptsNull; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return _acceptsNull ? ValidationResult.Success : new ValidationResult("Null value cannot be a valid file");
            if (value is string directory && (File.Exists(directory)))
                return ValidationResult.Success;
            return new ValidationResult($"The File '{value}' does not exist.");
        }
    }
}
