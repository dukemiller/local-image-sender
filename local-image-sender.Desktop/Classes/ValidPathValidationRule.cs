using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace local_image_sender.Desktop.Classes
{
    public class ValidPathValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return !Directory.Exists(System.Convert.ToString(value))
                ? new ValidationResult(false, "This is an invalid path.")
                : ValidationResult.ValidResult;
        }
    }
}