using System.ComponentModel.DataAnnotations;

namespace PollFiction.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Les Pseudo est obligatoire !")]
        public string Pseudo { get; set; }
        [Display(Name ="Nom")]
        public string Name { get; set; }
        [Required(ErrorMessage = "L'adresse mail est obligatoire !")]
        [DataType(DataType.EmailAddress)]
        [Display(Name ="Adresse Mail")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage ="L'E-mail saisie n'est pas valide")]
        public string Mail { get; set; }
        [Required(ErrorMessage = "Le mot de passe est obligatoire !")]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [Display(Name = "Mot de passe")]
        [RegularExpression("^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).*$", ErrorMessage ="Minimum 8 caractères avec minuscules, majuscules et chiffres obligatoires")]
        public string Password { get; set; }
        [Required(ErrorMessage = "La confirmation de mot de passe est obligatoire !")]
        [Compare("Password", ErrorMessage ="Les deux mot de passe ne sont pas identiques")]
        [Display (Name ="Confirmation du mot de passe")]
        [DataType (DataType.Password)]
        public string ConfirmPwd { get; set; }
    }
}
