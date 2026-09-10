using UnityEngine;

[CreateAssetMenu(menuName = "TinySword/Player Config")]
public class PlayerConfig : ScriptableObject
{
    // 仅作为新建资产的建议值；运行时必须显式绑定配置。
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float guardCooldown = 2f;

    public float MoveSpeed => moveSpeed;
    public float AttackDamage => attackDamage;
    public float MaxHealth => maxHealth;
    public float GuardCooldown => guardCooldown;
}
