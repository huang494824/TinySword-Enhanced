using UnityEngine;

public class AttackPerfab : MonoBehaviour
{
    public float destroyTime = 0.3f;
    public bool isPlayerAttack = true;
    public float attackDamage = 10f;
    public Transform attackPos;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="isPlayer">是否为玩家攻击</param>
    /// <<param name="damage">伤害数值</param>
    public void Init(bool isPlayer,float damage,Transform attackPosition)
    {
        isPlayerAttack = isPlayer;
        attackDamage = damage;
        attackPos = attackPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPlayerAttack)
        {
            if(collision.tag == "Enemy")
            {
                collision.GetComponent<EnemyBase>().TakeDamage(attackDamage, attackPos);
            }
        }
        else
        {
            if (collision.tag == "Player")
            {
                collision.GetComponent<Player>().TakeDamage(attackDamage, attackPos);
            }
        }
    }
}
