using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadUI : MonoBehaviour
{

    public void Restart()
    {
        SceneManager.LoadScene("GameScene");
    }
}
