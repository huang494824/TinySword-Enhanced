using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour
{
    public static GameManger Instance;
    public AudioSource audioS;
    public AudioSource audioBGM;
    public AudioClip[] audioClips;
    public AudioClip[] BGMClips;
    public Transform playerPos;
    public int coinNum = 10;
    public string savePath;
    public bool doLoadGame = false;

    private GameStateModel gameState;

    public GameState CurrentState => gameState.CurrentState;
    public bool CanAcceptPlayerInput => gameState != null && gameState.CanAcceptPlayerInput;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameState = new GameStateModel();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.targetFrameRate = 90;
        savePath = Path.Combine(Application.persistentDataPath, "save.txt");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameState = new GameStateModel();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    public bool TryTransition(GameState expectedState, GameState nextState)
    {
        return gameState != null && gameState.TryTransition(expectedState, nextState);
    }

    public bool EnterDead()
    {
        return gameState != null && gameState.EnterDead();
    }

    void Start()
    {
        
    }

    void Update()
    {
        DoLoad();
    }
    public void PlaySound(int index)
    {
        audioS.PlayOneShot(audioClips[index]);
    }
    public void PlayMusic(int index)
    {
        audioBGM.clip = BGMClips[index];
        audioBGM.Play();
    }
    public void ChangeSoundValue(float value)
    {
        audioS.volume = value;
    }
    public void ChangeBGMValue(float value)
    {
        audioBGM.volume = value;
    }

    public void SaveGame()
    {
        playerPos = Player.Instance.transform;
        string saveData = $"{playerPos.position.x},{playerPos.position.y},{playerPos.position.z},{coinNum}";
        File.WriteAllText(savePath, saveData);
    }

    public void BackStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void loadSaveGame()
    {
        doLoadGame = true;
        SceneManager.LoadScene("GameScene");
    }

    public void ADDCoin(int num)
    {
        coinNum += num;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//在编辑器中运行时才能用
#else
        Application.Quit();//打包之后才能用
#endif
    }

    public void DoLoad()
    {
        if (doLoadGame)
        {
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                doLoadGame = false;
                if (File.Exists(savePath))
                {
                    string saveData = File.ReadAllText(savePath);
                    if (saveData != null)
                    {
                        string[] data = saveData.Split(',');
                        Player.Instance.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
                        coinNum = int.Parse(data[3]);
                    }
                }
            }
        }
    }


}
