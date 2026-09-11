using UnityEngine;

internal abstract class EnemyStateBehaviour : IEnemyStateBehaviour
{
    protected EnemyBase Enemy { get; }

    public abstract EnemyState State { get; }

    protected EnemyStateBehaviour(EnemyBase enemy)
    {
        Enemy = enemy;
    }

    public abstract void Enter(EnemyState previousState);
    public abstract void Tick();
    public abstract void Exit();
}

internal sealed class EnemyIdleState : EnemyStateBehaviour
{
    public override EnemyState State => EnemyState.idle;

    public EnemyIdleState(EnemyBase enemy) : base(enemy)
    {
    }

    public override void Enter(EnemyState previousState)
    {
        Enemy.am.SetBool("IsRun", false);
        if (previousState == EnemyState.walk)
        {
            Enemy.ScheduleIldeToWalk(2f);
        }
    }

    public override void Tick()
    {
        Enemy.rb.linearVelocity = Vector2.zero;
    }

    public override void Exit()
    {
        Enemy.CancelIldeToWalk();
    }
}

internal sealed class EnemyWalkState : EnemyStateBehaviour
{
    public override EnemyState State => EnemyState.walk;

    public EnemyWalkState(EnemyBase enemy) : base(enemy)
    {
    }

    public override void Enter(EnemyState previousState)
    {
        Enemy.am.SetBool("IsRun", true);
    }

    public override void Tick()
    {
        Enemy.rb.linearVelocity = (Enemy.targetPos.position - Enemy.transform.position).normalized * Enemy.Config.MoveSpeed;
        Enemy.sr.flipX = Enemy.rb.linearVelocity.x < 0;

        if (Vector2.Distance(Enemy.transform.position, Enemy.pos1.position) < 0.1f)
        {
            if (Enemy.targetPos == Enemy.pos1)
            {
                Enemy.targetPos = Enemy.pos2;
                Enemy.ChangeState(EnemyState.idle);
            }
        }
        else if (Vector2.Distance(Enemy.transform.position, Enemy.pos2.position) < 0.1f)
        {
            if (Enemy.targetPos == Enemy.pos2)
            {
                Enemy.targetPos = Enemy.pos1;
                Enemy.ChangeState(EnemyState.idle);
            }
        }
    }

    public override void Exit()
    {
    }
}

internal sealed class EnemyPursuitState : EnemyStateBehaviour
{
    public override EnemyState State => EnemyState.pursuit;

    public EnemyPursuitState(EnemyBase enemy) : base(enemy)
    {
    }

    public override void Enter(EnemyState previousState)
    {
        Enemy.am.SetBool("IsRun", true);
    }

    public override void Tick()
    {
        Enemy.rb.linearVelocity = (Enemy.targetPos.position - Enemy.transform.position).normalized * Enemy.Config.MoveSpeed;
        Enemy.sr.flipX = Enemy.rb.linearVelocity.x < 0;

        if (Enemy.targetPos == Enemy.pos1 || Enemy.targetPos == Enemy.pos2)
        {
            Enemy.ChangeState(EnemyState.walk);
        }
        else if (Vector2.Distance(Enemy.transform.position, Enemy.targetPos.position) < Enemy.Config.AttackDistance)
        {
            if (Enemy.sr.flipX && Enemy.targetPos.position.x - Enemy.transform.position.x < 0)
            {
                Enemy.ChangeState(EnemyState.attack);
            }
            else if (!Enemy.sr.flipX && Enemy.targetPos.position.x - Enemy.transform.position.x > 0)
            {
                Enemy.ChangeState(EnemyState.attack);
            }
        }
    }

    public override void Exit()
    {
    }
}

internal sealed class EnemyAttackState : EnemyStateBehaviour
{
    public override EnemyState State => EnemyState.attack;

    public EnemyAttackState(EnemyBase enemy) : base(enemy)
    {
    }

    public override void Enter(EnemyState previousState)
    {
        Enemy.am.SetBool("IsRun", false);
    }

    public override void Tick()
    {
        Enemy.rb.linearVelocity = Vector2.zero;

        if (Enemy.canAttack)
        {
            if (Vector2.Distance(Enemy.transform.position, Enemy.targetPos.position) > Enemy.Config.AttackDistance)
            {
                Enemy.ChangeState(EnemyState.pursuit);
            }
            else if (Enemy.sr.flipX && Enemy.targetPos.position.x - Enemy.transform.position.x > 0)
            {
                Enemy.ChangeState(EnemyState.pursuit);
            }
            else if (!Enemy.sr.flipX && Enemy.targetPos.position.x - Enemy.transform.position.x < 0)
            {
                Enemy.ChangeState(EnemyState.pursuit);
            }
            else
            {
                Enemy.am.SetTrigger("Attack1");
                Enemy.canAttack = false;
            }
        }

        if (Enemy.AttackTimer < Enemy.Config.AttackCooldown)
        {
            Enemy.AttackTimer += Time.deltaTime;
        }
        else
        {
            Enemy.AttackTimer = 0f;
            Enemy.canAttack = true;
        }
    }

    public override void Exit()
    {
        Enemy.CancelAttackToWalk();
    }

    public void ScheduleWalkAfterRemainingCooldown()
    {
        Enemy.ScheduleAttackToWalk(Enemy.Config.AttackCooldown - Enemy.AttackTimer);
    }
}

internal sealed class EnemyGetHitState : EnemyStateBehaviour
{
    public override EnemyState State => EnemyState.getHit;

    public EnemyGetHitState(EnemyBase enemy) : base(enemy)
    {
    }

    public override void Enter(EnemyState previousState)
    {
        Enemy.am.SetTrigger("GetHit");
        Enemy.am.SetBool("IsRun", false);
        Enemy.rb.linearVelocity = Vector2.zero;
        Enemy.GetHitTimer = 0f;
    }

    public override void Tick()
    {
        Enemy.GetHitTimer += Time.deltaTime;
        if (Enemy.GetHitTimer <= 0.2f)
        {
            Enemy.rb.linearVelocity = (Enemy.transform.position - Enemy.attackerPos.position).normalized * 2f;
        }
        else
        {
            Enemy.rb.linearVelocity = Vector2.zero;
        }

        if (Enemy.GetHitTimer >= 1f)
        {
            Enemy.GetHitTimer = 0f;
            Enemy.ChangeState(EnemyState.pursuit);
        }
    }

    public override void Exit()
    {
    }
}

internal sealed class EnemyDeadState : EnemyStateBehaviour
{
    public override EnemyState State => EnemyState.dead;

    public EnemyDeadState(EnemyBase enemy) : base(enemy)
    {
    }

    public override void Enter(EnemyState previousState)
    {
        Enemy.am.SetBool("IsRun", false);
        Enemy.am.SetTrigger("Dead");
        Enemy.ScheduleDestroyEnemyAndPos(1f);
        Enemy.DestroyHpSlider();
    }

    public override void Tick()
    {
        Enemy.rb.linearVelocity = Vector2.zero;
    }

    public override void Exit()
    {
    }
}
