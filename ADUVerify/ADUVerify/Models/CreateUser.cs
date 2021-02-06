using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ADUVerify.Models
{
    public class CreateUser
    {

        public string OU { get; set; }

        public string SubOU { get; set; }

        [Display(Name="User Name")]
        [Required]
        public string UserName { get; set; }


        [Display(Name = "Password")]
        [Required]
        public string Password { get; set; }

        public string Mobileno { get; set; }

        public string Emailid { get; set; }
        public  List<string> groups { get; set; }

        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }
        [Display(Name = "Department")]
        [Required]
        public string Department { get; set; }

        [Display(Name = "Reporting Manager")]
        public string ReportingManager { get; set; }

        public string State { get; set; }
    }
}