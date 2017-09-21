using System;
using System.ComponentModel.DataAnnotations;

namespace OneRosterProviderDemo.Validators
{
    public class NotEmptyArrayAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Array arr = value as Array;

            if(arr.Length == 0)
            {
                return new ValidationResult($"At least one {validationContext.MemberName} required");
            }
            return ValidationResult.Success;
        }
    }
}
