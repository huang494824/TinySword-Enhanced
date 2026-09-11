public interface IEnemyStateBehaviour
{
    EnemyState State { get; }

    void Enter(EnemyState previousState);
    void Tick();
    void Exit();
}
