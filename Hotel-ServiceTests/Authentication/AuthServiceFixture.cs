using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.ServiceContracts;
using Hotel_Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Hotel_ServiceTests;

public class AuthServiceFixture : IDisposable
{
    public Mock<IIdentityService> MockIdentityService { get; }
    public Mock<UserManager<ApplicationUser>> MockUserManager { get; }
    public Mock<SignInManager<ApplicationUser>> MockSignInManager { get; }
    public Mock<IConfiguration> MockConfiguration { get; }
    public AuthService AuthService { get; }

    public AuthServiceFixture()
    {
        MockIdentityService = new Mock<IIdentityService>();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        MockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

        MockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                                                                     MockUserManager.Object,
                                                                     new Mock<IHttpContextAccessor>().Object,
                                                                     new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                                                                     null, null, null, null);

        MockConfiguration = new Mock<IConfiguration>();

        AuthService = new AuthService(
                                      MockUserManager.Object,
                                      MockSignInManager.Object,
                                      MockConfiguration.Object,
                                      MockIdentityService.Object);
    }

    public void Dispose() { }
}
