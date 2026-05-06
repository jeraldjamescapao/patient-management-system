namespace MedCore.Modules.Users.Tests.Application.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using MedCore.Common.Caching;
using MedCore.Modules.Identity.Domain.Users;
using MedCore.Modules.Users.Application.Abstractions;
using MedCore.Modules.Users.Application.Services;
using MedCore.Modules.Users.Tests.Helpers;
using NSubstitute;

public abstract class UserServiceTestBase
{
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IUserCultureCache UserCultureCache;
    protected readonly IUserService Sut;

    protected UserServiceTestBase()
    {
        UserManager = MockUserManager.Create();
        UserCultureCache = Substitute.For<IUserCultureCache>();
        
        Sut = new UserService(
            UserManager, 
            UserCultureCache,
            NullLogger<UserService>.Instance);
    }

    protected static ApplicationUser CreateUser()
    {
        return ApplicationUser.Create(
            email: "jjcapaotest@softwareengineers.ch",
            firstName: "Jerald James Capao",
            lastName: "Test",
            birthDate: new DateOnly(1988, 6, 27),
            createdBy: ApplicationUser.SelfRegisteredActor);
    }
        
}