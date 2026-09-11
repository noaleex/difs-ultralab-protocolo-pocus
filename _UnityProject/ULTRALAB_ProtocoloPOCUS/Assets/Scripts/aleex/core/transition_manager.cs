using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class transition_manager : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Configurações de Animação")]
    [SerializeField] private float fadeDurantion = 1.5f;
    [SerializeField] private float typingSpeed = 0.08f;
    [SerializeField] private float readDelay = 2.5f;

    private CanvasGroup canvasGroup;
    private string fullTextToType;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        fullTextToType = timeText.text;
        timeText.text = "";

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void PlayTimeSkip ()
    {
        StopAllCoroutines();
        StartCoroutine(TimeSkipSequence());
    }

    private IEnumerator TimeSkipSequence()
    {
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDurantion));
        timeText.text = "";
        foreach (char letter in fullTextToType)
        {
            timeText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(readDelay);
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDurantion));

        timeText.text = "";
    }

    private IEnumerator FadeCanvasGroup (float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}
