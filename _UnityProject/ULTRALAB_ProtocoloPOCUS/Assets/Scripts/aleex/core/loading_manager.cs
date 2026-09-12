using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loading_manager : MonoBehaviour
{
    public static loading_manager instance { get; private set; }

    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private string initialSceneName = "MainMenu";

    private string currentActiveScene = "";

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

        currentActiveScene = initialSceneName;
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
                timer += Time.unscaledDeltaTime; // <-- ALTERADO AQUI
                loadingCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            loadingCanvasGroup.alpha = 1f;
        }

        if (!string.IsNullOrEmpty(currentActiveScene))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(currentActiveScene);
            if (sceneToUnload.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
                while (unloadOp != null && !unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        if (loadOp != null)
        {
            while (!loadOp.isDone)
            {
                yield return null;
            }

            currentActiveScene = sceneToLoad;
            Scene newScene = SceneManager.GetSceneByName(sceneToLoad);
            if (newScene.IsValid())
            {
                SceneManager.SetActiveScene(newScene);
            }

            if (save_manager.instance != null && save_manager.instance.currentData != null)
            {
                save_manager.instance.currentData.currentScene = sceneToLoad;
                save_manager.instance.SaveGame();
            }
        }

        if (loadingCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime; // <-- ALTERADO AQUI TAMBÉM
                loadingCanvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }
}