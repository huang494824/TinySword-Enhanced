using UnityEngine;
using UnityEngine.UI;

public enum EnemyState { idle, walk , pursuit, attack, getHit, dead }

public class EnemyBase : MonoBehaviour
{
    public float speed = 1f;

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
    public float AttackDis = 1f;
    public float AttackCoolTime = 1f;
    private float attackTimer = 0f;
    public bool canAttack = true;
    private float getHitTimer = 0f;
    public Transform attackerPos;
    public GameObject enemyAndPos;
    public GameObject attackPerfab;
    public Transform attack1PosL;
    public Transform attack1PosR;

    [Header("基础属性")]
    public float ATK = 10f;
    public float HPMax = 100f;
    public float HPNow = 100f;

    public virtual void Start()
    {
        ChangeState(EnemyState.walk);
        targetPos = pos1;
        HPNow = HPMax;
    }

    public virtual void Update()
    {
        if(state == EnemyState.idle){ idleUpdate(); }
        else if(state == EnemyState.walk){ walkUpdate(); }
        else if(state == EnemyState.pursuit){ pursuitUpdate(); }
        else if(state == EnemyState.attack){ attackUpdate(); }
        else if(state == EnemyState.getHit){ getHitUpdate(); }
        else if(state == EnemyState.dead){ deadUpdate(); }
    }

    public virtual void FixedUpdate()
    {

    }

    #region 状态机
    /// <summary>
    /// 切换为新状态
    /// </summary>
    /// <param name="newState">新状态</param>
    public virtual void ChangeState(EnemyState newState)
    {
        if (state == EnemyState.idle) { idleExit(); }
        else if (state == EnemyState.walk) { walkExit(); }
        else if (state == EnemyState.pursuit) { pursuitExit(); }
        else if (state == EnemyState.attack) { attackExit(); }
        else if (state == EnemyState.getHit) { getHitExit(); }
        else if (state == EnemyState.dead) { deadExit(); }
        stateOld = state;
        state = newState;
        if (state == EnemyState.idle) { idleEnter(); }
        else if (state == EnemyState.walk) { walkEnter(); }
        else if (state == EnemyState.pursuit) { pursuitEnter(); }
        else if (state == EnemyState.attack) { attackEnter(); }
        else if (state == EnemyState.getHit) { getHitEnter(); }
        else if (state == EnemyState.dead) { deadEnter(); }
    }
    public virtual void idleEnter()
    {
        am.SetBool("IsRun", false);
        if(stateOld == EnemyState.walk)
        {
            CancelInvoke(nameof(IldeToWalk));
            Invoke(nameof(IldeToWalk), 2f);
        }
    }
    public virtual void idleUpdate()
    {
        rb.linearVelocity = Vector2.zero;
    }
    public virtual void idleExit()
    {
        CancelInvoke(nameof(IldeToWalk));
    }
    public virtual void walkEnter()
    {
        am.SetBool("IsRun", true);
    }
    public virtual void walkUpdate()
    {
        rb.linearVelocity = (targetPos.position - transform.position).normalized * speed;
        sr.flipX = rb.linearVelocity.x < 0;

        if(Vector2.Distance(transform.position,pos1.position) < 0.1f)//角色在pos1位置
        {
            if(targetPos == pos1)//角色目标点是pos1
            {
                targetPos = pos2;//角色目标点变为pos2
                ChangeState(EnemyState.idle);//角色状态变为idle
            }
        }
        else if(Vector2.Distance(transform.position, pos2.position) < 0.1f)
        {
            if (targetPos == pos2)
            {
                targetPos = pos1;
                ChangeState(EnemyState.idle);
            }
        }
    }
    public virtual void walkExit()
    {

    }
    public virtual void pursuitEnter()
    {
        am.SetBool("IsRun", true);
    }
    public virtual void pursuitUpdate()
    {
        rb.linearVelocity = (targetPos.position - transform.position).normalized * speed;
        sr.flipX = rb.linearVelocity.x < 0;
        if(targetPos ==pos1|| targetPos == pos2)
        {
            ChangeState(EnemyState.walk);
        }
        else if(Vector2.Distance(transform.position, targetPos.position) < AttackDis)
        {
            if(sr.flipX && (targetPos.position.x - transform.position.x) < 0 )
            {
                ChangeState(EnemyState.attack);
            }
            else if(!sr.flipX && (targetPos.position.x - transform.position.x) > 0)
            {
                ChangeState(EnemyState.attack);
            }
        }
    }
    public virtual void pursuitExit()
    {
        
    }
    public virtual void attackEnter()
    {
        am.SetBool("IsRun", false);
    }
    public virtual void attackUpdate()
    {
        rb.linearVelocity = Vector2.zero;

        if (canAttack)
        {
            if (Vector2.Distance(transform.position, targetPos.position) > AttackDis)//在攻击距离外
            {
                ChangeState(EnemyState.pursuit);
            }
            else if (sr.flipX && (targetPos.position.x - transform.position.x) > 0)//在攻击距离内但在反方向
            {
                ChangeState(EnemyState.pursuit);
            }
            else if (!sr.flipX && (targetPos.position.x - transform.position.x) < 0)//在攻击距离内但在反方向
            {
                ChangeState(EnemyState.pursuit);
            }
            else//在攻击距离内且方向正确
            {
                am.SetTrigger("Attack1");
                
                canAttack = false;
            }
        }

        if(attackTimer < AttackCoolTime)
        {
            attackTimer += Time.deltaTime;
        }
        else
        {
            attackTimer = 0f;
            canAttack = true;
        }
    }
    public virtual void attackExit()
    {
        CancelInvoke(nameof(AttackToWalk));
    }
    public virtual void getHitEnter()
    {
        am.SetTrigger("GetHit");
        am.SetBool("IsRun", false);
        rb.linearVelocity = Vector2.zero;
        getHitTimer = 0f;
    }
    public virtual void getHitUpdate()
    {
        getHitTimer += Time.deltaTime;
        //受到攻击后后退
        if(getHitTimer <= 0.2f){rb.linearVelocity = (transform.position - attackerPos.position).normalized * 2f; }
        else{rb.linearVelocity = Vector2.zero;}
        //受到攻击后一段时间后回到pursuit状态
        if(getHitTimer >= 1f)
        {   
            getHitTimer = 0f;
            ChangeState(EnemyState.pursuit);
        }
    }
    public virtual void getHitExit()
    {

    }
    public virtual void deadEnter()
    {
        am.SetBool("IsRun", false);
        am.SetTrigger("Dead");
        Invoke(nameof(DestroyEnemyAndPos), 1f);
        Destroy(hpSlider.transform.parent.gameObject);//销毁怪物身上的UI
    }
    public virtual void deadUpdate()
    {
        rb.linearVelocity = Vector2.zero;
    }
    public virtual void deadExit()
    {

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
    #endregion
    /// <summary>
    /// 玩家进入警戒范围
    /// </summary>
    /// <param name="player">玩家</param>
    public virtual void PlayerEnterPursuitBox(Player player)
    {
        if (state == EnemyState.dead)
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
        if(state == EnemyState.attack)
        {
            Invoke(nameof(AttackToWalk), AttackCoolTime - attackTimer);
        }
        else if(state == EnemyState.dead)
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
        if (HPNow<=0){return;}

        HPNow -= damage;//受到伤害
        hpSlider.value = HPNow/HPMax;//血条变化
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
        go.GetComponent<AttackPerfab>().Init(false, ATK, transform);//初始化伤害触发器
        GameManger.Instance.PlaySound(2);//播放攻击音效
    }
    #endregion


}
