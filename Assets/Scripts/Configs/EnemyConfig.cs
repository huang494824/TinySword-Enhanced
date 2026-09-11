using UnityEngine;

[CreateAssetMenu(menuName = "TinySword/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    // 仅作为新建资产的建议值；运行时必须显式绑定配置。
    [SerializeField] private float moveSpeed = 0.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float attackCooldown = 1f;

    public float MoveSpeed => moveSpeed;
    public float AttackDamage => attackDamage;
    public float MaxHealth => maxHealth;
    public float AttackDistance => attackDistance;
    public float AttackCooldown => attackCooldown;
}
