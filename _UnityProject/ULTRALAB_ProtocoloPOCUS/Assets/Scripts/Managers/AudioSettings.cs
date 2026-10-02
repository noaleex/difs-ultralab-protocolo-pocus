using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class AudioSettings : MonoBehaviour
{
    [Header("Sliders")]
    public Slider volumeGeral;
    public Slider volumeMusica;
    public Slider volumeSFX;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    private bool alterandoSlider = false;

    void Start()
    {
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        volumeGeral.value = PlayerPrefs.GetFloat("VolumeGeral", 1f);
        volumeMusica.value = volumeGeral.value;
        volumeSFX.value = volumeGeral.value;

        volumeGeral.onValueChanged.AddListener(AlterarVolumeGeral);
        volumeMusica.onValueChanged.AddListener(AlterarVolumeMusica);
        volumeSFX.onValueChanged.AddListener(AlterarVolumeSFX);

        AplicarVolumes();
    }

    // VOLUME GERAL
    public void AlterarVolumeGeral(float valor)
    {
        if (alterandoSlider)
            return;

        alterandoSlider = true;

        // O volume geral define o volume dos dois
        volumeMusica.value = valor;
        volumeSFX.value = valor;

        alterandoSlider = false;

        AplicarVolumes();

        PlayerPrefs.SetFloat("VolumeGeral", valor);
        PlayerPrefs.Save();
    }

    // MÚSICA
    public void AlterarVolumeMusica(float valor)
    {
        if (alterandoSlider)
            return;

        musicBus.setVolume(valor);

        PlayerPrefs.SetFloat("VolumeMusica", valor);
        PlayerPrefs.Save();
    }

    // EFEITOS
    public void AlterarVolumeSFX(float valor)
    {
        if (alterandoSlider)
            return;

        sfxBus.setVolume(valor);

        PlayerPrefs.SetFloat("VolumeSFX", valor);
        PlayerPrefs.Save();
    }

    void AplicarVolumes()
    {
        float geral = volumeGeral.value;

        masterBus.setVolume(geral);
        musicBus.setVolume(geral);
        sfxBus.setVolume(geral);
    }
}