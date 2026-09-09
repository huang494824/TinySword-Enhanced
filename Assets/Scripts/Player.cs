using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    [Header("组件")]
    public Rigidbody2D rb;
    public Animator am;
    public SpriteRenderer sr;
    public Slider hpSlider;
    public Text hpText;
    public GameObject deadUI;
    public static Player Instance;
    
    [Header("移动")]
    public float speed = 1.0f;
    [Header("攻击")]
    public Transform attack1Pos;
    public GameObject attackPerfab,skill1Perfab,skill2Perfab, skill3Perfab;
    public Transform skill1LeftPos, skill1RightPos,skill2LeftPos, skill2RightPos,skill3Pos;
    public int skillNum = 1;
    public float skill1CD = 3f, skill2CD = 4f, skill3CD = 10f,skill4CD = 2f;
    [HideInInspector]public float skill1Time = 0f, skill2Time = 0f, skill3Time = 0f, skill4Time = 0f;
    [HideInInspector]public bool canSkill1 = true, canSkill2 = true, canSkill3 = true, canSkill4 = true;

    [Header("基础属性")]
    public float ATK = 10f;
    public float HPMax = 100f;
    public float HPNow = 100f;

    private Vector2 moveDirection;
    private int attackCombo = 1;
    private bool isAttacking = false;
    private bool isGuard = false;
    private bool isDead = false;

    private bool CanAcceptPlayerInput => !isDead && GameManger.Instance != null &&
        GameManger.Instance.CanAcceptPlayerInput;

    private void StopMovement()
    {
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        am.SetBool("IsRun", false);
    }

    void Awake()
    {
        HPNow = HPMax;
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    void Update()
    {
        PlayerWindowsInput();
        SkillTimers();
    }

    private void FixedUpdate()
    {
        if(!isDead)
        {
            PlayerMove();
        }
        else
        {
            StopMovement();
        }
    }
    ///<summary>
    ///输入
    ///</summary>
    public void PlayerWindowsInput()
    {
        if (isDead)
        {
            StopMovement();
            return;
        }
        if (!CanAcceptPlayerInput)
        {
            StopMovement();
            return;
        }

        //移动输入
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        moveDirection.y = Input.GetAxisRaw("Vertical");
        //攻击输入
        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayerAttack();
        }
        //防御输入
        else if (Input.GetKeyDown(KeyCode.K))
        {
            PlayerGuard();
        }
        //技能输入
        else if(Input.GetKeyDown(KeyCode.E))
        {
            PlayerSkill1();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayerSkill2();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerSkill3();
        }
    }
    /// <summary>
    /// 移动
    /// </summary>
    public void PlayerMove()
    {
        if (!CanAcceptPlayerInput)
        {
            StopMovement();
            return;
        }

        if (!isAttacking && !isGuard)
        {
            rb.linearVelocity = moveDirection * speed;//移动
            am.SetBool("IsRun", moveDirection.magnitude > 0.1f);//动画
            if (moveDirection.magnitude > 0.1f)
            {
                sr.flipX = (moveDirection.x < -0.1f);//翻转 
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;//攻击时停止
        }
    }
    /// <summary>
    /// 攻击动画
    /// </summary>
    public void PlayerAttack()
    {
        if (isDead) return;
        if (!CanAcceptPlayerInput) return;

        if (!isAttacking && !isGuard)//如果不在攻击状态
        {
            isAttacking = true;
            if (attackCombo == 1)
            {
                am.SetTrigger("Attack1");
                attackCombo = 2;
            }
            else
            {
                am.SetTrigger("Attack2");
                attackCombo = 1;
            }
            Invoke(nameof(AttackEnd), 0.35f);//攻击结束
        }
    }
    /// <summary>
    /// 攻击结束
    /// </summary>
    public void AttackEnd()
    {
        isAttacking = false;
    }
    /// <summary>
    /// 进入守卫状态
    /// </summary>
    public void PlayerGuard()
    {
        if (!CanAcceptPlayerInput) return;

        if (!isDead)
        {
            if(isGuard)
            {
                EndGuard();
            }
            else
            {
                if (canSkill4)
                {
                    am.SetBool("IsGuard", true);
                    isGuard = true;
                    canSkill4 = false;
                    skill4Time = 0;
                }
            }
        }
    }
    private void EndGuard()
    {
        am.SetBool("IsGuard", false);
        isGuard = false;
    }

    /// <summary>
    /// 技能计时器
    /// </summary>
    public void SkillTimers()
    {
        if(!canSkill1)
        {
            if(skill1Time< skill1CD)
            {
                skill1Time += Time.deltaTime;
            }
            else
            {
                canSkill1 = true;
                skill1Time = skill1CD;
            }
        }
        if(!canSkill2)
        {
            if (skill2Time < skill2CD)
            {
                skill2Time += Time.deltaTime;
            }
            else
            {
                canSkill2 = true;
                skill2Time = skill2CD;
            }
        }
        if(!canSkill3)
        {
            if (skill3Time < skill3CD)
            {
                skill3Time += Time.deltaTime;
            }
            else
            {
                canSkill3 = true;
                skill3Time = skill3CD;
            }
        }
        if(!canSkill4)
        {
            if (skill4Time < skill4CD)
            {
                skill4Time += Time.deltaTime;
            }
            else
            {
                canSkill4 = true;
                skill4Time = skill4CD;
                if(isGuard && !isDead){ EndGuard();}
            }
        }
    }
    /// <summary>
    /// 技能1
    /// </summary>
    public void PlayerSkill1()
    {
        if (isDead) return;
        if (!CanAcceptPlayerInput) return;

        if (!isAttacking && !isGuard && canSkill1)
        {
            isAttacking = true;
            canSkill1 = false;
            skill1Time = 0;
            am.SetTrigger("Skill");
            skillNum = 1;
            Invoke(nameof(AttackEnd), 0.5f);//攻击结束
            GameManger.Instance.PlaySound(3);
        }
    }
    /// <summary>
    /// 技能2
    /// </summary>
    public void PlayerSkill2()
    {
        if (isDead) return;
        if (!CanAcceptPlayerInput) return;

        if (!isAttacking && !isGuard && canSkill2)
        {
            isAttacking = true;
            canSkill2 = false;
            skill2Time = 0;
            am.SetTrigger("Skill");
            skillNum = 2;
            Invoke(nameof(AttackEnd), 0.5f);//攻击结束
            GameManger.Instance.PlaySound(3);
        }
    }
    /// <summary>
    /// 技能3
    /// </summary>
    public void PlayerSkill3()
    {
        if (isDead) return;
        if (!CanAcceptPlayerInput) return;

        if (!isAttacking && !isGuard && canSkill3)
        {
            isAttacking = true;
            canSkill3 = false;
            skill3Time = 0;
            am.SetTrigger("Skill");
            skillNum = 3;
            Invoke(nameof(AttackEnd), 0.5f);//攻击结束
            GameManger.Instance.PlaySound(4);
        }
    }

    #region 动画事件
    public void Attack1()
    {
        if (isDead) return;

        GameObject go = Instantiate(attackPerfab, attack1Pos.position, attack1Pos.rotation);
        go.transform.localScale = attack1Pos.localScale;
        go.GetComponent<AttackPerfab>().Init(true, ATK, transform);
        GameManger.Instance.PlaySound(0);
    }
    public void Skill()
    {
        if (isDead) return;

        if (skillNum == 1){Skill1();}
        else if (skillNum == 2) { Skill2(); }
        else if (skillNum == 3) { Skill3(); }
    }
    public void Skill1()
    {
        if (isDead) return;

        GameObject go;
        if (sr.flipX)//向左
        {
            go = Instantiate(skill1Perfab,skill1LeftPos.position, skill1LeftPos.rotation);
            go.transform.localScale = skill1LeftPos.localScale;
        }
        else//向右
        {
            go = Instantiate(skill1Perfab, skill1RightPos.position, skill1RightPos.rotation);
            go.transform.localScale = skill1RightPos.localScale;
        }
    }
    public void Skill2()
    {
        if (isDead) return;

        GameObject go;
        if (sr.flipX)//向左
        {
            go = Instantiate(skill2Perfab, skill2LeftPos.position, skill2LeftPos.rotation);
            go.transform.localScale = skill2LeftPos.localScale;
        }
        else//向右
        {
            go = Instantiate(skill2Perfab, skill2RightPos.position, skill2RightPos.rotation);
            go.transform.localScale = skill2RightPos.localScale;
        }
    }
    public void Skill3()
    {
        if (isDead) return;

        GameObject go;
        go = Instantiate(skill3Perfab, skill3Pos.position, skill3Pos.rotation);
        go.transform.localScale = skill3Pos.localScale;
    }
    #endregion
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">造成的伤害数值</param>
    /// <param name="attackPosition">攻击者的位置</param>
    public void TakeDamage(float damage, Transform attackPosition)
    {
        if (HPNow <= 0) {return;}
        if (isGuard) 
        {
            if (transform.position.x < attackPosition.position.x && !sr.flipX)
            {
                GameManger.Instance.PlaySound(1);
                return; }
            else if (transform.position.x > attackPosition.position.x && sr.flipX)
            {
                GameManger.Instance.PlaySound(1);//播放防御音效
                return; }
        }

        HPNow -= damage;//受到伤害
        hpSlider.value = HPNow / HPMax;//血条
        hpText.text = HPNow.ToString("f0") + " / " + HPMax.ToString("f0");//血量显示
        if (HPNow <= 0) 
        {
            PlayerDead();
        }
        else
        {
            am.SetTrigger("GetHit");
            am.SetBool("IsRun", false);
            rb.linearVelocity = Vector2.zero;
        }
    }
    public void PlayerDead()
    {
        if(!isDead)
        {
            isDead = true;
            if (GameManger.Instance != null)
            {
                GameManger.Instance.EnterDead();
            }

            am.SetTrigger("Dead");
            StopMovement();
            deadUI.SetActive(true);
        }
    }
}



