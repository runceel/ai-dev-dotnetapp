using EventRegistration.SharedKernel.Domain;

namespace EventRegistration.Web.Tests.Modules.SharedKernel.Domain;

[TestClass]
public sealed class EntityTests
{
    [TestMethod]
    public void Entities_WithSameId_AreEqual()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Entities_WithDifferentIds_AreNotEqual()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Entity_ComparedWithNull_IsNotEqual()
    {
        var entity = new TestEntity(Guid.NewGuid());

        Assert.IsFalse(entity.Equals(null));
        Assert.IsTrue(entity != null);
    }

    [TestMethod]
    public void Entity_ComparedWithSelf_IsEqual()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var sameEntity = entity;

        Assert.IsTrue(entity.Equals(sameEntity));
        Assert.IsTrue(entity == sameEntity);
    }

    [TestMethod]
    public void Entities_WithDefaultId_AreNotEqual()
    {
        var a = new TestEntity(Guid.Empty);
        var b = new TestEntity(Guid.Empty);

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Entity_DefaultId_NotEqualToNonDefaultId()
    {
        var a = new TestEntity(Guid.Empty);
        var b = new TestEntity(Guid.NewGuid());

        Assert.AreNotEqual(a, b);
    }

    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
    }
}
