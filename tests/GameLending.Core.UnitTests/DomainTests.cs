using GameLending.Core.Domain;
namespace GameLending.Core.UnitTests;

public sealed class DomainTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");
    private static Game Game() => Domain.Game.Create("Chrono Trigger", ["PS5"], Now);
    private static Friend Friend() => Domain.Friend.Create("Diogo", "diogo@example.com", Now);
    [Fact] public void CannotLoanGameTwice() => Assert.Throws<DomainException>(() => Loan.Create(Game(), Friend(), true, Now));
    [Fact] public void CannotLoanInactiveGame() { var game = Game(); game.Deactivate(false, Now); Assert.Throws<DomainException>(() => Loan.Create(game, Friend(), false, Now)); }
    [Fact] public void CannotLoanToInactiveFriend() { var friend = Friend(); friend.Deactivate(false, Now); Assert.Throws<DomainException>(() => Loan.Create(Game(), friend, false, Now)); }
    [Fact] public void ReturnReleasesLoan() { var loan = Loan.Create(Game(), Friend(), false, Now); loan.Return(Now.AddDays(1)); Assert.False(loan.IsActive); Assert.Equal(Now.AddDays(1), loan.ReturnedAt); }
    [Fact] public void CannotReturnTwice() { var loan = Loan.Create(Game(), Friend(), false, Now); loan.Return(Now); Assert.Throws<DomainException>(() => loan.Return(Now)); }
    [Fact] public void CannotReturnBeforeBorrowing() { var loan = Loan.Create(Game(), Friend(), false, Now); Assert.Throws<DomainException>(() => loan.Return(Now.AddSeconds(-1))); }
    [Fact] public void CannotDeactivateBorrowedGame() => Assert.Throws<DomainException>(() => Game().Deactivate(true, Now));
    [Fact] public void CannotDeactivateBorrower() => Assert.Throws<DomainException>(() => Friend().Deactivate(true, Now));
    [Theory][InlineData("")][InlineData("   ")] public void GameRequiresTitle(string title) => Assert.Throws<DomainException>(() => Domain.Game.Create(title, [], Now));
    [Fact] public void FriendRequiresValidEmail() => Assert.Throws<DomainException>(() => Domain.Friend.Create("Alice", "invalid", Now));
    [Fact] public void SourceRequiresHttps() => Assert.Throws<DomainException>(() => new CatalogSource("http://example.com"));
    [Fact] public void UpdatesPreserveIdentity() { var game = Game(); var id = game.Id; game.Update("New title", ["PS4"], Now.AddDays(1)); Assert.Equal(id, game.Id); Assert.Equal("New title", game.Title); }
}
