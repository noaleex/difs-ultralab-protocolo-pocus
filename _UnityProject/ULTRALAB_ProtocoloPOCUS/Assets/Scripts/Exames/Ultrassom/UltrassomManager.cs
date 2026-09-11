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
        Instance = this;

        probeRect = probeImage.GetComponent<RectTransform>();

        // Detecta quando uma cena começa a ser carregada
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }


    private void Start()
    {
        resultImage.sprite = defaultImage;
    }


    public void SelectProbe(TransdutorSelected.ProbeType probe)
    {
        currentProbe = probe;

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
    }


    public void CheckProbePosition(Vector2 probePosition)
    {
        bool probeOverBody = gelArea.ContainsPoint(probePosition);

        HandleGelSound(probeOverBody);


        // CORAÇÃO
        if (heart.ContainsPoint(probePosition))
        {
            CurrentRegion = BodyArea.BodyRegion.Heart;

            if (currentProbe == TransdutorSelected.ProbeType.Linear)
                resultImage.sprite = heart.correctImage;
            else
                resultImage.sprite = defaultImage;

            return;
        }


        // PULMÃO
        if (lung1.ContainsPoint(probePosition) ||
            lung2.ContainsPoint(probePosition))
        {
            CurrentRegion = BodyArea.BodyRegion.Lung1;

            if (currentProbe == TransdutorSelected.ProbeType.Setorial)
                resultImage.sprite = lung1.correctImage;
            else
                resultImage.sprite = defaultImage;

            return;
        }


        // BEXIGA
        if (bladder.ContainsPoint(probePosition))
        {
            CurrentRegion = BodyArea.BodyRegion.Bladder;

            if (currentProbe == TransdutorSelected.ProbeType.Convex)
                resultImage.sprite = bladder.correctImage;
            else
                resultImage.sprite = defaultImage;

            return;
        }


        resultImage.sprite = defaultImage;
    }
}