using System;
using System.Collections.Generic;
using LETS.Models;

namespace LETS.ViewModels
{
    public class MemberViewModel
    {
        public int Id;
        public int IdLocality { get; set; }
        public string UserName { get; set; }
        public DateTime JoinDate { get; set; }
        public int Turnover { get; set; }
        public int Balance { get; set; }
        public string Telephone { get; set; }
        public IEnumerable<dynamic> Fields { get; set; }
        public string Email { get; set; }
        public string LastFirstName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Locality { get; set; }
    }
}