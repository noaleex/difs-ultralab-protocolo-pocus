using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loading_manager : MonoBehaviour
{
    public static loading_manager instance { get; private set; }

    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private string initialSceneName = "MainMenu";
    
    [Header("Configuração de Loading")]
    [Tooltip("Tempo mínimo em segundos que a tela de loading deve ficar ativa.")]
    [SerializeField] private float tempoMinimoLoading = 5.0f;

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
        StartCoroutine(LoadSceneCoroutine(sceneToLoad, true));
    }

    public void SwitchSceneWithoutUpdatingSave(string sceneToLoad)
    {
        StartCoroutine(LoadSceneCoroutine(sceneToLoad, false));
    }

    private IEnumerator LoadSceneCoroutine(string sceneToLoad, bool updateSavedScene)
    {
        // 1. FADE IN (Escurece a tela / Aparece o Loading)
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.blocksRaycasts = true;
            float timerFade = 0f;
            while (timerFade < fadeDuration)
            {
                timerFade += Time.unscaledDeltaTime;
                loadingCanvasGroup.alpha = Mathf.Clamp01(timerFade / fadeDuration);
                yield return null;
            }
            loadingCanvasGroup.alpha = 1f;
        }

        // Inicia o cronômetro para garantir os 5 segundos mínimos
        float tempoDecorrido = 0f;

        // 2. Descarrega a cena antiga
        if (!string.IsNullOrEmpty(currentActiveScene))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(currentActiveScene);
            if (sceneToUnload.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
                while (unloadOp != null && !unloadOp.isDone)
                {
                    tempoDecorrido += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
        }

        // 3. Carrega a nova cena, mas bloqueia a ativação
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        if (loadOp != null)
        {
            // Impede a Unity de mudar para a cena nova imediatamente
            loadOp.allowSceneActivation = false;

            // O progresso da Unity vai até 0.9f enquanto a cena está bloqueada
            while (loadOp.progress < 0.9f)
            {
                tempoDecorrido += Time.unscaledDeltaTime;
                yield return null;
            }

            // 4. Trava o sistema aqui até bater os 5 segundos definidos
            while (tempoDecorrido < tempoMinimoLoading)
            {
                tempoDecorrido += Time.unscaledDeltaTime;
                yield return null;
            }

            // 5. Tempo finalizado. Libera a cena para aparecer
            loadOp.allowSceneActivation = true;
            
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

            if (updateSavedScene && save_manager.instance != null && save_manager.instance.currentData != null)
            {
                save_manager.instance.currentData.currentScene = sceneToLoad;
                save_manager.instance.SaveGame();
            }
        }

        // 6. FADE OUT (Clareia a tela revelando a nova cena)
        if (loadingCanvasGroup != null)
        {
            float timerFadeOut = 0f;
            while (timerFadeOut < fadeDuration)
            {
                timerFadeOut += Time.unscaledDeltaTime;
                loadingCanvasGroup.alpha = 1f - Mathf.Clamp01(timerFadeOut / fadeDuration);
                yield return null;
            }
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }
}