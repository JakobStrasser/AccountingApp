using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class AccountGroup
    {

        public int Id { get; set; }
        public int AccountGroupNumber { get; set; }
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; }
        public int AccountClassId { get; set; }
        public List<Account> Accounts { get; set; }

    }
}
