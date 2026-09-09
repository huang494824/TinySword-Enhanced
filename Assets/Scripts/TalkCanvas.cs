using UnityEngine;
using UnityEngine.UI;

public class TalkCanvas : MonoBehaviour
{
    public Text nameText;
    public Text talkText;
    public GameObject NPCImage, playerImage;
    public string[] talkContent;
    public int talkIndex;


    void Start()
    {
        talkIndex = 0;
        
        NPCImage.SetActive(false);
        playerImage.SetActive(false);
    }

    public void DoTalk(string talkName,string talkContent)
    {
        gameObject.SetActive(true);
        NPCImage.SetActive(false);
        playerImage.SetActive(false);
        nameText.text = talkName;
        talkText.text = talkContent;
        if (talkName == "村民")
        {
            NPCImage.SetActive(true);
        }
        else if(talkName == "勇者")
        {
            playerImage.SetActive(true);
        }
    }

    public void TalkWithNPC1()
    {
        if (GameManger.Instance == null ||
            !GameManger.Instance.TryTransition(GameState.Playing, GameState.Dialogue)) return;

        talkIndex = 0;
        DoTalk("村民", talkContent[talkIndex]);
    }

    public void EndTalkWithNPC1()
    {
        talkIndex = 0;
        gameObject.SetActive(false);
        NPCImage.SetActive(false);
        playerImage.SetActive(false);
        if (GameManger.Instance != null)
        {
            GameManger.Instance.TryTransition(GameState.Dialogue, GameState.Playing);
        }
    }

    public void OnTalkButtonClick()
    {
        if (GameManger.Instance == null ||
            GameManger.Instance.CurrentState != GameState.Dialogue) return;

        talkIndex += 1;
        if (talkIndex < talkContent.Length)
        {
            if(talkIndex%2 == 1)
            {
                DoTalk("勇者", talkContent[talkIndex]);
            }
            else
            {
                DoTalk("村民", talkContent[talkIndex]);
            }
        }
        else
        {
            EndTalkWithNPC1();
        }
    }

}
