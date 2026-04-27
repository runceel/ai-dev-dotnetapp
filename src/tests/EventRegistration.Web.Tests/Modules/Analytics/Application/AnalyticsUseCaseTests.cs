using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Application.UseCases;
using EventRegistration.Analytics.Domain;

namespace EventRegistration.Web.Tests.Modules.Analytics.Application;

[TestClass]
public sealed class GetEventStatisticsUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsync_DelegatesToRepository()
    {
        var eventId = Guid.NewGuid();
        var expected = new EventStatistics(eventId, 1, 2, 3, 4);

        var repo = Substitute.For<IRegistrationActivityRepository>();
        repo.GetEventStatisticsAsync(eventId, Arg.Any<CancellationToken>()).Returns(expected);

        var useCase = new GetEventStatisticsUseCase(repo);

        var actual = await useCase.ExecuteAsync(eventId);

        Assert.AreSame(expected, actual);
    }
}

[TestClass]
public sealed class GetDailyStatisticsUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsync_DelegatesToRepository_WithGivenRange()
    {
        var eventId = Guid.NewGuid();
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 3);
        IReadOnlyList<DailyStatistics> expected =
        [
            new(from, 1, 0, 0, 0),
            new(from.AddDays(1), 0, 1, 0, 0),
            new(to, 0, 0, 1, 0),
        ];

        var repo = Substitute.For<IRegistrationActivityRepository>();
        repo.GetDailyStatisticsAsync(eventId, from, to, Arg.Any<CancellationToken>())
            .Returns(expected);

        var useCase = new GetDailyStatisticsUseCase(repo);

        var actual = await useCase.ExecuteAsync(eventId, from, to);

        Assert.AreEqual(3, actual.Count);
        CollectionAssert.AreEqual((System.Collections.ICollection)expected, (System.Collections.ICollection)actual);
    }

    [TestMethod]
    public async Task ExecuteAsync_FromAfterTo_Throws()
    {
        var repo = Substitute.For<IRegistrationActivityRepository>();
        var useCase = new GetDailyStatisticsUseCase(repo);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(Guid.NewGuid(), new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 1)));
    }
}
