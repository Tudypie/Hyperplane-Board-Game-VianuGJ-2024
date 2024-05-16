using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadSceneByIndex(int index) => SceneManager.LoadScene(index);

    public void LoadGameScene() => SceneManager.LoadScene(1);

    public void LoadLoseScene() => SceneManager.LoadScene(2);

    public void LoadWinScene() => SceneManager.LoadScene(3);

}
