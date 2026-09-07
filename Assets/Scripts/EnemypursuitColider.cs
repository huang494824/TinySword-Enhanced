using UnityEngine;

public class EnemypursuitColider : MonoBehaviour
{
    public EnemyBase enemyScript;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            //如果玩家进入追击范围，则调用EnemyBase中的PlayerEnterPursuitBox方法
            enemyScript.PlayerEnterPursuitBox(collision.GetComponent<Player>());
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //如果玩家离开追击范围，则调用EnemyBase中的PlayerExitPursuitBox方法
            enemyScript.PlayerExitPursuitBox(collision.GetComponent<Player>());
        }
    }
}
