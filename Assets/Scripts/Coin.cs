using UnityEngine;

public class Coin : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject,0.1f);
            GameManger.Instance.ADDCoin(1);
            GameManger.Instance.PlaySound(5);
        }
    }
}
