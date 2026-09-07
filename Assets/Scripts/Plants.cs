using UnityEngine;

public class Plants : MonoBehaviour
{
    public SpriteRenderer sr;
    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
    }
    /// <summary>
    /// 当玩家进入植物区域时，植物变透明
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (sr != null)
            {
                sr.color = new Color(1, 1, 1, 0.6f); // 50% transparency
            }
        }
    }
    /// <summary>
    /// 当玩家离开植物区域时，植物恢复原样
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (sr != null)
            {
                sr.color = new Color(1, 1, 1, 1); // 100% transparency
            }
        }
    }
}
