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
    
    [Header("配置")]
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private SkillConfig skill1Config;
    [SerializeField] private SkillConfig skill2Config;
    [SerializeField] private SkillConfig skill3Config;

    public float speed => playerConfig.MoveSpeed;
    [Header("攻击")]
    public Transform attack1Pos;
    public GameObject attackPerfab,skill1Perfab,skill2Perfab, skill3Perfab;
    public Transform skill1LeftPos, skill1RightPos,skill2LeftPos, skill2RightPos,skill3Pos;
    public int skillNum = 1;
    public float skill1CD => skill1Config.Cooldown;
    public float skill2CD => skill2Config.Cooldown;
    public float skill3CD => skill3Config.Cooldown;
    public float skill4CD => playerConfig.GuardCooldown;
    [HideInInspector]public float skill1Time = 0f, skill2Time = 0f, skill3Time = 0f, skill4Time = 0f;
    [HideInInspector]public bool canSkill1 = true, canSkill2 = true, canSkill3 = true, canSkill4 = true;

    public float ATK => playerConfig.AttackDamage;
    public float HPMax => playerConfig.MaxHealth;
    [Header("基础属性")]
    public float HPNow = 100f;

    private Vector2 moveDirection;
    private int attackCombo = 1;
    private bool isAttacking = false;
    private bool isGuard = false;
    private bool isDead = false;
    private bool configurationErrorReported;

    private bool CanAcceptPlayerInput => HasRequiredConfigs() && !isDead && GameManger.Instance != null &&
        GameManger.Instance.CanAcceptPlayerInput;

    private bool HasRequiredConfigs()
    {
        if (playerConfig != null && skill1Config != null && skill2Config != null && skill3Config != null)
            return true;

        if (!configurationErrorReported)
        {
            Debug.LogError("Player requires playerConfig, skill1Config, skill2Config and skill3Config. Bind all four configurations before running.", this);
            configurationErrorReported = true;
        }
        enabled = false;
        return false;
    }

    private void InitializeNestedSkill()
    {
        // 仅兼容当前 Prefab 中唯一的 HolyCross；GameScene 已移除它。
        PlayerSkill[] nestedSkills = GetComponentsInChildren<PlayerSkill>(true);
        if (nestedSkills.Length == 1)
        {
            nestedSkills[0].Init(skill1Config, transform);
        }
        else if (nestedSkills.Length > 1)
        {
            Debug.LogError("Player contains multiple nested PlayerSkill components; cannot safely assign skill1Config.", this);
        }
    }

    private void InitializeSpawnedSkill(GameObject effect, SkillConfig config)
    {
        if (effect.TryGetComponent<PlayerSkill>(out var skill))
        {
            skill.Init(config, transform);
            return;
        }

        Debug.LogError("Spawned skill VFX requires a PlayerSkill component.", effect);
        effect.SetActive(false);
        Destroy(effect);
    }

    private void StopMovement()
    {
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        am.SetBool("IsRun", false);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (!HasRequiredConfigs()) return;

        HPNow = HPMax;
        InitializeNestedSkill();
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
        if (!HasRequiredConfigs()) return;
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
            Invoke(nameof(AttackEnd), skill1Config.ActionRecovery);//攻击结束
            GameManger.Instance.PlaySound(skill1Config.SoundIndex);
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
            Invoke(nameof(AttackEnd), skill2Config.ActionRecovery);//攻击结束
            GameManger.Instance.PlaySound(skill2Config.SoundIndex);
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
            Invoke(nameof(AttackEnd), skill3Config.ActionRecovery);//攻击结束
            GameManger.Instance.PlaySound(skill3Config.SoundIndex);
        }
    }

    #region 动画事件
    public void Attack1()
    {
        if (isDead) return;
        if (!HasRequiredConfigs()) return;

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
        if (!HasRequiredConfigs()) return;

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
        InitializeSpawnedSkill(go, skill1Config);
    }
    public void Skill2()
    {
        if (isDead) return;
        if (!HasRequiredConfigs()) return;

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
        InitializeSpawnedSkill(go, skill2Config);
    }
    public void Skill3()
    {
        if (isDead) return;
        if (!HasRequiredConfigs()) return;

        GameObject go;
        go = Instantiate(skill3Perfab, skill3Pos.position, skill3Pos.rotation);
        go.transform.localScale = skill3Pos.localScale;
        InitializeSpawnedSkill(go, skill3Config);
    }
    #endregion
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">造成的伤害数值</param>
    /// <param name="attackPosition">攻击者的位置</param>
    public void TakeDamage(float damage, Transform attackPosition)
    {
        if (!HasRequiredConfigs()) return;
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



