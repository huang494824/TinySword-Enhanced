using UnityEngine;

public class NormalCanvas : MonoBehaviour
{
    void Start()
    {
        GameManger.Instance.PlayMusic(1);
    }

    void Update()
    {
        
    }
    public void OpenSettingCanvas()
    {
        if (GameManger.Instance == null ||
            !GameManger.Instance.TryTransition(GameState.Playing, GameState.Settings)) return;

        CanvasManger.Instance.OpenCanvas(true,0);
    }
}
