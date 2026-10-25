using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class UltrasoundManager : MonoBehaviour
{
    public static UltrasoundManager Instance;

    [Header("Imagem da Sonda")]
    public Image probeImage;

    [Header("Tela do Ultrassom")]
    public Image resultImage;

    [Header("FMOD")]
    public EventReference gelSound;

    private EventInstance gelInstance;

    [Header("Imagem Padrão")]
    public Sprite defaultImage;

    [Header("Sprites dos Transdutores")]
    public Sprite setorialProbeSprite;
    public Sprite linearProbeSprite;
    public Sprite convexProbeSprite;

    private RectTransform probeRect;

    [Header("Áreas")]
    public BodyArea heart;
    public BodyArea lung1;
    public BodyArea lung2;
    public BodyArea bladder;
    public BodyArea gelArea;

    [HideInInspector]
    public TransdutorSelected.ProbeType currentProbe =
        TransdutorSelected.ProbeType.None;

    public BodyArea.BodyRegion CurrentRegion { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UltrasoundManager] Instância duplicada.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (probeImage != null)
            probeRect = probeImage.GetComponent<RectTransform>();

        // Detecta quando uma cena começa a ser carregada
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }


    private void Start()
    {
        if (resultImage != null)
            resultImage.sprite = defaultImage;
    }


    public void SelectProbe(TransdutorSelected.ProbeType probe)
    {
        currentProbe = probe;
        if (probeImage == null || probeRect == null)
        {
            Debug.LogWarning("[UltrasoundManager] Imagem/RectTransform da sonda não configurado.", this);
            return;
        }

        switch (probe)
        {
            case TransdutorSelected.ProbeType.Setorial:
                probeImage.sprite = setorialProbeSprite;
                break;

            case TransdutorSelected.ProbeType.Linear:
                probeImage.sprite = linearProbeSprite;
                break;

            case TransdutorSelected.ProbeType.Convex:
                probeImage.sprite = convexProbeSprite;
                break;
        }

        HandleGelSound(true);

        CheckProbePosition(probeRect.position);
    }


    private void HandleGelSound(bool probeOverBody)
    {
        if (gelSound.IsNull || currentProbe == TransdutorSelected.ProbeType.None)
        {
            StopAndReleaseGel();
            return;
        }

        if (probeOverBody)
        {
            if (!gelInstance.isValid())
            {
                Debug.Log("CRIANDO SOM DO GEL");

                gelInstance = RuntimeManager.CreateInstance(gelSound);

                FMOD.RESULT result = gelInstance.start();

                Debug.Log("GEL INICIADO: " + result);
            }
        }
        else
        {
            StopAndReleaseGel();
        }
    }


    public void StopAndReleaseGel()
    {
        if (gelInstance.isValid())
        {
            Debug.Log("PARANDO SOM DO GEL");

            gelInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            gelInstance.release();

            Debug.Log("SOM DO GEL PARADO E LIBERADO");
        }

        gelInstance = default;
    }


    // CHAMADO QUANDO A CENA É DESCARREGADA
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log("CENA DESCARREGADA: " + scene.name);

        StopAndReleaseGel();
    }


    // Segurança adicional
    private void OnDisable()
    {
        StopAndReleaseGel();
    }


    private void OnDestroy()
    {
        StopAndReleaseGel();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        if (Instance == this)
            Instance = null;
    }


    public void CheckProbePosition(Vector2 probePosition)
    {
        bool probeOverBody = gelArea != null && gelArea.ContainsPoint(probePosition);
        HandleGelSound(probeOverBody);

        BodyArea foundArea = null;
        BodyArea.BodyRegion foundRegion = BodyArea.BodyRegion.Empty;

        if (heart != null && heart.ContainsPoint(probePosition))
        {
            foundArea = heart;
            foundRegion = BodyArea.BodyRegion.Heart;
        }
        else if (lung1 != null && lung1.ContainsPoint(probePosition))
        {
            foundArea = lung1;
            foundRegion = BodyArea.BodyRegion.Lung1;
        }
        else if (lung2 != null && lung2.ContainsPoint(probePosition))
        {
            foundArea = lung2;
            foundRegion = BodyArea.BodyRegion.Lung2;
        }
        else if (bladder != null && bladder.ContainsPoint(probePosition))
        {
            foundArea = bladder;
            foundRegion = BodyArea.BodyRegion.Bladder;
        }

        CurrentRegion = foundRegion;
        if (resultImage == null) return;

        if (foundArea == null)
        {
            resultImage.sprite = defaultImage;
            return;
        }

        // Se o paciente possui imagens candidatas configuradas, o requisito do dia
        // prevalece sobre a imagem genérica da área corporal.
        PatientData basePatient = CurrentPatient.Data;
        int day = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
        Sprite requiredImage = PatientDailyState.GetUltrasoundImage(basePatient, day);
        if (requiredImage != null)
        {
            BodyArea.BodyRegion requiredRegion = PatientDailyState.GetUltrasoundRegion(basePatient, day);
            bool rightProbe = IsCorrectProbe(foundRegion);
            resultImage.sprite = foundRegion == requiredRegion && rightProbe
                ? requiredImage
                : defaultImage;
            return;
        }

        resultImage.sprite = IsCorrectProbe(foundRegion) ? foundArea.correctImage : defaultImage;
    }

    private bool IsCorrectProbe(BodyArea.BodyRegion region)
    {
        switch (region)
        {
            case BodyArea.BodyRegion.Heart:
                return currentProbe == TransdutorSelected.ProbeType.Linear;
            case BodyArea.BodyRegion.Lung1:
            case BodyArea.BodyRegion.Lung2:
                return currentProbe == TransdutorSelected.ProbeType.Setorial;
            case BodyArea.BodyRegion.Bladder:
                return currentProbe == TransdutorSelected.ProbeType.Convex;
            default:
                return false;
        }
    }
}