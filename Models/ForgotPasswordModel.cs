using System.ComponentModel.DataAnnotations;

public class ForgotPasswordModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; }
}
