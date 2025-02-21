using Moq;
using Microsoft.AspNetCore.Identity;
using Hotel_Core.Services;
using ContactsManager.Core.Domain.IdentityEntities;
using System.Security.Claims;
using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hotel_ServiceTests
{
    public class UserClaimServiceFixture
    {
        public Mock<UserManager<ApplicationUser>> MockUserManager { get; }
        public UserClaimService UserClaimService { get; }

        public UserClaimServiceFixture()
        {

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            MockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            UserClaimService = new UserClaimService(MockUserManager.Object);
        }

        public Mock<ApplicationUser> GetMockUser(Guid userId)
        {
            var mockUser = new Mock<ApplicationUser>();
            mockUser.Setup(u => u.Id).Returns(userId);

            return mockUser;
        }
        
        public IList<Claim> GetClaims()
        {
            return new List<Claim>
            {
                new("Permission", "SomePermission"),
                new("Role", "Admin")
            };
        }
    }
}