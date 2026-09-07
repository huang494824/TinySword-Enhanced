using UnityEngine;

public class NPCUnit : MonoBehaviour
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
            CanvasManger.Instance.talkCanvas.TalkWithNPC1();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CanvasManger.Instance.talkCanvas.EndTalkWithNPC1();
        }
    }
}
