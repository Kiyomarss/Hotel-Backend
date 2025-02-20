using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Hotel_Core.Services;
using ContactsManager.Core.Domain.IdentityEntities;

namespace Hotel_ServiceTests
{
    public class IdentityServiceFixture : IDisposable
    {
        public Mock<IHttpContextAccessor> MockHttpContextAccessor { get; }
        public Mock<UserManager<ApplicationUser>> MockUserManager { get; }
        public IdentityService IdentityService { get; }

        public IdentityServiceFixture()
        {
            MockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            MockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            IdentityService = new IdentityService(MockHttpContextAccessor.Object, MockUserManager.Object);
        }

        public void Dispose() { }
    }
}
