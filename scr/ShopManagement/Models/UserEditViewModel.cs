using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Models
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Citizen ID")]
        public string CitizenId { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}