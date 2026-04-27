using EventRegistration.Registrations.Domain;

namespace EventRegistration.Web.Tests.Modules.Registrations.Domain;

[TestClass]
public sealed class RegistrationTests
{
    [TestMethod]
    public void Create_WithConfirmedStatus_ReturnsRegistration()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト太郎", "test@example.com", RegistrationStatus.Confirmed);

        Assert.IsNotNull(registration);
        Assert.AreNotEqual(Guid.Empty, registration.Id);
        Assert.AreEqual("テスト太郎", registration.ParticipantName);
        Assert.AreEqual("test@example.com", registration.Email);
        Assert.AreEqual(RegistrationStatus.Confirmed, registration.Status);
        Assert.IsNull(registration.CancelledAt);
    }

    [TestMethod]
    public void Create_WithWaitListedStatus_ReturnsRegistration()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト花子", "hanako@example.com", RegistrationStatus.WaitListed);

        Assert.AreEqual(RegistrationStatus.WaitListed, registration.Status);
    }

    [TestMethod]
    public void Create_EmailIsNormalized()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト", " Test@EXAMPLE.COM ", RegistrationStatus.Confirmed);

        Assert.AreEqual("test@example.com", registration.Email);
    }

    [TestMethod]
    public void Create_ParticipantNameIsTrimmed()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), " テスト太郎 ", "test@example.com", RegistrationStatus.Confirmed);

        Assert.AreEqual("テスト太郎", registration.ParticipantName);
    }

    [TestMethod]
    public void Create_NullParticipantName_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Registration.Create(Guid.NewGuid(), null!, "test@example.com", RegistrationStatus.Confirmed));
    }

    [TestMethod]
    public void Create_NullEmail_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Registration.Create(Guid.NewGuid(), "テスト", null!, RegistrationStatus.Confirmed));
    }

    [TestMethod]
    public void Cancel_ConfirmedRegistration_SetsCancelled()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト", "test@example.com", RegistrationStatus.Confirmed);

        registration.Cancel();

        Assert.AreEqual(RegistrationStatus.Cancelled, registration.Status);
        Assert.IsNotNull(registration.CancelledAt);
    }

    [TestMethod]
    public void Cancel_WaitListedRegistration_SetsCancelled()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト", "test@example.com", RegistrationStatus.WaitListed);

        registration.Cancel();

        Assert.AreEqual(RegistrationStatus.Cancelled, registration.Status);
        Assert.IsNotNull(registration.CancelledAt);
    }

    [TestMethod]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト", "test@example.com", RegistrationStatus.Confirmed);
        registration.Cancel();

        Assert.ThrowsExactly<InvalidOperationException>(() => registration.Cancel());
    }

    [TestMethod]
    public void Confirm_WaitListedRegistration_SetsConfirmed()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト", "test@example.com", RegistrationStatus.WaitListed);

        registration.Confirm();

        Assert.AreEqual(RegistrationStatus.Confirmed, registration.Status);
    }

    [TestMethod]
    public void Confirm_ConfirmedRegistration_ThrowsInvalidOperationException()
    {
        var registration = Registration.Create(
            Guid.NewGuid(), "テスト", "test@example.com", RegistrationStatus.Confirmed);

        Assert.ThrowsExactly<InvalidOperationException>(() => registration.Confirm());
    }

    [TestMethod]
    public void Create_InvalidEmailFormat_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Registration.Create(Guid.NewGuid(), "テスト", "invalid-email", RegistrationStatus.Confirmed));
    }

    [TestMethod]
    public void Create_ValidEmail_DoesNotThrow()
    {
        var registration = Registration.Create(Guid.NewGuid(), "テスト", "test@example.com", RegistrationStatus.Confirmed);
        Assert.IsNotNull(registration);
    }

    [TestMethod]
    public void Create_CancelledStatus_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Registration.Create(Guid.NewGuid(), "テスト", "test@example.com", RegistrationStatus.Cancelled));
    }

    [TestMethod]
    public void NormalizeEmail_TrimsAndLowercases()
    {
        Assert.AreEqual("test@example.com", Registration.NormalizeEmail(" Test@EXAMPLE.COM "));
    }
}
