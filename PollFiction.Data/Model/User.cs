using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class User
    {   
        [Key]
        public int UserId { get; set; }
        [Required]
        [StringLength(maximumLength: 50)]
        public string UserName { get; set; }
        [Required]
        [StringLength(maximumLength: 50)]
        public string UserPseudo { get; set; }
        [Required]
        [StringLength(maximumLength: 100)]
        public string UserMail { get; set; }
        public string UserUuidCookie { get; set; }
        [Required]
        [StringLength(maximumLength: 255)]
        public string UserPwd { get; set; }

        //relation 1 - n avec la table Poll
        public ICollection<Poll> Polls { get; set; }

    }
}
