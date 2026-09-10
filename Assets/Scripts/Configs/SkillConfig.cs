using UnityEngine;

[CreateAssetMenu(menuName = "TinySword/Skill Config")]
public class SkillConfig : ScriptableObject
{
    // 仅作为新建资产的建议值；各技能资产需分别设置。
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private Vector3 attackScale = new Vector3(1.5f, 1.5f, 1.5f);
    [SerializeField] private float actionRecovery = 0.5f;
    [SerializeField] private int soundIndex = 3;

    public float Cooldown => cooldown;
    public float Damage => damage;
    public Vector3 AttackScale => attackScale;
    public float ActionRecovery => actionRecovery;
    public int SoundIndex => soundIndex;
}
