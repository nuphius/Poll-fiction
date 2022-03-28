using System.ComponentModel.DataAnnotations;

namespace PollFiction.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Login obligatoire")]
        [Display(Name ="Login")]
        public string Pseudo { get; set; }
        [Required(ErrorMessage ="Mot de passe obligatoire")]
        [DataType(DataType.Password)]
        [Display(Name ="Mot de passe")]
        [RegularExpression("^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).*$", ErrorMessage = "Minimum 8 caractères avec minuscules, majuscules et chiffres obligatoires")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        [Display(Name = "Garder la session ouverte")]
        public bool RememberMe { get; set; }

        public string Error { get; set; }
    }
}
