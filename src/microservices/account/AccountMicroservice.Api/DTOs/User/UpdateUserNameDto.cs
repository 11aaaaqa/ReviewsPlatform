using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AccountMicroservice.Api.DTOs.User
{
    public class UpdateUserNameDto
    {
        private string newUserName;
        [Required]
        [StringLength(30)]
        public string NewUserName
        {
            get => newUserName;
            set => newUserName = value != null ? Regex.Replace(value.Trim(), @"\s+", " ") : null;
        }
    }
}
