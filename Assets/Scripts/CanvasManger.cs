using UnityEngine;
using UnityEngine.UI;

public class CanvasManger : MonoBehaviour
{
    public static CanvasManger Instance;
    public GameObject[] Canvas;
    public TalkCanvas talkCanvas;
    public Text coinText;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        coinText.text = GameManger.Instance.coinNum.ToString();
    }
    /// <summary>
    /// 打开指定面板
    /// </summary>
    /// <param name="open">true打开，false关闭</param>
    /// <param name="canvasNum">面板编号</param>
    public void OpenCanvas(bool open,int canvasNum)
    {
        Canvas[canvasNum].SetActive(open);
    }
}
