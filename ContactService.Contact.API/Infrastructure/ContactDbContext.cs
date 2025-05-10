using Microsoft.EntityFrameworkCore;
using PhoneBookMicroservices.Shared.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ContactService.Contact.API.Infrastructure
{
    public class ContactDbContext : DbContext
    {
        public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options) { }

        public DbSet<Person> Contacts { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<ContactReport> Reports { get; set; }
    }

}
