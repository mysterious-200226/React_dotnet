using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PHP.QARAdjustmentTool.API.DTOs
{
    public class AddUserRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
        public string UserFirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        public string UserLastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Roles are required.")]
        public string Roles { get; set; }

        [StringLength(50, ErrorMessage = "Availity User ID cannot exceed 50 characters.")]
        public string? AvailityUserId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserEmail != null && !UserEmail.Contains("."))
            {
                yield return new ValidationResult(
                    "The email address must contain a period in the domain (e.g., example.com).",
                    new[] { nameof(UserEmail) }
                );
            }
        }
    }
}