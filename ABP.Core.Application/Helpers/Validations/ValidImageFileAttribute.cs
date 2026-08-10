using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ABP.Core.Application.Helpers.Validations
{
    public class ValidImageFileAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (!ImageValidator.ValidateFile(file, out string errorMessage))
                {
                    return new ValidationResult(errorMessage);
                }
            }
            else if (value is IEnumerable<IFormFile> files)
            {
                foreach (var f in files)
                {
                    if (!ImageValidator.ValidateFile(f, out string errorMessage))
                    {
                        return new ValidationResult(errorMessage);
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}
