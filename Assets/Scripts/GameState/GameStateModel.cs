public enum GameState
{
    Playing,
    Dialogue,
    Settings,
    Dead
}

public sealed class GameStateModel
{
    public GameState CurrentState { get; private set; }

    public bool CanAcceptPlayerInput
    {
        get { return CurrentState == GameState.Playing; }
    }

    public GameStateModel()
    {
        CurrentState = GameState.Playing;
    }

    // 只有预期状态匹配且属于允许的 UI 转换时才改变状态。
    public bool TryTransition(GameState expectedState, GameState nextState)
    {
        if (CurrentState != expectedState)
        {
            return false;
        }

        bool allowed =
            (CurrentState == GameState.Playing &&
                (nextState == GameState.Dialogue || nextState == GameState.Settings)) ||
            ((CurrentState == GameState.Dialogue || CurrentState == GameState.Settings) &&
                nextState == GameState.Playing);

        if (!allowed)
        {
            return false;
        }

        CurrentState = nextState;
        return true;
    }

    // Dead 只能通过创建新模型解除；重复上报不发生转换。
    public bool EnterDead()
    {
        if (CurrentState == GameState.Dead)
        {
            return false;
        }

        CurrentState = GameState.Dead;
        return true;
    }
}
