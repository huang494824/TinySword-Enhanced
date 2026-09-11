using System;
using System.Collections.Generic;

public sealed class EnemyStateMachine
{
    private readonly Dictionary<EnemyState, IEnemyStateBehaviour> states;
    private IEnemyStateBehaviour currentBehaviour;

    public bool IsInitialized { get; private set; }
    public EnemyState CurrentState { get; private set; }
    public EnemyState PreviousState { get; private set; }

    public EnemyStateMachine(params IEnemyStateBehaviour[] stateBehaviours)
    {
        if (stateBehaviours == null)
        {
            throw new ArgumentNullException(nameof(stateBehaviours));
        }

        states = new Dictionary<EnemyState, IEnemyStateBehaviour>(stateBehaviours.Length);
        foreach (IEnemyStateBehaviour stateBehaviour in stateBehaviours)
        {
            if (stateBehaviour == null)
            {
                throw new ArgumentException("Enemy state behaviours cannot contain null.", nameof(stateBehaviours));
            }

            if (states.ContainsKey(stateBehaviour.State))
            {
                throw new ArgumentException($"Enemy state {stateBehaviour.State} is registered more than once.", nameof(stateBehaviours));
            }

            states.Add(stateBehaviour.State, stateBehaviour);
        }
    }

    public void Initialize(EnemyState initialState)
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Enemy state machine is already initialized.");
        }

        IEnemyStateBehaviour initialBehaviour = GetRegisteredState(initialState);
        CurrentState = initialState;
        PreviousState = initialState;
        currentBehaviour = initialBehaviour;
        IsInitialized = true;
        currentBehaviour.Enter(PreviousState);
    }

    public void Tick()
    {
        EnsureInitialized();
        currentBehaviour.Tick();
    }

    public void ChangeState(EnemyState nextState)
    {
        EnsureInitialized();

        // Resolve first so a missing state cannot leave the current lifecycle half-exited.
        IEnemyStateBehaviour nextBehaviour = GetRegisteredState(nextState);

        currentBehaviour.Exit();
        PreviousState = CurrentState;
        CurrentState = nextState;
        currentBehaviour = nextBehaviour;
        currentBehaviour.Enter(PreviousState);
    }

    private IEnemyStateBehaviour GetRegisteredState(EnemyState state)
    {
        if (!states.TryGetValue(state, out IEnemyStateBehaviour stateBehaviour))
        {
            throw new KeyNotFoundException($"Enemy state {state} is not registered.");
        }

        return stateBehaviour;
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Enemy state machine must be initialized before use.");
        }
    }
}
