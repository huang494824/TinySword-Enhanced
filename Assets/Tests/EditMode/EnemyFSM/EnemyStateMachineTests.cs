using System;
using System.Collections.Generic;
using NUnit.Framework;

public class EnemyStateMachineTests
{
    [Test]
    public void EnemyState_ValuesRemainSerializationCompatible()
    {
        Assert.That((int)EnemyState.idle, Is.EqualTo(0));
        Assert.That((int)EnemyState.walk, Is.EqualTo(1));
        Assert.That((int)EnemyState.pursuit, Is.EqualTo(2));
        Assert.That((int)EnemyState.attack, Is.EqualTo(3));
        Assert.That((int)EnemyState.getHit, Is.EqualTo(4));
        Assert.That((int)EnemyState.dead, Is.EqualTo(5));
    }

    [Test]
    public void Initialize_SetsOwnershipAndEntersInitialStateOnce()
    {
        var idle = new RecordingState(EnemyState.idle);
        var machine = new EnemyStateMachine(idle);

        machine.Initialize(EnemyState.idle);

        Assert.That(machine.IsInitialized, Is.True);
        Assert.That(machine.CurrentState, Is.EqualTo(EnemyState.idle));
        Assert.That(machine.PreviousState, Is.EqualTo(EnemyState.idle));
        Assert.That(idle.EnterCount, Is.EqualTo(1));
        Assert.That(idle.ExitCount, Is.Zero);
    }

    [Test]
    public void ChangeState_ExitsThenUpdatesOwnershipThenEnters()
    {
        var calls = new List<string>();
        EnemyStateMachine machine = null;
        var idle = new RecordingState(
            EnemyState.idle,
            onExit: () => calls.Add($"exit:{machine.CurrentState}:{machine.PreviousState}"));
        var walk = new RecordingState(
            EnemyState.walk,
            onEnter: _ => calls.Add($"enter:{machine.CurrentState}:{machine.PreviousState}"));
        machine = new EnemyStateMachine(idle, walk);
        machine.Initialize(EnemyState.idle);
        calls.Clear();

        machine.ChangeState(EnemyState.walk);

        CollectionAssert.AreEqual(
            new[] { "exit:idle:idle", "enter:walk:idle" },
            calls);
    }

    [Test]
    public void Tick_TicksOnlyCurrentState()
    {
        var idle = new RecordingState(EnemyState.idle);
        var walk = new RecordingState(EnemyState.walk);
        var machine = new EnemyStateMachine(idle, walk);
        machine.Initialize(EnemyState.idle);

        machine.Tick();
        machine.ChangeState(EnemyState.walk);
        machine.Tick();
        machine.Tick();

        Assert.That(idle.TickCount, Is.EqualTo(1));
        Assert.That(walk.TickCount, Is.EqualTo(2));
    }

    [Test]
    public void ChangeState_ToSameState_PerformsFullReentry()
    {
        var state = new RecordingState(EnemyState.getHit);
        var machine = new EnemyStateMachine(state);
        machine.Initialize(EnemyState.getHit);

        machine.ChangeState(EnemyState.getHit);

        Assert.That(machine.CurrentState, Is.EqualTo(EnemyState.getHit));
        Assert.That(machine.PreviousState, Is.EqualTo(EnemyState.getHit));
        Assert.That(state.EnterCount, Is.EqualTo(2));
        Assert.That(state.ExitCount, Is.EqualTo(1));
    }

    [Test]
    public void MultipleMachines_MaintainIndependentOwnershipAndStateObjects()
    {
        var firstIdle = new RecordingState(EnemyState.idle);
        var firstWalk = new RecordingState(EnemyState.walk);
        var secondIdle = new RecordingState(EnemyState.idle);
        var secondWalk = new RecordingState(EnemyState.walk);
        var first = new EnemyStateMachine(firstIdle, firstWalk);
        var second = new EnemyStateMachine(secondIdle, secondWalk);
        first.Initialize(EnemyState.idle);
        second.Initialize(EnemyState.walk);

        first.ChangeState(EnemyState.walk);
        first.Tick();

        Assert.That(first.CurrentState, Is.EqualTo(EnemyState.walk));
        Assert.That(first.PreviousState, Is.EqualTo(EnemyState.idle));
        Assert.That(second.CurrentState, Is.EqualTo(EnemyState.walk));
        Assert.That(second.PreviousState, Is.EqualTo(EnemyState.walk));
        Assert.That(firstWalk.TickCount, Is.EqualTo(1));
        Assert.That(secondWalk.TickCount, Is.Zero);
    }

    [Test]
    public void Tick_WhenStateRequestsTransition_CompletesTransitionImmediately()
    {
        EnemyStateMachine machine = null;
        var walk = new RecordingState(EnemyState.walk);
        var idle = new RecordingState(
            EnemyState.idle,
            onTick: () => machine.ChangeState(EnemyState.walk));
        machine = new EnemyStateMachine(idle, walk);
        machine.Initialize(EnemyState.idle);

        machine.Tick();

        Assert.That(machine.CurrentState, Is.EqualTo(EnemyState.walk));
        Assert.That(machine.PreviousState, Is.EqualTo(EnemyState.idle));
        Assert.That(idle.ExitCount, Is.EqualTo(1));
        Assert.That(walk.EnterCount, Is.EqualTo(1));
    }

    [Test]
    public void ChangeState_WhenStateIsMissing_FailsBeforeExitOrOwnershipChange()
    {
        var idle = new RecordingState(EnemyState.idle);
        var machine = new EnemyStateMachine(idle);
        machine.Initialize(EnemyState.idle);

        Assert.Throws<KeyNotFoundException>(() => machine.ChangeState(EnemyState.walk));
        Assert.That(machine.CurrentState, Is.EqualTo(EnemyState.idle));
        Assert.That(machine.PreviousState, Is.EqualTo(EnemyState.idle));
        Assert.That(idle.ExitCount, Is.Zero);
    }

    private sealed class RecordingState : IEnemyStateBehaviour
    {
        private readonly Action<EnemyState> onEnter;
        private readonly Action onTick;
        private readonly Action onExit;

        public EnemyState State { get; }
        public int EnterCount { get; private set; }
        public int TickCount { get; private set; }
        public int ExitCount { get; private set; }

        public RecordingState(
            EnemyState state,
            Action<EnemyState> onEnter = null,
            Action onTick = null,
            Action onExit = null)
        {
            State = state;
            this.onEnter = onEnter;
            this.onTick = onTick;
            this.onExit = onExit;
        }

        public void Enter(EnemyState previousState)
        {
            EnterCount++;
            onEnter?.Invoke(previousState);
        }

        public void Tick()
        {
            TickCount++;
            onTick?.Invoke();
        }

        public void Exit()
        {
            ExitCount++;
            onExit?.Invoke();
        }
    }
}
