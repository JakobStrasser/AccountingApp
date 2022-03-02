using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class Account
    {
       
        public int Id { get; set; }
        public int AccountNumber { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; }
        public bool Standard { get; set; }
        public bool Used { get; set; }
        public int AccountGroupId { get; set; }
        public AccountGroup AccountGroup { get; set; }
      
    }
}
