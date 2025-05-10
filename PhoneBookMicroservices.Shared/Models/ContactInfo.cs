using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBookMicroservices.Shared.Models
{
    public class ContactInfo
    {
        public Guid Id { get; set; }
        public InfoType InfoType { get; set; }
        public string InfoContent { get; set; }
    }

}
