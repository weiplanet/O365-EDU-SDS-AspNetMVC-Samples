/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OneRosterProviderDemo.Validators
{
    public class GradesAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string[] grades = value as string[];
            if(grades.All(grade => Vocabulary.Grades.Members.Contains(grade)))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid grade entry");
        }
    }
}
