using NUnit.Framework;

public class GameStateModelTests
{
    [Test]
    public void NewModel_StartsPlayingWithInputAllowed()
    {
        var model = new GameStateModel();

        Assert.That(model.CurrentState, Is.EqualTo(GameState.Playing));
        Assert.That(model.CanAcceptPlayerInput, Is.True);
    }

    [TestCase(GameState.Dialogue)]
    [TestCase(GameState.Settings)]
    public void OpenAndCloseUI_ReturnsToPlaying(GameState uiState)
    {
        var model = new GameStateModel();

        Assert.That(model.TryTransition(GameState.Playing, uiState), Is.True);
        Assert.That(model.CurrentState, Is.EqualTo(uiState));
        Assert.That(model.CanAcceptPlayerInput, Is.False);
        Assert.That(model.TryTransition(uiState, GameState.Playing), Is.True);
        Assert.That(model.CurrentState, Is.EqualTo(GameState.Playing));
        Assert.That(model.CanAcceptPlayerInput, Is.True);
    }

    [TestCase(GameState.Dialogue, GameState.Settings)]
    [TestCase(GameState.Settings, GameState.Dialogue)]
    public void UIStates_CannotTransitionToEachOther(GameState current, GameState next)
    {
        var model = CreateAt(current);

        Assert.That(model.TryTransition(current, next), Is.False);
        Assert.That(model.CurrentState, Is.EqualTo(current));
        Assert.That(model.CanAcceptPlayerInput, Is.False);
    }

    [TestCase(GameState.Playing)]
    [TestCase(GameState.Dialogue)]
    [TestCase(GameState.Settings)]
    public void EnterDead_FromAnyLivingStateLocksInput(GameState current)
    {
        var model = CreateAt(current);

        Assert.That(model.EnterDead(), Is.True);
        Assert.That(model.CurrentState, Is.EqualTo(GameState.Dead));
        Assert.That(model.CanAcceptPlayerInput, Is.False);
    }

    [Test]
    public void Dead_RejectsEveryOrdinaryTransition()
    {
        var model = CreateAt(GameState.Dead);
        foreach (GameState expected in System.Enum.GetValues(typeof(GameState)))
        {
            foreach (GameState next in System.Enum.GetValues(typeof(GameState)))
            {
                Assert.That(model.TryTransition(expected, next), Is.False);
                Assert.That(model.CurrentState, Is.EqualTo(GameState.Dead));
                Assert.That(model.CanAcceptPlayerInput, Is.False);
            }
        }
    }

    [TestCase(GameState.Playing)]
    [TestCase(GameState.Dialogue)]
    [TestCase(GameState.Settings)]
    public void OrdinaryTransition_CannotEnterDead(GameState current)
    {
        var model = CreateAt(current);

        Assert.That(model.TryTransition(current, GameState.Dead), Is.False);
        Assert.That(model.CurrentState, Is.EqualTo(current));
    }

    [TestCase(GameState.Playing, GameState.Dialogue, GameState.Playing)]
    [TestCase(GameState.Dialogue, GameState.Playing, GameState.Settings)]
    [TestCase(GameState.Settings, GameState.Playing, GameState.Dialogue)]
    [TestCase(GameState.Dialogue, GameState.Settings, GameState.Playing)]
    [TestCase(GameState.Settings, GameState.Dialogue, GameState.Playing)]
    public void MismatchedExpectedState_DoesNotChangeState(
        GameState current, GameState expected, GameState next)
    {
        var model = CreateAt(current);

        Assert.That(model.TryTransition(expected, next), Is.False);
        Assert.That(model.CurrentState, Is.EqualTo(current));
    }

    [TestCase(GameState.Dialogue)]
    [TestCase(GameState.Settings)]
    public void RepeatedOpenAndClose_ReturnFalseWithoutChangingState(GameState uiState)
    {
        var model = new GameStateModel();

        Assert.That(model.TryTransition(GameState.Playing, uiState), Is.True);
        Assert.That(model.TryTransition(GameState.Playing, uiState), Is.False);
        Assert.That(model.CurrentState, Is.EqualTo(uiState));
        Assert.That(model.TryTransition(uiState, GameState.Playing), Is.True);
        Assert.That(model.TryTransition(uiState, GameState.Playing), Is.False);
        Assert.That(model.CurrentState, Is.EqualTo(GameState.Playing));
    }

    [TestCase(GameState.Playing)]
    [TestCase(GameState.Dialogue)]
    [TestCase(GameState.Settings)]
    [TestCase(GameState.Dead)]
    public void SameStateTransition_ReturnsFalse(GameState current)
    {
        var model = CreateAt(current);

        Assert.That(model.TryTransition(current, current), Is.False);
        Assert.That(model.CurrentState, Is.EqualTo(current));
    }

    [Test]
    public void RepeatedEnterDead_ReturnsFalseAndRemainsDead()
    {
        var model = new GameStateModel();

        Assert.That(model.EnterDead(), Is.True);
        Assert.That(model.EnterDead(), Is.False);
        Assert.That(model.CurrentState, Is.EqualTo(GameState.Dead));
        Assert.That(model.CanAcceptPlayerInput, Is.False);
    }

    [TestCase(GameState.Dialogue)]
    [TestCase(GameState.Settings)]
    [TestCase(GameState.Dead)]
    public void NewModel_DoesNotInheritOrShareOldState(GameState oldState)
    {
        var oldModel = CreateAt(oldState);
        var newModel = new GameStateModel();

        Assert.That(newModel.CurrentState, Is.EqualTo(GameState.Playing));
        Assert.That(newModel.CanAcceptPlayerInput, Is.True);
        Assert.That(oldModel.CurrentState, Is.EqualTo(oldState));
        oldModel.EnterDead();
        Assert.That(newModel.CurrentState, Is.EqualTo(GameState.Playing));
        Assert.That(newModel.CanAcceptPlayerInput, Is.True);
    }

    private static GameStateModel CreateAt(GameState state)
    {
        var model = new GameStateModel();
        if (state == GameState.Dead)
        {
            Assert.That(model.EnterDead(), Is.True);
        }
        else if (state != GameState.Playing)
        {
            Assert.That(model.TryTransition(GameState.Playing, state), Is.True);
        }
        return model;
    }
}
