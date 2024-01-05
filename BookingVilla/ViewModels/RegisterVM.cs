using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BookingVilla.ViewModels
{
    public class RegisterVM
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
        [Required]
        [Range(2,25)]
        public string Name { get; set; }
        [Display(Name ="Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Role")]
        public string? Role { get; set; }
        public string? RedirectUrl { get; set; }
        public IEnumerable<SelectListItem>? RoleList { get; set; }

    }
}
