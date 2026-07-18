using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VarsityTrade.Core.DTOs.Auth
{
    //This DTO defines the data user must provide to log in
    //Simple-just email and password
    public class LoginRequestDto
    {
        //Email is the unique identifier used to find the user accoubt
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        //Password is verified against the stored has using identity
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

    }
}
