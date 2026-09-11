using UnityEngine;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private EnemyConfig enemyConfig;

    [Header("组件")]
    public Rigidbody2D rb;
    public Animator am;
    public SpriteRenderer sr;
    public EnemyState state = EnemyState.walk;
    public EnemyState stateOld = EnemyState.walk;
    public Slider hpSlider;
    public GameObject coinGO;
    [Header("移动相关")]
    public Transform pos1, pos2;
    public Transform targetPos;
    [Header("攻击相关")]
    private float attackTimer = 0f;
    public bool canAttack = true;
    private float getHitTimer = 0f;
    public Transform attackerPos;
    public GameObject enemyAndPos;
    public GameObject attackPerfab;
    public Transform attack1PosL;
    public Transform attack1PosR;

    [Header("基础属性")]
    public float HPNow = 100f;
    private bool configurationErrorReported;
    private EnemyStateMachine stateMachine;
    private EnemyAttackState attackState;

    internal EnemyConfig Config => enemyConfig;
    internal float AttackTimer
    {
        get => attackTimer;
        set => attackTimer = value;
    }
    internal float GetHitTimer
    {
        get => getHitTimer;
        set => getHitTimer = value;
    }

    private EnemyState RuntimeState => stateMachine != null && stateMachine.IsInitialized
        ? stateMachine.CurrentState
        : state;

    private bool HasRequiredConfig()
    {
        if (enemyConfig != null)
        {
            return true;
        }

        if (!configurationErrorReported)
        {
            Debug.LogError("EnemyBase requires enemyConfig. Bind an EnemyConfig before running.", this);
            configurationErrorReported = true;
        }

        enabled = false;
        return false;
    }

    public virtual void Start()
    {
        if (!HasRequiredConfig()) return;

        targetPos = pos1;
        HPNow = enemyConfig.MaxHealth;
        EnsureStateMachineCreated();
        if (stateMachine.IsInitialized)
        {
            ChangeState(EnemyState.walk);
        }
        else
        {
            try
            {
                stateMachine.Initialize(EnemyState.walk);
            }
            finally
            {
                SyncStateMirrors();
            }
        }
    }

    public virtual void Update()
    {
        if (stateMachine != null && stateMachine.IsInitialized)
        {
            stateMachine.Tick();
        }
    }

    public virtual void FixedUpdate()
    {

    }

    #region 状态机
    public virtual void ChangeState(EnemyState newState)
    {
        EnsureStateMachineCreated();
        try
        {
            if (stateMachine.IsInitialized)
            {
                stateMachine.ChangeState(newState);
            }
            else
            {
                stateMachine.Initialize(newState);
            }
        }
        finally
        {
            SyncStateMirrors();
        }
    }

    public virtual void IldeToWalk()
    {
        ChangeState(EnemyState.walk);
    }
    public virtual void AttackToWalk()
    {
        targetPos = pos1;
        attackTimer = 0f;
        canAttack = true;
        ChangeState(EnemyState.walk);
    }
    public virtual void DestroyEnemyAndPos()
    {
        Instantiate(coinGO, transform.position, transform.rotation);
        Destroy(enemyAndPos);
    }

    internal void ScheduleIldeToWalk(float delay)
    {
        CancelInvoke(nameof(IldeToWalk));
        Invoke(nameof(IldeToWalk), delay);
    }

    internal void CancelIldeToWalk()
    {
        CancelInvoke(nameof(IldeToWalk));
    }

    internal void ScheduleAttackToWalk(float delay)
    {
        Invoke(nameof(AttackToWalk), delay);
    }

    internal void CancelAttackToWalk()
    {
        CancelInvoke(nameof(AttackToWalk));
    }

    internal void ScheduleDestroyEnemyAndPos(float delay)
    {
        Invoke(nameof(DestroyEnemyAndPos), delay);
    }

    internal void DestroyHpSlider()
    {
        Destroy(hpSlider.transform.parent.gameObject);
    }

    private void EnsureStateMachineCreated()
    {
        if (stateMachine != null)
        {
            return;
        }

        attackState = new EnemyAttackState(this);
        stateMachine = new EnemyStateMachine(
            new EnemyIdleState(this),
            new EnemyWalkState(this),
            new EnemyPursuitState(this),
            attackState,
            new EnemyGetHitState(this),
            new EnemyDeadState(this));
    }

    private void SyncStateMirrors()
    {
        if (stateMachine == null || !stateMachine.IsInitialized)
        {
            return;
        }

        stateOld = stateMachine.PreviousState;
        state = stateMachine.CurrentState;
    }
    #endregion
    /// <summary>
    /// 玩家进入警戒范围
    /// </summary>
    /// <param name="player">玩家</param>
    public virtual void PlayerEnterPursuitBox(Player player)
    {
        if (!HasRequiredConfig()) return;

        if (RuntimeState == EnemyState.dead)
        {
            return;
        }
        CancelInvoke(nameof(IldeToWalk));
        targetPos = player.transform;
        ChangeState(EnemyState.pursuit);
    }
    /// <summary>
    /// 玩家退出警戒范围
    /// </summary>
    /// <param name="player">玩家</param>
    public virtual void PlayerExitPursuitBox(Player player)
    {
        if (!HasRequiredConfig()) return;

        EnsureStateMachineCreated();
        if(RuntimeState == EnemyState.attack)
        {
            attackState.ScheduleWalkAfterRemainingCooldown();
        }
        else if(RuntimeState == EnemyState.dead)
        {
            return;
        }
        else
        {
            targetPos = pos1;
            ChangeState(EnemyState.walk);
        }
    }
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">造成的伤害数值</param>
    /// <param name="attackPosition">攻击者的位置</param>
    public virtual void TakeDamage(float damage,Transform attackPosition)
    {
        if (!HasRequiredConfig()) return;

        if (HPNow<=0){return;}

        HPNow -= damage;//受到伤害
        hpSlider.value = HPNow/enemyConfig.MaxHealth;//血条变化
        attackerPos = attackPosition;
        if (HPNow <= 0)
        {
            ChangeState(EnemyState.dead);
        }
        else
        {
            ChangeState(EnemyState.getHit);
        }
    }
    #region 动画事件
    public void Attack1()
    {
        if (!HasRequiredConfig()) return;

        GameObject go;
        if (sr.flipX)//向左
        {
            go = Instantiate(attackPerfab, attack1PosL.position, attack1PosL.rotation);
            go.transform.localScale = attack1PosL.localScale;
        }
        else//向右
        {
            go = Instantiate(attackPerfab, attack1PosR.position, attack1PosR.rotation);
            go.transform.localScale = attack1PosR.localScale;
        }
        go.GetComponent<AttackPerfab>().Init(false, enemyConfig.AttackDamage, transform);//初始化伤害触发器
        GameManger.Instance.PlaySound(2);//播放攻击音效
    }
    #endregion


}
