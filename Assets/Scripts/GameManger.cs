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
    public int coinNum = 10;
    public string savePath;

    private int initialCoinNum;
    private SaveData pendingSave;

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
        initialCoinNum = coinNum;
        gameState = new GameStateModel();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.targetFrameRate = 90;
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;
        gameState = new GameStateModel();
        if (pendingSave == null) return;

        if (mode != LoadSceneMode.Single || !TryGetScenePlayer(scene, out Player player))
        {
            pendingSave = null;
            Debug.LogWarning("Continue cancelled: the expected GameScene Player is not ready.", this);
            SceneManager.LoadScene("StartScene");
            return;
        }

        // pending 只保存已完整校验的数据，所有 readiness 检查均在写入之前完成。
        Vector3 position = new Vector3(pendingSave.positionX, pendingSave.positionY, pendingSave.positionZ);
        player.transform.position = position;
        coinNum = pendingSave.coinNum;
        pendingSave = null;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        pendingSave = null;
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
        TrySaveGame();
    }

    public bool TrySaveGame()
    {
        if (Instance != this) return false;
        if (pendingSave != null || !TryGetScenePlayer(SceneManager.GetActiveScene(), out Player player))
        {
            Debug.LogWarning("Cannot save: an active GameScene Player is required.", this);
            return false;
        }

        Vector3 position = player.transform.position;
        var snapshot = new SaveData
        {
            version = SaveJsonCodec.CurrentVersion,
            positionX = position.x,
            positionY = position.y,
            positionZ = position.z,
            coinNum = coinNum
        };
        if (SaveFileStore.TryWrite(savePath, snapshot, out string error)) return true;

        Debug.LogWarning("Save failed: " + error, this);
        return false;
    }

    public void BackStartScene()
    {
        if (Instance != this) return;
        pendingSave = null;
        SceneManager.LoadScene("StartScene");
    }

    public void StartGame()
    {
        if (Instance != this) return;
        pendingSave = null;
        coinNum = initialCoinNum;
        SceneManager.LoadScene("GameScene");
    }

    public void loadSaveGame()
    {
        if (Instance != this) return;
        if (pendingSave != null)
        {
            Debug.LogWarning("Continue is already pending.", this);
            return;
        }
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            Debug.LogWarning("Continue is only available in StartScene.", this);
            return;
        }
        if (!SaveFileStore.TryRead(savePath, out SaveData data, out string error))
        {
            Debug.LogWarning("Continue failed: " + error, this);
            return;
        }

        pendingSave = data;
        try
        {
            SceneManager.LoadScene("GameScene");
        }
        catch (System.Exception exception) when (exception is UnityException || exception is System.ArgumentException)
        {
            pendingSave = null;
            Debug.LogWarning("Continue could not load GameScene: " + exception.Message, this);
        }
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

    private static bool TryGetScenePlayer(Scene scene, out Player player)
    {
        player = Player.Instance;
        return scene.IsValid() && scene.isLoaded && scene.name == "GameScene" &&
            player != null && player.isActiveAndEnabled && player.gameObject.scene == scene;
    }


}
