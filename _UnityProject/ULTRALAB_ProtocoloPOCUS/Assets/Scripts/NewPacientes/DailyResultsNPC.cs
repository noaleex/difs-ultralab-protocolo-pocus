using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>NPC que mostra avaliações reais pendentes e só as consome quando o diálogo termina.</summary>
public class DailyResultsNPC : MonoBehaviour, IInteractable
{
    [Header("Diálogo") ]
    [SerializeField] private string npcName = "Enfermeiro(a)";
    [SerializeField] private Sprite portrait;
    [SerializeField, Min(0f)] private float typingSpeed = 0.02f;
    [SerializeField] private string noResultsMessage = "Ainda não há resultados do dia anterior para consultar.";

    private List<GameSession.DailyEvaluationResult> currentResults;
    private readonly List<string> lines = new List<string>();
    private int lineIndex;
    private bool dialogueActive;
    private bool typing;
    private Coroutine typingRoutine;

    public bool CanInteract() => !dialogueActive;

    public void Interact()
    {
        if (dialogueActive) return;
        if (dialogue_hud.instance == null)
        {
            Debug.LogError("[DailyResultsNPC] dialogue_hud não está disponível na cena.", this);
            return;
        }

        currentResults = GameSession.GetPendingDailyResults();
        lines.Clear();

        if (currentResults.Count == 0)
        {
            lines.Add(noResultsMessage);
        }
        else
        {
            int previousDay = -1;
            foreach (GameSession.DailyEvaluationResult result in currentResults)
            {
                if (result == null || result.Patient == null) continue;
                if (result.Day != previousDay)
                {
                    lines.Add($"Resultados do dia {result.Day}:");
                    previousDay = result.Day;
                }

                bool discharged = GameSession.IsPatientDischarged(result.Patient);
                string status = discharged
                    ? "O paciente recebeu alta."
                    : $"Continue tratando o paciente {result.Patient.patientName}.";
                lines.Add($"{result.Patient.patientName}: você acertou {result.Correct}/{result.Total}. {status}");
            }
            if (lines.Count == 0) lines.Add(noResultsMessage);
        }

        lineIndex = 0;
        dialogueActive = true;
        typing = false;

        dialogue_hud hud = dialogue_hud.instance;
        hud.nameText.text = npcName;
        hud.portraitImage.sprite = portrait;
        hud.dialoguePanel.SetActive(true);

        pause_manager.SetPause(true);
        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.DisablePlayer();
            if (PlayerReferences.Instance.InteractIcon != null)
                PlayerReferences.Instance.InteractIcon.SetActive(false);
        }

        ShowCurrentLine();
    }

    private void Update()
    {
        if (!dialogueActive || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (typing)
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            dialogue_hud.instance.dialogueText.text = lines[lineIndex];
            typing = false;
            return;
        }

        lineIndex++;
        if (lineIndex >= lines.Count) EndDialogue();
        else ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[lineIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        typing = true;
        TMP_Text text = dialogue_hud.instance.dialogueText;
        text.text = string.Empty;
        if (typingSpeed <= 0f)
        {
            text.text = line;
            typing = false;
            yield break;
        }

        foreach (char character in line)
        {
            text.text += character;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        typing = false;
    }

    private void EndDialogue()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        dialogueActive = false;
        typing = false;

        if (dialogue_hud.instance != null)
        {
            dialogue_hud.instance.dialogueText.text = string.Empty;
            dialogue_hud.instance.dialoguePanel.SetActive(false);
        }

        // Os resultados só deixam de estar pendentes após o jogador terminar de lê-los.
        if (currentResults != null && currentResults.Count > 0)
            GameSession.MarkDailyResultsCollected(currentResults);
        currentResults = null;
        lines.Clear();

        pause_manager.SetPause(false);
        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.EnablePlayer();
            if (PlayerReferences.Instance.InteractIcon != null)
                PlayerReferences.Instance.InteractIcon.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (dialogueActive) EndDialogue();
    }
}
