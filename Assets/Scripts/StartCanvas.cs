using UnityEngine;

public class StartCanvas : MonoBehaviour
{
    void Start()
    {
        GameManger.Instance.PlayMusic(0);
    }

    void Update()
    {
        
    }
    public void OnStartGameButtonClick()
    {
        GameManger.Instance.StartGame();
    }
    public void OnLoadSaveButtonClick()
    {

        GameManger.Instance.loadSaveGame();
    }
    public void OnExitGameButtonClick()
    {
        GameManger.Instance.ExitGame();
    }
}
