using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.EventSystems;

public class MedicalData : MonoBehaviour
{
    [Header("Referência de Dados")]
    public PatientData patientData;

    // =====================================================
    // VIA AÉREA
    // =====================================================

    [Header("Via Aérea")]
    public TMP_Dropdown permeabilidadeDropdown;
    public TMP_Dropdown presencaDropdown;
    public TMP_Dropdown intervencaoDropdown;

    // =====================================================
    // RESPIRAÇÃO
    // =====================================================

    [Header("Respiração")]
    public TMP_InputField frequenciaInput;
    public TMP_InputField saturationInput;

    public TMP_Dropdown acessoriaDropdown;
    public TMP_Dropdown padraoRespiratorioDropdown;
    public TMP_Dropdown ascultaDropdown;
    public TMP_Dropdown expansibilidadeDropdown;
    public TMP_Dropdown oxigenoterapiaTipoDropdown;

    public TMP_InputField oxigenoterapiaFluxoInput;

    // =====================================================
    // CIRCULAÇÃO
    // =====================================================

    [Header("Circulação")]
    public TMP_InputField frequenciaCardiacaInput;
    public TMP_InputField pressaoArterialInput;
    public TMP_InputField pressaoArterial2Input;
    public TMP_InputField perfusaoTempoInput;

    public TMP_Dropdown perfusaoExtremidadesDropdown;
    public TMP_Dropdown pulsosDropdown;
    public TMP_Dropdown ritmoCardiacoDropdown;
    public TMP_Dropdown edemaDropdown;

    public TMP_InputField temperaturaInput;

    // =====================================================
    // AVALIAÇÃO NEUROLÓGICA
    // =====================================================

    [Header("Avaliação Neurológica")]
    public TMP_Dropdown nivelConscienciaDropdown;

    // =====================================================
    // EXPOSIÇÃO
    // =====================================================

    [Header("Exposição")]
    public TMP_Dropdown avalicaoPeleDropdown;
    public TMP_Dropdown presencaPeleDropdown;

    public TMP_InputField dorEscalaInput;

    // =====================================================
    // FMOD
    // =====================================================

    [Header("FMOD - Sons")]
    [SerializeField] private EventReference somSelecao;
    [SerializeField] private EventReference somDigitacao;

    private string ultimoValorDor = "";

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {

        SetupNumericInput(frequenciaInput);
        SetupNumericInput(saturationInput);
        SetupNumericInput(oxigenoterapiaFluxoInput);

        SetupNumericInput(frequenciaCardiacaInput);
        SetupNumericInput(pressaoArterialInput);
        SetupNumericInput(pressaoArterial2Input);
        SetupNumericInput(perfusaoTempoInput);

        SetupNumericInput2(temperaturaInput);
        SetupNumericInput2(dorEscalaInput);

        // Som de digitação - Respiração
AdicionarSomDigitacao(frequenciaInput);
AdicionarSomDigitacao(saturationInput);
AdicionarSomDigitacao(oxigenoterapiaFluxoInput);

// Som de digitação - Circulação
AdicionarSomDigitacao(frequenciaCardiacaInput);
AdicionarSomDigitacao(pressaoArterialInput);
AdicionarSomDigitacao(pressaoArterial2Input);
AdicionarSomDigitacao(perfusaoTempoInput);
AdicionarSomDigitacao(temperaturaInput);

// Som de digitação - Exposição
AdicionarSomDigitacao(dorEscalaInput);

        AdicionarSomDropdowns();

        if (dorEscalaInput != null)
        {
            dorEscalaInput.onValueChanged.AddListener(
                OnDorEscalaChanged
            );
        }
    }

    // Som digitação
    private void OnDorEscalaChanged(string valor)
    {
        if (valor.Length > ultimoValorDor.Length)
        {
            TocarSomDigitacao();
        }

        ultimoValorDor = valor;
    }

private void TocarSomDigitacao()
{
    if (!somDigitacao.IsNull)
    {
        RuntimeManager.PlayOneShot(somDigitacao);
    }
}

    private void AdicionarSomDigitacao(TMP_InputField input)
{
    if (input == null)
        return;

    input.onValueChanged.AddListener((valor) =>
    {
        // Toca o som quando um caractere é digitado
        if (valor.Length > 0)
        {
            TocarSomDigitacao();
        }
    });
}

    //Som caixinhas
    private void AdicionarSomDropdowns()
    {
        AdicionarSomDropdown(permeabilidadeDropdown);
        AdicionarSomDropdown(presencaDropdown);
        AdicionarSomDropdown(intervencaoDropdown);

        AdicionarSomDropdown(acessoriaDropdown);
        AdicionarSomDropdown(padraoRespiratorioDropdown);
        AdicionarSomDropdown(ascultaDropdown);
        AdicionarSomDropdown(expansibilidadeDropdown);
        AdicionarSomDropdown(oxigenoterapiaTipoDropdown);

        AdicionarSomDropdown(perfusaoExtremidadesDropdown);
        AdicionarSomDropdown(pulsosDropdown);
        AdicionarSomDropdown(ritmoCardiacoDropdown);
        AdicionarSomDropdown(edemaDropdown);

        AdicionarSomDropdown(nivelConscienciaDropdown);

        AdicionarSomDropdown(avalicaoPeleDropdown);
        AdicionarSomDropdown(presencaPeleDropdown);
    }

    private void AdicionarSomDropdown(
        TMP_Dropdown dropdown)
    {
        if (dropdown == null)
            return;

        dropdown.onValueChanged.AddListener(
            _ => TocarSomSelecao()
        );

        AdicionarEventTriggerDropdown(dropdown);
    }

    private void AdicionarEventTriggerDropdown(
        TMP_Dropdown dropdown)
    {
        if (dropdown == null)
            return;

        EventTrigger trigger =
            dropdown.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger =
                dropdown.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry =
            new EventTrigger.Entry();

        entry.eventID =
            EventTriggerType.PointerClick;

        entry.callback.AddListener(
            (data) => TocarSomSelecao()
        );

        trigger.triggers.Add(entry);
    }

    private void TocarSomSelecao()
    {
        if (!somSelecao.IsNull)
        {
            RuntimeManager.PlayOneShot(
                somSelecao
            );
        }
    }

    // =====================================================
    // INPUT NUMÉRICO 0-999
    // =====================================================

    private void SetupNumericInput(
        TMP_InputField inputField)
    {
        if (inputField == null)
            return;

        inputField.contentType =
            TMP_InputField.ContentType.IntegerNumber;

        inputField.characterLimit = 3;

        inputField.onEndEdit.AddListener(
            (string value) =>
            {
                if (int.TryParse(
                    value,
                    out int numericValue))
                {
                    int clampedValue =
                        Mathf.Clamp(
                            numericValue,
                            0,
                            999
                        );

                    inputField.text =
                        clampedValue.ToString();
                }
                else
                {
                    inputField.text = "";
                }
            }
        );
    }

    // =====================================================
    // INPUT NUMÉRICO 0-99
    // =====================================================

    private void SetupNumericInput2(
        TMP_InputField inputField)
    {
        if (inputField == null)
            return;

        inputField.contentType =
            TMP_InputField.ContentType.IntegerNumber;

        inputField.characterLimit = 2;

        inputField.onEndEdit.AddListener(
            (string value) =>
            {
                if (int.TryParse(
                    value,
                    out int numericValue))
                {
                    int clampedValue =
                        Mathf.Clamp(
                            numericValue,
                            0,
                            99
                        );

                    inputField.text =
                        clampedValue.ToString();
                }
                else
                {
                    inputField.text = "";
                }
            }
        );
    }

    // =====================================================
    // VALIDAR CAMPOS
    // =====================================================

    public bool ValidateAllFields()
    {
        bool valid = true;

        // Todos os campos de texto avaliados pela conduta precisam ter conteúdo.
        valid &= ValidateRequiredInput(frequenciaInput, "Frequência respiratória");
        valid &= ValidateRequiredInput(saturationInput, "Saturação");
        valid &= ValidateRequiredInput(oxigenoterapiaFluxoInput, "Fluxo de oxigenoterapia");
        valid &= ValidateRequiredInput(frequenciaCardiacaInput, "Frequência cardíaca");
        valid &= ValidateRequiredInput(pressaoArterialInput, "Pressão arterial");
        valid &= ValidateRequiredInput(pressaoArterial2Input, "Segundo valor da pressão arterial");
        valid &= ValidateRequiredInput(perfusaoTempoInput, "Tempo de perfusão");
        valid &= ValidateRequiredInput(temperaturaInput, "Temperatura");
        valid &= ValidateRequiredInput(dorEscalaInput, "Escala de dor");

        // Os dropdowns também precisam existir e ter uma opção selecionável.
        // Quando a primeira opção é um placeholder como "Selecione...", ela não conta como resposta.
        valid &= ValidateRequiredDropdown(permeabilidadeDropdown, "Permeabilidade da via aérea");
        valid &= ValidateRequiredDropdown(presencaDropdown, "Presença na via aérea");
        valid &= ValidateRequiredDropdown(intervencaoDropdown, "Intervenção na via aérea");
        valid &= ValidateRequiredDropdown(acessoriaDropdown, "Musculatura acessória");
        valid &= ValidateRequiredDropdown(padraoRespiratorioDropdown, "Padrão respiratório");
        valid &= ValidateRequiredDropdown(ascultaDropdown, "Ausculta");
        valid &= ValidateRequiredDropdown(expansibilidadeDropdown, "Expansibilidade");
        valid &= ValidateRequiredDropdown(oxigenoterapiaTipoDropdown, "Tipo de oxigenoterapia");
        valid &= ValidateRequiredDropdown(perfusaoExtremidadesDropdown, "Perfusão das extremidades");
        valid &= ValidateRequiredDropdown(pulsosDropdown, "Pulsos");
        valid &= ValidateRequiredDropdown(ritmoCardiacoDropdown, "Ritmo cardíaco");
        valid &= ValidateRequiredDropdown(edemaDropdown, "Edema");
        valid &= ValidateRequiredDropdown(nivelConscienciaDropdown, "Nível de consciência");
        valid &= ValidateRequiredDropdown(avalicaoPeleDropdown, "Avaliação da pele");
        valid &= ValidateRequiredDropdown(presencaPeleDropdown, "Presença na pele");

        if (!valid)
        {
            Debug.LogWarning("[MedicalData] O prontuário está incompleto. Confira os campos indicados no Console.", this);
            return false;
        }

        Debug.Log("[MedicalData] Todos os campos obrigatórios foram validados.", this);
        return true;
    }

    private bool ValidateRequiredInput(TMP_InputField field, string fieldName)
    {
        if (field == null)
        {
            Debug.LogError($"[MedicalData] O campo '{fieldName}' não foi atribuído no Inspector.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(field.text))
        {
            Debug.LogWarning($"[MedicalData] Campo obrigatório vazio: {fieldName}.", field);
            return false;
        }

        return true;
    }

    private bool ValidateRequiredDropdown(TMP_Dropdown dropdown, string fieldName)
    {
        if (dropdown == null)
        {
            Debug.LogError($"[MedicalData] O dropdown '{fieldName}' não foi atribuído no Inspector.", this);
            return false;
        }

        if (dropdown.options == null || dropdown.options.Count == 0 ||
            dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
        {
            Debug.LogError($"[MedicalData] O dropdown '{fieldName}' não possui uma opção válida.", dropdown);
            return false;
        }

        string selectedText = dropdown.options[dropdown.value].text;
        if (string.IsNullOrWhiteSpace(selectedText) ||
            selectedText.Trim().StartsWith("Selecione", StringComparison.OrdinalIgnoreCase) ||
            selectedText.Trim().StartsWith("Escolha", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[MedicalData] Selecione uma resposta para: {fieldName}.", dropdown);
            return false;
        }

        return true;
    }

    // =====================================================
    // PEGAR ESTADO ATUAL DA INTERFACE
    // =====================================================

    public ConductState GetCurrentState()
    {
        ConductState state =
            new ConductState();

        // -------------------------------------------------
        // VIA AÉREA
        // -------------------------------------------------

        state.permeabilidade =
            permeabilidadeDropdown.value;

        state.presenca =
            presencaDropdown.value;

        state.intervencao =
            intervencaoDropdown.value;

        // -------------------------------------------------
        // RESPIRAÇÃO
        // -------------------------------------------------

        state.frequencia =
            frequenciaInput.text;

        state.saturation =
            saturationInput.text;

        state.acessoria =
            acessoriaDropdown.value;

        state.padraoRespiratorio =
            padraoRespiratorioDropdown.value;

        state.asculta =
            ascultaDropdown.value;

        state.expansibilidade =
            expansibilidadeDropdown.value;

        state.oxigenoterapiaTipo =
            oxigenoterapiaTipoDropdown.value;

        state.oxigenoterapiaFluxo =
            oxigenoterapiaFluxoInput.text;

        // -------------------------------------------------
        // CIRCULAÇÃO
        // -------------------------------------------------

        state.frequenciaCardiaca =
            frequenciaCardiacaInput.text;

        state.pressaoArterial =
            pressaoArterialInput.text;

        state.pressaoArterial2 =
            pressaoArterial2Input.text;

        state.perfusaoTempo =
            perfusaoTempoInput.text;

        state.perfusaoExtremidades =
            perfusaoExtremidadesDropdown.value;

        state.pulsos =
            pulsosDropdown.value;

        state.ritmoCardiaco =
            ritmoCardiacoDropdown.value;

        state.edema =
            edemaDropdown.value;

        state.temperatura =
            temperaturaInput.text;

        // -------------------------------------------------
        // NEUROLÓGICO
        // -------------------------------------------------

        state.nivelConsciencia =
            nivelConscienciaDropdown.value;

        // -------------------------------------------------
        // EXPOSIÇÃO
        // -------------------------------------------------

        state.avalicaoPele =
            avalicaoPeleDropdown.value;

        state.presencaPele =
            presencaPeleDropdown.value;

        state.dorEscala =
            dorEscalaInput.text;

        return state;
    }

    // =====================================================
    // CARREGAR ESTADO DE UM PACIENTE
    // =====================================================

    public void LoadState(
        ConductState state)
    {
        if (state == null)
        {
            ResetForm();
            return;
        }

        // -------------------------------------------------
        // VIA AÉREA
        // -------------------------------------------------

        permeabilidadeDropdown.SetValueWithoutNotify(
            state.permeabilidade
        );

        presencaDropdown.SetValueWithoutNotify(
            state.presenca
        );

        intervencaoDropdown.SetValueWithoutNotify(
            state.intervencao
        );

        // -------------------------------------------------
        // RESPIRAÇÃO
        // -------------------------------------------------

        frequenciaInput.SetTextWithoutNotify(
            state.frequencia
        );

        saturationInput.SetTextWithoutNotify(
            state.saturation
        );

        acessoriaDropdown.SetValueWithoutNotify(
            state.acessoria
        );

        padraoRespiratorioDropdown.SetValueWithoutNotify(
            state.padraoRespiratorio
        );

        ascultaDropdown.SetValueWithoutNotify(
            state.asculta
        );

        expansibilidadeDropdown.SetValueWithoutNotify(
            state.expansibilidade
        );

        oxigenoterapiaTipoDropdown.SetValueWithoutNotify(
            state.oxigenoterapiaTipo
        );

        oxigenoterapiaFluxoInput.SetTextWithoutNotify(
            state.oxigenoterapiaFluxo
        );

        // -------------------------------------------------
        // CIRCULAÇÃO
        // -------------------------------------------------

        frequenciaCardiacaInput.SetTextWithoutNotify(
            state.frequenciaCardiaca
        );

        pressaoArterialInput.SetTextWithoutNotify(
            state.pressaoArterial
        );

        pressaoArterial2Input.SetTextWithoutNotify(
            state.pressaoArterial2
        );

        perfusaoTempoInput.SetTextWithoutNotify(
            state.perfusaoTempo
        );

        perfusaoExtremidadesDropdown.SetValueWithoutNotify(
            state.perfusaoExtremidades
        );

        pulsosDropdown.SetValueWithoutNotify(
            state.pulsos
        );

        ritmoCardiacoDropdown.SetValueWithoutNotify(
            state.ritmoCardiaco
        );

        edemaDropdown.SetValueWithoutNotify(
            state.edema
        );

        temperaturaInput.SetTextWithoutNotify(
            state.temperatura
        );

        // -------------------------------------------------
        // NEUROLÓGICO
        // -------------------------------------------------

        nivelConscienciaDropdown.SetValueWithoutNotify(
            state.nivelConsciencia
        );

        // -------------------------------------------------
        // EXPOSIÇÃO
        // -------------------------------------------------

        avalicaoPeleDropdown.SetValueWithoutNotify(
            state.avalicaoPele
        );

        presencaPeleDropdown.SetValueWithoutNotify(
            state.presencaPele
        );

        dorEscalaInput.SetTextWithoutNotify(
            state.dorEscala
        );

        AtualizarDropdowns();

        ultimoValorDor =
            state.dorEscala;
    }

    // =====================================================
    // RESETAR FORMULÁRIO
    // =====================================================

    public void ResetForm()
    {
        // -------------------------------------------------
        // VIA AÉREA
        // -------------------------------------------------

        permeabilidadeDropdown.SetValueWithoutNotify(0);
        presencaDropdown.SetValueWithoutNotify(0);
        intervencaoDropdown.SetValueWithoutNotify(0);

        // -------------------------------------------------
        // RESPIRAÇÃO
        // -------------------------------------------------

        frequenciaInput.SetTextWithoutNotify("");
        saturationInput.SetTextWithoutNotify("");

        acessoriaDropdown.SetValueWithoutNotify(0);
        padraoRespiratorioDropdown.SetValueWithoutNotify(0);
        ascultaDropdown.SetValueWithoutNotify(0);
        expansibilidadeDropdown.SetValueWithoutNotify(0);
        oxigenoterapiaTipoDropdown.SetValueWithoutNotify(0);

        oxigenoterapiaFluxoInput.SetTextWithoutNotify("");

        // -------------------------------------------------
        // CIRCULAÇÃO
        // -------------------------------------------------

        frequenciaCardiacaInput.SetTextWithoutNotify("");
        pressaoArterialInput.SetTextWithoutNotify("");
        pressaoArterial2Input.SetTextWithoutNotify("");
        perfusaoTempoInput.SetTextWithoutNotify("");

        perfusaoExtremidadesDropdown.SetValueWithoutNotify(0);
        pulsosDropdown.SetValueWithoutNotify(0);
        ritmoCardiacoDropdown.SetValueWithoutNotify(0);
        edemaDropdown.SetValueWithoutNotify(0);

        temperaturaInput.SetTextWithoutNotify("");

        // -------------------------------------------------
        // NEUROLÓGICO
        // -------------------------------------------------

        nivelConscienciaDropdown.SetValueWithoutNotify(0);

        // -------------------------------------------------
        // EXPOSIÇÃO
        // -------------------------------------------------

        avalicaoPeleDropdown.SetValueWithoutNotify(0);
        presencaPeleDropdown.SetValueWithoutNotify(0);

        dorEscalaInput.SetTextWithoutNotify("");

        AtualizarDropdowns();

        ultimoValorDor = "";
    }

    // =====================================================
    // ATUALIZAR VISUAL DOS DROPDOWNS
    // =====================================================

    private void AtualizarDropdowns()
    {
        if (permeabilidadeDropdown != null)
            permeabilidadeDropdown.RefreshShownValue();

        if (presencaDropdown != null)
            presencaDropdown.RefreshShownValue();

        if (intervencaoDropdown != null)
            intervencaoDropdown.RefreshShownValue();

        if (acessoriaDropdown != null)
            acessoriaDropdown.RefreshShownValue();

        if (padraoRespiratorioDropdown != null)
            padraoRespiratorioDropdown.RefreshShownValue();

        if (ascultaDropdown != null)
            ascultaDropdown.RefreshShownValue();

        if (expansibilidadeDropdown != null)
            expansibilidadeDropdown.RefreshShownValue();

        if (oxigenoterapiaTipoDropdown != null)
            oxigenoterapiaTipoDropdown.RefreshShownValue();

        if (perfusaoExtremidadesDropdown != null)
            perfusaoExtremidadesDropdown.RefreshShownValue();

        if (pulsosDropdown != null)
            pulsosDropdown.RefreshShownValue();

        if (ritmoCardiacoDropdown != null)
            ritmoCardiacoDropdown.RefreshShownValue();

        if (edemaDropdown != null)
            edemaDropdown.RefreshShownValue();

        if (nivelConscienciaDropdown != null)
            nivelConscienciaDropdown.RefreshShownValue();

        if (avalicaoPeleDropdown != null)
            avalicaoPeleDropdown.RefreshShownValue();

        if (presencaPeleDropdown != null)
            presencaPeleDropdown.RefreshShownValue();
    }
}