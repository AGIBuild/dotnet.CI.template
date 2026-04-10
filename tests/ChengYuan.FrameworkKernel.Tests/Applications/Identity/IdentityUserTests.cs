using ChengYuan.Identity;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class IdentityUserTests
{
    [Fact]
    public void IdentityUser_ShouldNormalizeAndTrimUserNameAndEmail()
    {
        var user = new IdentityUser(Guid.NewGuid(), " Alice ", " Alice@example.com ");

        user.UserName.ShouldBe("Alice");
        user.NormalizedUserName.ShouldBe("ALICE");
        user.Email.ShouldBe("Alice@example.com");
        user.NormalizedEmail.ShouldBe("ALICE@EXAMPLE.COM");
        user.IsActive.ShouldBeTrue();
        user.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void IdentityUser_ShouldMarkDeletedAndDeactivateTheUser()
    {
        var user = new IdentityUser(Guid.NewGuid(), "Alice", "alice@example.com");

        user.MarkDeleted();

        user.IsDeleted.ShouldBeTrue();
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void IdentityUser_ShouldManageRoleAssignmentsWithoutDuplicates()
    {
        var user = new IdentityUser(Guid.NewGuid(), "Alice", "alice@example.com");
        var firstRoleId = Guid.NewGuid();
        var secondRoleId = Guid.NewGuid();

        user.AssignRole(firstRoleId);
        user.AssignRole(firstRoleId);
        user.AssignRole(secondRoleId);
        user.UnassignRole(firstRoleId);

        user.Roles.Select(role => role.RoleId).ShouldBe([secondRoleId]);
    }
}
