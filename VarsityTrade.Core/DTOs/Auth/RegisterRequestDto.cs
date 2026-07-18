using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; // Provides validation attributes like Required and EmailAddress

namespace VarsityTrade.Core.DTOs.Auth
{

    //This DTO defines the data a new user must provide when registering
    //It is what the client sends in the request body to the register endpoint
    public class RegisterRequestDto
    {
        //FirstName is required-cannpt register without it
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;

        //LastName is required-cannot register without it
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        //Email must be a valid email format and ir required
        //This is used as the login domain
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        //Password must be at least 8 characters for security
        [Required(ErrorMessage ="Password is required")]
        [MinLength(8, ErrorMessage ="Password must be at least 8 characters")]
        public string Password {  get; set; } = string.Empty;

        //UniversityID loscks the student to their campus marketplace
        //This is the most important field-it determines what listings they see
        [Required(ErrorMessage ="University is required")]
        public int UniversityId { get; set; }

        //Suburb is required for pickup coordination between buyers and sellers
        [Required(ErrorMessage ="Suburb is required")]
        public string Suburb { get; set; }= string.Empty;

        //City is required for location context 
        [Required(ErrorMessage ="City is required")]
        public string City {  get; set; } = string.Empty;

        //Province is required for location context
        [Required(ErrorMessage ="Province is required")]
        public string Province { get; set; }=string.Empty;

        //Residence is optional-not all students live in a residence
        public string? ResidenceName { get; set; }

        //Student number is optional at registration-can be added later for verification
        public string? StudentNumber { get; set; }




    }
}
