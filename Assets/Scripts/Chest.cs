using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    public Sprite chestOpenTexture;
    public SpriteRenderer sr;
    public GameObject coinGO;
    public bool isOpened = false;
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
            if (!isOpened)
            {
                isOpened = true;
                sr.sprite = chestOpenTexture;
                Instantiate(coinGO, transform.position, transform.rotation);
            }
        }
    }
}
