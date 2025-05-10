using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBookMicroservices.Shared.DTOs
{
    public class ContactInfoDto
    {
        public Guid Id { get; set; }
        public InfoType InfoType { get; set; }
        public string InfoContent { get; set; }
    }
}
