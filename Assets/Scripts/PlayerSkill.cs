using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public float destroyTime = 1f;
    public GameObject attackPerfab;
    public Transform player;
    private SkillConfig config;
    private bool initialized;
    private bool initializationErrorReported;

    public void Init(SkillConfig skillConfig, Transform attacker)
    {
        config = skillConfig;
        player = attacker;
        initialized = config != null && player != null;
        if (!initialized) ReportInitializationError();
    }

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
    public void Skill1()
    {
        SpawnAttack();
    }
    public void Skill2()
    {
        SpawnAttack();
    }
    public void Skill3()
    {
        SpawnAttack();
    }

    private void SpawnAttack()
    {
        if (!initialized || config == null || player == null)
        {
            ReportInitializationError();
            return;
        }

        GameObject go = Instantiate(attackPerfab, transform.position, transform.rotation);
        go.transform.localScale = config.AttackScale;
        go.GetComponent<AttackPerfab>().Init(true, config.Damage, player);
    }

    private void ReportInitializationError()
    {
        if (initializationErrorReported) return;
        initializationErrorReported = true;
        Debug.LogError("PlayerSkill requires Init with a valid SkillConfig and attacker before an Animation Event can spawn an attack.", this);
    }
}
