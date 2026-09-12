using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;
using FMOD.Studio;

public class ImageController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Image targetImage;

    [Header("Brilho")]
    [SerializeField] private Slider ganhoSlider;

    [Header("Zoom")]
    [SerializeField] private Slider realZoomSlider;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 4f;

    [Header("FMOD - Som da Barra")]
    [SerializeField] private EventReference somMovendo;
    [SerializeField] private EventReference somParou;

    [Header("Configuração do som")]
    [SerializeField] private float tempoParaConsiderarParado = 0.15f;

    private RectTransform imageRect;
    private ScrollRect scrollRect;

    private EventInstance somMovendoInstance;

    private bool barraMovendo = false;
    private float ultimoMovimento;

    private bool isInitializing = true;

    private void Start()
    {
        imageRect = targetImage.rectTransform;

        scrollRect = targetImage.GetComponentInParent<ScrollRect>();

        imageRect.pivot = new Vector2(0.5f, 0.5f);

        ganhoSlider.onValueChanged.AddListener(AlterarBrilho);
        realZoomSlider.onValueChanged.AddListener(AlterarZoom);

        AlterarZoom(realZoomSlider.value);
        AlterarBrilho(ganhoSlider.value);

        isInitializing = false;
    }

    private void Update()
    {
        if (isInitializing)
            return;

        if (barraMovendo &&
            Time.unscaledTime - ultimoMovimento >= tempoParaConsiderarParado)
        {
            PararSomMovimento();
        }
    }

    private void AlterarBrilho(float valor)
    {
        targetImage.color = new Color(valor, valor, valor, 1f);

        RegistrarMovimentoDaBarra();
    }

    private void AlterarZoom(float valor)
    {
        float fatorZoom = Mathf.Lerp(minZoom, maxZoom, valor);

        imageRect.localScale = new Vector3(
            fatorZoom,
            fatorZoom,
            1f
        );

        if (scrollRect != null)
        {
            scrollRect.enabled = fatorZoom > minZoom;

            if (fatorZoom <= minZoom)
            {
                imageRect.anchoredPosition = Vector2.zero;
            }
        }

        RegistrarMovimentoDaBarra();
    }

    private void RegistrarMovimentoDaBarra()
    {
        if (isInitializing)
            return;

        ultimoMovimento = Time.unscaledTime;

        if (!barraMovendo)
        {
            IniciarSomMovimento();
        }
    }

    private void IniciarSomMovimento()
    {
        if (somMovendo.IsNull)
            return;

        barraMovendo = true;

        if (!somMovendoInstance.isValid())
        {
            somMovendoInstance = RuntimeManager.CreateInstance(somMovendo);

            somMovendoInstance.start();
        }
    }

    private void PararSomMovimento()
    {
        if (!barraMovendo)
            return;

        barraMovendo = false;

        if (somMovendoInstance.isValid())
        {
            somMovendoInstance.stop(
                FMOD.Studio.STOP_MODE.IMMEDIATE
            );

            somMovendoInstance.release();
        }

        somMovendoInstance = default;

        if (!somParou.IsNull)
        {
            RuntimeManager.PlayOneShot(somParou);
        }
    }

    private void OnDestroy()
    {
        if (somMovendoInstance.isValid())
        {
            somMovendoInstance.stop(
                FMOD.Studio.STOP_MODE.IMMEDIATE
            );

            somMovendoInstance.release();
        }
    }
}