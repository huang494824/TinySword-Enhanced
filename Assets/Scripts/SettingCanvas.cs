using UnityEngine;
using UnityEngine.UI;

public class SettingCanvas : MonoBehaviour
{
    public Slider soundSlider;
    public Slider musicSlider;
    public GameObject suText;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    //游戏物体启用
    private void OnEnable()
    {
        if (GameManger.Instance != null)
        {
            soundSlider.value = GameManger.Instance.audioS.volume;
            musicSlider.value = GameManger.Instance.audioBGM.volume;
        }
    }
    //游戏物体禁用
    private void OnDisable()
    {
        
    }

    public void Close()
    {
        CanvasManger.Instance.OpenCanvas(false,0);
    }


    public void OnMusicSliderChange()
    {
        GameManger.Instance.audioBGM.volume = musicSlider.value;
    }

    public void OnSoundSliderChange()
    {
        GameManger.Instance.audioS.volume = soundSlider.value;
    }

    public void OnBackStartClick()
    {
        GameManger.Instance.BackStartScene();
    }

    public void OnSaveGameClick()
    {
        GameManger.Instance.SaveGame();
        suText.SetActive(true);
        Invoke(nameof(HideSuText), 1f);
    }
    public void HideSuText()
    {
        suText.SetActive(false);
    }
}
