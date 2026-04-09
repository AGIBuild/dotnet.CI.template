using ChengYuan.Identity;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class IdentityRoleTests
{
    [Fact]
    public void IdentityRole_ShouldNormalizeAndTrimName()
    {
        var role = new IdentityRole(Guid.NewGuid(), " Administrators ");

        role.Name.ShouldBe("Administrators");
        role.NormalizedName.ShouldBe("ADMINISTRATORS");
        role.IsActive.ShouldBeTrue();
        role.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void IdentityRole_ShouldMarkDeletedAndDeactivateTheRole()
    {
        var role = new IdentityRole(Guid.NewGuid(), "Administrators");

        role.MarkDeleted();

        role.IsDeleted.ShouldBeTrue();
        role.IsActive.ShouldBeFalse();
    }
}
