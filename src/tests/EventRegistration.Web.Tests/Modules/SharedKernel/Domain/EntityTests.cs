using EventRegistration.SharedKernel.Domain;

namespace EventRegistration.Web.Tests.Modules.SharedKernel.Domain;

[TestClass]
public sealed class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public static TestEntity Create(Guid id) => new() { Id = id };
    }

    [TestMethod]
    public void Entities_WithSameId_AreEqual()
    {
        var id = Guid.NewGuid();
        var a = TestEntity.Create(id);
        var b = TestEntity.Create(id);

        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Entities_WithDifferentId_AreNotEqual()
    {
        var a = TestEntity.Create(Guid.NewGuid());
        var b = TestEntity.Create(Guid.NewGuid());

        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Entities_WithDefaultId_AreNotEqual()
    {
        var a = TestEntity.Create(Guid.Empty);
        var b = TestEntity.Create(Guid.Empty);

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Entity_SameReference_IsEqual()
    {
        var a = TestEntity.Create(Guid.NewGuid());

        Assert.AreEqual(a, a);
        Assert.IsTrue(a == a);
    }

    [TestMethod]
    public void Entity_ComparedWithNull_IsNotEqual()
    {
        var a = TestEntity.Create(Guid.NewGuid());

        Assert.AreNotEqual(a, null);
        Assert.IsFalse(a.Equals(null));
    }
}
