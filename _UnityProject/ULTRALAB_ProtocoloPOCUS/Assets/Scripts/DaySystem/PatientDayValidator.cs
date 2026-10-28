using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PatientDayValidator : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private MedicalData medicalDataUI;


    [Header("Avaliação")]
    [SerializeField] private PatientConductEvaluator conductEvaluator;


    [Header("Configuração")]
    [SerializeField] private float messageTime = 1.5f;


    public enum PatientResult
    {
        Continuing,
        Discharged,
        GameOver
    }


    // =====================================================
    // VERIFICAR SE TODOS FIZERAM A CONDUTA
    // =====================================================

    public bool AllPatientsCompletedConduct()
    {
        if (PatientManager.Instance == null)
        {
            Debug.LogError(
                "PatientManager não encontrado!"
            );

            return false;
        }


        return PatientManager.Instance
            .AllPatientsCompletedConduct();
    }


    // =====================================================
    // FIM DO DIA PELO RELÓGIO
    // ====================================================

    public IEnumerator ValidatePatientsAtEndOfDay(
        Action<bool> onFinished,
        TextMeshProUGUI messageText)
    {
        if (PatientManager.Instance == null)
        {
            Debug.LogError(
                "PatientManager não encontrado!"
            );

            onFinished?.Invoke(false);

            yield break;
        }


        // =================================================
        // PEGAR PACIENTES
        // =================================================

        IReadOnlyList<OpenExams> patients =
            PatientManager.Instance
                .GetPatients();


        List<OpenExams> patientsToDischarge =
            new List<OpenExams>();


        bool gameOver = false;


        // =================================================
        // AVALIAR CADA PACIENTE
        // =================================================

        foreach (OpenExams patientObject
                 in patients)
        {
            if (patientObject == null)
                continue;


            PatientData patient =
                patientObject.PatientDataReference;


            if (patient == null)
                continue;


            // ---------------------------------------------
            // VERIFICAR SE A CONDUTA FOI PREENCHIDA
            // ---------------------------------------------

            bool conductCompleted =
                patientObject.ConductCompleted;


            if (!conductCompleted)
            {
                Debug.LogWarning(
                    $"[PatientDayValidator] " +
                    $"O paciente {patient.patientName} " +
                    $"não teve o prontuário preenchido. " +
                    $"Campos vazios serão considerados erros."
                );


                // -----------------------------------------
                // AQUI NÃO BLOQUEAMOS O DIA.
                //
                // O paciente será avaliado pelo welfare
                // que já foi alterado pelas condutas feitas.
                //
                // Para contabilizar os campos vazios como
                // erros, fazemos uma avaliação automática
                // usando um formulário vazio.
                // -----------------------------------------

                if (conductEvaluator != null)
                {
                    conductEvaluator.EvaluateEmptyConduct(
                        patient
                    );
                }
            }


            // ---------------------------------------------
            // SALVAR ESTADO ATUAL
            // ---------------------------------------------

            GameSession.SavePatient(
                patient
            );


            PatientResult result =
                EvaluatePatient(
                    patient
                );


            // =============================================
            // ALTA
            // =============================================

            if (result ==
                PatientResult.Discharged)
            {
                if (messageText != null)
                {
                    messageText.text =
                        $"O paciente {patient.patientName} ganhou alta";


                    messageText.gameObject
                        .SetActive(true);
                }


                yield return
                    new WaitForSecondsRealtime(
                        messageTime
                    );


                // -----------------------------------------
                // REGISTRAR ALTA
                // -----------------------------------------

                GameSession.MarkPatientAsDischarged(
                    patient
                );


                patientsToDischarge.Add(
                    patientObject
                );
            }


            // =============================================
            // GAME OVER
            // =============================================

            else if (
                result ==
                PatientResult.GameOver)
            {
                if (messageText != null)
                {
                    messageText.text =
                        $"O paciente {patient.patientName} piorou, game over";


                    messageText.gameObject
                        .SetActive(true);
                }


                yield return
                    new WaitForSecondsRealtime(
                        messageTime
                    );


                GameSession.SavePatient(
                    patient
                );


                gameOver = true;

                break;
            }


            // =============================================
            // CONTINUA
            // =============================================

            else
            {
                if (messageText != null)
                {
                    messageText.text =
                        $"O paciente {patient.patientName} continua em tratamento";


                    messageText.gameObject
                        .SetActive(true);
                }


                yield return
                    new WaitForSecondsRealtime(
                        messageTime
                    );


                GameSession.SavePatient(
                    patient
                );
            }
        }


        // =================================================
        // REMOVER PACIENTES QUE TIVERAM ALTA
        // =================================================

        foreach (OpenExams patient
                 in patientsToDischarge)
        {
            if (patient != null)
            {
                Destroy(
                    patient.gameObject
                );
            }
        }


        // =================================================
        // GAME OVER
        // =================================================

        if (gameOver)
        {
            GameOver();

            onFinished?.Invoke(false);

            yield break;
        }


        // =================================================
        // FINALIZOU O DIA
        // =================================================

        if (messageText != null)
        {
            messageText.gameObject
                .SetActive(false);
        }


        // =================================================
        // IMPORTANTE:
        //
        // O dia SEMPRE pode terminar quando chegou
        // naturalmente ao endHour.
        // =================================================

        onFinished?.Invoke(true);
    }


    // =====================================================
    // VERIFICAÇÃO PARA SKIP DAY
    // =====================================================
    //
    // Diferentemente do relógio, SkipDay exige que todos
    // os prontuários estejam preenchidos.
    // =====================================================

    public IEnumerator ValidatePatientsForSkipDay(
        Action<bool> onFinished,
        TextMeshProUGUI messageText)
    {
        if (PatientManager.Instance == null)
        {
            Debug.LogError(
                "PatientManager não encontrado!"
            );

            onFinished?.Invoke(false);

            yield break;
        }


        // =================================================
        // TODOS OS PACIENTES PRECISAM ESTAR COMPLETOS
        // =================================================

        IReadOnlyList<OpenExams> registeredPatients =
            PatientManager.Instance.GetPatients();

        if (registeredPatients.Count == 0)
        {
            Debug.LogError("[PatientDayValidator] SkipDay bloqueado: PatientManager não possui pacientes registrados.");
            yield return ShowSkipValidationMessage(
                messageText,
                "Nenhum paciente foi registrado. Verifique o PatientManager e os componentes OpenExams."
            );
            onFinished?.Invoke(false);
            yield break;
        }

        if (!AllPatientsCompletedConduct())
        {
            yield return ShowSkipValidationMessage(
                messageText,
                "Confirme a conduta de todos os pacientes antes de pular o dia"
            );

            onFinished?.Invoke(false);
            yield break;
        }


        // =================================================
        // ESTÁ TUDO PREENCHIDO
        // =================================================
        //
        // Agora podemos utilizar a mesma avaliação do
        // final normal do dia.
        // =================================================

        yield return StartCoroutine(
            ValidatePatientsAtEndOfDay(
                onFinished,
                messageText
            )
        );
    }


    private IEnumerator ShowSkipValidationMessage(
        TextMeshProUGUI messageText,
        string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(messageTime);

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }


    // =====================================================
    // AVALIAR PACIENTE
    // =====================================================

    private PatientResult EvaluatePatient(
        PatientData patient)
    {
        int welfare =
            GameSession.GetPatientWelfare(
                patient
            );


        Debug.Log(
            $"[PatientDayValidator] " +
            $"Paciente: {patient.patientName} | " +
            $"Welfare atual: {welfare} | " +
            $"Welfare original: {patient.welfareScore}"
        );


        if (welfare >= 74)
        {
            return PatientResult.Discharged;
        }


        if (welfare <= 0)
        {
            return PatientResult.GameOver;
        }


        return PatientResult.Continuing;
    }


    // =====================================================
    // RESETAR CONDUTAS
    // =====================================================

    public void ResetAllPatientsForNewDay()
    {
        if (PatientManager.Instance == null)
        {
            Debug.LogError(
                "PatientManager não encontrado!"
            );

            return;
        }


        IReadOnlyList<OpenExams> patients =
            PatientManager.Instance
                .GetPatients();


        foreach (OpenExams patient
                 in patients)
        {
            if (patient == null)
                continue;


            // ---------------------------------------------
            // PREPARAR DADOS CLÍNICOS ALEATÓRIOS DO PRÓXIMO DIA
            // ---------------------------------------------

            if (patient.PatientDataReference != null)
            {
                int nextDay = Timer.Instance != null ? Timer.Instance.CurrentDay + 1 : 1;
                PatientDailyState.PrepareForDay(patient.PatientDataReference, nextDay);
            }

            // ---------------------------------------------
            // RESETAR SOMENTE A CONDUTA
            // ---------------------------------------------

            patient.ResetConductForNewDay();


            // ---------------------------------------------
            // GARANTIR QUE O WELFARE CONTINUE SALVO
            // ---------------------------------------------

            if (patient.PatientDataReference != null)
            {
                GameSession.SavePatient(
                    patient.PatientDataReference
                );
            }
        }


        // Imagem anexada é específica do paciente e do dia.
        ExamsSaveData.Clear();
        CurrentPatient.Clear();

        // =================================================
        // RESETAR UI
        // =================================================

        if (medicalDataUI != null)
        {
            medicalDataUI.ResetForm();
        }
        else
        {
            Debug.LogWarning(
                "MedicalData não foi atribuído no PatientDayValidator."
            );
        }


        Debug.Log(
            "Condutas resetadas para o novo dia. " +
            "Pontuação dos pacientes mantida."
        );
    }


    // =====================================================
    // GAME OVER
    // =====================================================

    private void GameOver()
    {
        Debug.Log("[PatientDayValidator] Final negativo: um paciente chegou a 0 pontos.");
        GameEndingManager.FinishGame(GameEndingManager.EndingType.Negative);
    }
}