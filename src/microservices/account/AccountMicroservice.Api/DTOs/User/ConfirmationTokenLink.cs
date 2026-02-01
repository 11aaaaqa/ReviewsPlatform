using System.ComponentModel.DataAnnotations;

namespace AccountMicroservice.Api.DTOs.User
{
    public class ConfirmationTokenLink
    {
        [Required]
        [ValidateConfirmationLinkUrl]
        [Display(Description = "Path to an endpoint, you use to validate token, that will be inserted into email message as a link. "
                               + "Format: \"[Scheme]://[domain]/[path]?[parameterForToken]=\" "
                               + "For example: \"https://example.com/user/confirm-email?token=\"")]
        public string Url { get; set; }
    }

    class ValidateConfirmationLinkUrlAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value == null)
                return ValidationResult.Success;

            string url = value.ToString()!;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return new ValidationResult("Invalid url format");

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return new ValidationResult("Invalid scheme");

            if (string.IsNullOrWhiteSpace(uri.Host))
                return new ValidationResult("Invalid url");

            if (!url.EndsWith("="))
                return new ValidationResult("Invalid format");

            return ValidationResult.Success;
        }
    }
}