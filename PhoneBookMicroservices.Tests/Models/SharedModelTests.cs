using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBookMicroservices.Tests.Models
{
    public class SharedModelTests
    {
        [Fact]
        public void Person_ShouldInitializeWithDefaultValues()
        {
            var person = new Person();

            Assert.Equal(Guid.Empty, person.Id);
            Assert.Null(person.FirstName);
            Assert.Null(person.LastName);
            Assert.Null(person.Company);
            Assert.Empty(person.ContactInfos);
        }

        [Fact]
        public void Person_ShouldSetPropertiesCorrectly()
        {
            var personId = Guid.NewGuid();
            var contactInfos = new List<ContactInfo>
            {
                new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.PhoneNumber, InfoContent = "123456789" }
            };

            var person = new Person
            {
                Id = personId,
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp",
                ContactInfos = contactInfos
            };

            Assert.Equal(personId, person.Id);
            Assert.Equal("John", person.FirstName);
            Assert.Equal("Doe", person.LastName);
            Assert.Equal("ABC Corp", person.Company);
            Assert.Equal(contactInfos, person.ContactInfos);
        }

        [Fact]
        public void Person_ContactInfos_ShouldBeEmptyList_WhenInitialized()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };

            person.ContactInfos = new List<ContactInfo>();

            Assert.Empty(person.ContactInfos);
        }

        [Fact]
        public void Person_ContactInfos_ShouldAddNewContactInfo()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp",
                ContactInfos = new List<ContactInfo>()
            };
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                InfoType = InfoType.Email,
                InfoContent = "john.doe@example.com"
            };

            person.ContactInfos.Add(contactInfo);

            Assert.Single(person.ContactInfos);
            Assert.Equal(contactInfo, person.ContactInfos[0]);
        }

        [Fact]
        public void ContactInfo_ShouldInitializeWithDefaultValues()
        {
            var contactInfo = new ContactInfo();

            Assert.Equal(Guid.Empty, contactInfo.Id);
            Assert.Equal(InfoType.PhoneNumber, contactInfo.InfoType);
            Assert.Null(contactInfo.InfoContent);
        }

        [Fact]
        public void ContactInfo_ShouldSetPropertiesCorrectly()
        {
            var contactInfoId = Guid.NewGuid();

            var contactInfo = new ContactInfo
            {
                Id = contactInfoId,
                InfoType = InfoType.Email,
                InfoContent = "john.doe@example.com"
            };

            Assert.Equal(contactInfoId, contactInfo.Id);
            Assert.Equal(InfoType.Email, contactInfo.InfoType);
            Assert.Equal("john.doe@example.com", contactInfo.InfoContent);
        }

        [Fact]
        public void ContactInfo_ShouldHandleDifferentInfoTypes()
        {
            var contactInfo = new ContactInfo();

            contactInfo.InfoType = InfoType.Location;
            contactInfo.InfoContent = "Istanbul";

            Assert.Equal(InfoType.Location, contactInfo.InfoType);
            Assert.Equal("Istanbul", contactInfo.InfoContent);
        }

        [Fact]
        public void ContactInfo_ShouldHandlePhoneNumberInfoType()
        {
            var contactInfo = new ContactInfo();

            contactInfo.InfoType = InfoType.PhoneNumber;
            contactInfo.InfoContent = "123456789";

            Assert.Equal(InfoType.PhoneNumber, contactInfo.InfoType);
            Assert.Equal("123456789", contactInfo.InfoContent);
        }

        [Fact]
        public void ContactInfo_ShouldHandleNullInfoContent()
        {
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                InfoType = InfoType.Email,
                InfoContent = null
            };

            Assert.Null(contactInfo.InfoContent);
        }
    }
}
