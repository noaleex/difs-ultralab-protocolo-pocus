using UnityEngine;

public class PatientConductEvaluator : MonoBehaviour
{
    // Quantidade atual de campos comparados em EvaluatePatient.
    private const int EvaluatedFieldCount = 24;
    public bool EvaluatePatient(
        PatientData npc,
        MedicalData medicalDataUI)
    {
        return EvaluatePatient(npc, npc, medicalDataUI);
    }

    public bool EvaluatePatient(
        PatientData npc,
        PatientData expectedClinicalData,
        MedicalData medicalDataUI)
    {
        // npc mantém a identidade usada pelo GameSession; expectedClinicalData
        // contém os valores clínicos sorteados para o dia.
        if (expectedClinicalData == null)
            expectedClinicalData = npc;

        // =====================================================
        // VERIFICAÇÕES
        // =====================================================

        if (npc == null)
        {
            Debug.LogError(
                "[PatientConductEvaluator] PatientData não foi informado!"
            );

            return false;
        }

        if (medicalDataUI == null)
        {
            Debug.LogError(
                "[PatientConductEvaluator] MedicalData não foi informado!"
            );

            return false;
        }

        Debug.Log(
            $"[PatientConductEvaluator] Iniciando avaliação do paciente: " +
            $"{npc.patientName}"
        );

        int acertos = 0;
        int erros = 0;

        // =====================================================
        // VIA AÉREA
        // =====================================================

        Contabilizar(
            medicalDataUI.permeabilidadeDropdown
                .options[
                    medicalDataUI.permeabilidadeDropdown.value
                ].text,

            expectedClinicalData.permeabilidade,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.presencaDropdown
                .options[
                    medicalDataUI.presencaDropdown.value
                ].text,

            expectedClinicalData.presenca,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.intervencaoDropdown
                .options[
                    medicalDataUI.intervencaoDropdown.value
                ].text,

            expectedClinicalData.intervencao,

            ref acertos,
            ref erros
        );

        // =====================================================
        // RESPIRAÇÃO
        // =====================================================

        Contabilizar(
            medicalDataUI.frequenciaInput.text,
            expectedClinicalData.frequencia,
            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.saturationInput.text,
            expectedClinicalData.saturation,
            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.acessoriaDropdown
                .options[
                    medicalDataUI.acessoriaDropdown.value
                ].text,

            expectedClinicalData.acessoria,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.padraoRespiratorioDropdown
                .options[
                    medicalDataUI.padraoRespiratorioDropdown.value
                ].text,

            expectedClinicalData.padraoRespiratorio,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.ascultaDropdown
                .options[
                    medicalDataUI.ascultaDropdown.value
                ].text,

            expectedClinicalData.asculta,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.expansibilidadeDropdown
                .options[
                    medicalDataUI.expansibilidadeDropdown.value
                ].text,

            expectedClinicalData.expansibilidade,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.oxigenoterapiaTipoDropdown
                .options[
                    medicalDataUI.oxigenoterapiaTipoDropdown.value
                ].text,

            expectedClinicalData.oxigenoterapiaTipo,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.oxigenoterapiaFluxoInput.text,
            expectedClinicalData.oxigenoterapiaFluxo,
            ref acertos,
            ref erros
        );

        // =====================================================
        // CIRCULAÇÃO
        // =====================================================

        Contabilizar(
            medicalDataUI.frequenciaCardiacaInput.text,
            expectedClinicalData.frequenciaCardiaca,
            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.pressaoArterialInput.text,
            expectedClinicalData.pressaoArterial,
            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.pressaoArterial2Input.text,
            expectedClinicalData.pressaoArterial2,
            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.perfusaoTempoInput.text,
            expectedClinicalData.perfusaoTempo,
            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.perfusaoExtremidadesDropdown
                .options[
                    medicalDataUI.perfusaoExtremidadesDropdown.value
                ].text,

            expectedClinicalData.perfusaoExtremidades,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.pulsosDropdown
                .options[
                    medicalDataUI.pulsosDropdown.value
                ].text,

            expectedClinicalData.pulsos,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.ritmoCardiacoDropdown
                .options[
                    medicalDataUI.ritmoCardiacoDropdown.value
                ].text,

            expectedClinicalData.ritmoCardiaco,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.edemaDropdown
                .options[
                    medicalDataUI.edemaDropdown.value
                ].text,

            expectedClinicalData.edema,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.temperaturaInput.text,
            expectedClinicalData.temperatura,
            ref acertos,
            ref erros
        );

        // =====================================================
        // NEUROLÓGICO
        // =====================================================

        Contabilizar(
            medicalDataUI.nivelConscienciaDropdown
                .options[
                    medicalDataUI.nivelConscienciaDropdown.value
                ].text,

            expectedClinicalData.nivelConsciencia,

            ref acertos,
            ref erros
        );

        // =====================================================
        // EXPOSIÇÃO
        // =====================================================

        Contabilizar(
            medicalDataUI.avalicaoPeleDropdown
                .options[
                    medicalDataUI.avalicaoPeleDropdown.value
                ].text,

            expectedClinicalData.avalicaoPele,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.presencaPeleDropdown
                .options[
                    medicalDataUI.presencaPeleDropdown.value
                ].text,

            expectedClinicalData.presencaPele,

            ref acertos,
            ref erros
        );

        Contabilizar(
            medicalDataUI.dorEscalaInput.text,
            expectedClinicalData.dorEscala,
            ref acertos,
            ref erros
        );

        // =====================================================
        // WELFARE
        // =====================================================

        int welfareAnterior =
            GameSession.GetPatientWelfare(npc);

        int resultado =
            acertos - erros;

        int novoWelfare =
            welfareAnterior + resultado;

        novoWelfare =
            Mathf.Clamp(
                novoWelfare,
                0,
                74
            );

        // =====================================================
        // SALVAR WELFARE NO GAMESESSION
        // =====================================================

        GameSession.SetPatientWelfare(
            npc,
            novoWelfare
        );

        // =====================================================
        // DEBUG
        // =====================================================

        Debug.Log(
            $"===== AVALIAÇÃO DO PACIENTE =====\n" +
            $"Paciente: {npc.patientName}\n" +
            $"Welfare inicial do PatientData: {npc.welfareScore}\n" +
            $"Valor de acertos: {acertos}\n" +
            $"Valor de erros: {erros}\n" +
            $"Conta: {acertos} - {erros} = {resultado}\n" +
            $"Novo Welfare da sessão: {novoWelfare}\n" +
            $"Welfare no PatientData continua: {npc.welfareScore}\n" +
            $"=================================="
        );

        return erros == 0;
    }

    // =========================================================
    // CONTABILIZAR
    // =========================================================

    private void Contabilizar(
        string valorUI,
        string valorCorreto,
        ref int acertos,
        ref int erros)
    {
        if (valorUI == valorCorreto)
        {
            acertos++;
        }
        else
        {
            erros++;
        }
    }

    public void EvaluateEmptyConduct(
    PatientData npc)
    {
        if (npc == null)
        {
            Debug.LogError(
                "[PatientConductEvaluator] " +
                "Paciente não informado para avaliação vazia."
            );

            return;
        }


        // =====================================================
        // TODOS OS CAMPOS ESTÃO VAZIOS
        // =====================================================
        //
        // Um formulário vazio significa que cada campo que
        // deveria ser preenchido representa um erro.
        //
        // O sistema possui 24 campos avaliados atualmente.
        // Mantenha a constante sincronizada com os Contabilizar de EvaluatePatient.
        // =====================================================

        int acertos = 0;
        int erros = EvaluatedFieldCount;


        // =====================================================
        // ALTERAR WELFARE
        // =====================================================

        int welfareAnterior =
            GameSession.GetPatientWelfare(
                npc
            );


        int resultado =
            acertos - erros;


        int novoWelfare =
            welfareAnterior + resultado;


        novoWelfare =
            Mathf.Clamp(
                novoWelfare,
                0,
                74
            );


        GameSession.SetPatientWelfare(
            npc,
            novoWelfare
        );


        Debug.Log(
            $"===== PRONTUÁRIO VAZIO =====\n" +
            $"Paciente: {npc.patientName}\n" +
            $"Acertos: {acertos}\n" +
            $"Erros: {erros}\n" +
            $"Resultado: {resultado}\n" +
            $"Welfare anterior: {welfareAnterior}\n" +
            $"Novo Welfare: {novoWelfare}\n" +
            $"============================"
        );
    }
}