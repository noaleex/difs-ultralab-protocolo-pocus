using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loading_manager : MonoBehaviour
{
    public static loading_manager instance { get; private set; }

    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private string currentActiveScene = "MainMenu";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }

    public void SwitchScene(string sceneToLoad)
    {
        StartCoroutine(LoadSceneCoroutine(sceneToLoad));
    }

    private IEnumerator LoadSceneCoroutine(string sceneToLoad)
    {
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.blocksRaycasts = true;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                loadingCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
        }

        if (!string.IsNullOrEmpty(currentActiveScene))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActiveScene);
            while (unloadOp != null && !unloadOp.isDone)
            {
                yield return null;
            }
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        currentActiveScene = sceneToLoad;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));

        if (save_manager.instance != null && save_manager.instance.currentData != null)
        {
            save_manager.instance.currentData.currentScene = sceneToLoad;
            save_manager.instance.SaveGame();
        }

        if (loadingCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                loadingCanvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }
}