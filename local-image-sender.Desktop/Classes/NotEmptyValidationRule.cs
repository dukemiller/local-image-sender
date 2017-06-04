using System.Globalization;
using System.Windows.Controls;

namespace local_image_sender.Desktop.Classes
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "You must enter a path here.")
                : ValidationResult.ValidResult;
        }
    }
}