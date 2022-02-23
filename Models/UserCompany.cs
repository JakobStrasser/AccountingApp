using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace AccountingApp.Models
{
    public class UserCompany
    {
        public int Id { get; set; }
       
        public string UserName { get; set; }  
        public int CompanyId { get; set; }
    }}
