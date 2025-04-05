using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers
{
    public class ValidationHelper
    {
        internal static void ModelValidation(object obj)
        {
            //Model validations
            ValidationContext validationContext = new ValidationContext(obj);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            //personAddRequest to obiekt, który jest walidowany.
            //validationContext to kontekst walidacji.
            //validationResults to lista, w której będą przechowywane ewentualne błędy.
            //true oznacza, że walidacja ma obejmować właściwości obiektu oraz jego zagnieżdżone obiekty (jeśli są).
            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);

            if (!isValid)
                throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);
        }
    }
}
