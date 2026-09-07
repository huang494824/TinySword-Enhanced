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
        CanvasManger.Instance.OpenCanvas(true,0);
    }
}
