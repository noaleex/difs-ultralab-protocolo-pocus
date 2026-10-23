using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI dayOfWeekText; // <-- Novo campo para o dia

    [Header("Configuração")]
    [SerializeField] private int startHour = 8;
    [SerializeField] private int endHour = 16;

    public int CurrentDay { get; private set; }
    public int CurrentHour => currentHour;
    public int CurrentMinute => currentMinute;

    private int currentHour;
    private int currentMinute;
    private float secondCounter;
    private bool timerRunning;
    
    public bool isInitialized { get; private set; }

    // Array da escala 5x2
    private readonly string[] weekDays = { 
        "SEGUNDA", 
        "TERÇA", 
        "QUARTA", 
        "QUINTA", 
        "SEXTA" 
    };

    private void Awake()
    {
        instance = this;
    }

    public void InitializeTimer()
    {
        if (isInitialized) return;
        isInitialized = true;

        bool validSaveLoaded = false;

        if (GameSession.HasSavedTime)
        {
            GameSession.LoadTime(out int savedDay, out int savedHour, out int savedMinute);
            
            // Trava de segurança: só confia no save se o dia for 1 ou maior
            if (savedDay > 0)
            {
                CurrentDay = savedDay;
                currentHour = savedHour;
                currentMinute = savedMinute;
                validSaveLoaded = true;
            }
        }

        // Se não tinha save, ou se o save estava corrompido (dia 0)
        if (!validSaveLoaded)
        {
            CurrentDay = 1;
            currentHour = startHour;
            currentMinute = 0;
            
            // Força o salvamento imediato para sobrescrever o save bugado de "dia 0"
            SaveCurrentTime(); 
        }

        UpdateClockText();

        if (DayTransition.Instance != null)
        {
            DayTransition.Instance.BeginDay(CurrentDay, StartDay, true);
        }
        else
        {
            StartDay();
        }
    }

    private void Update()
    {
        if (!timerRunning) return;

        secondCounter += Time.deltaTime;

        if (secondCounter >= 1f)
        {
            secondCounter -= 1f;
            AddMinute();
        }
    }

    private void AddMinute()
    {
        currentMinute++;

        if (currentMinute >= 60)
        {
            currentMinute = 0;
            currentHour++;
        }

        SaveCurrentTime();
        UpdateClockText();

        if (currentHour >= endHour)
        {
            timerRunning = false;
            EndDay();
        }
    }

    public void SaveCurrentTime()
    {
        GameSession.SaveTime(CurrentDay, currentHour, currentMinute);
    }

    private void EndDay()
    {
        if (DayTransition.Instance != null)
        {
            DayTransition.Instance.CheckPatientsAtEndOfDay(NextDay);
        }
    }

    private void NextDay()
    {
        CurrentDay++;
        currentHour = startHour;
        currentMinute = 0;

        SaveCurrentTime();
        UpdateClockText();

        DayTransition.Instance.BeginDay(CurrentDay, StartDay, false);
    }

    private void StartDay()
    {
        secondCounter = 0f;
        timerRunning = true;
    }

    public void SkipToNextDay()
    {
        if (!timerRunning) return;
        timerRunning = false;
        SaveCurrentTime();
        EndDay();
    }

    private void UpdateClockText()
    {
        if (timerText != null)
        {
            timerText.text = $"{currentHour:00}:{currentMinute:00}";
        }

        if (dayOfWeekText != null)
        {
            // Garante que o dia nunca seja zero ou negativo (proteção contra saves antigos)
            int safeDay = Mathf.Max(1, CurrentDay); 
            int dayIndex = (safeDay - 1) % weekDays.Length;
            dayOfWeekText.text = weekDays[dayIndex];
        }
    }

    private void OnDisable()
    {
        if (GameSession.HasSavedTime)
        {
            SaveCurrentTime();
        }
    }

    public void TentarDormir()
    {
        if (!timerRunning) return;
        timerRunning = false;
        SaveCurrentTime();

        // Puxa a nota calculada no Prontuário
        string notaDoPlantao = "N/A";
        if (prontuario_manager.instance != null)
        {
            notaDoPlantao = prontuario_manager.instance.notaFinal;
        }

        // Se fracassou ou esqueceu de validar
        if (notaDoPlantao == "D" || notaDoPlantao == "N/A")
        {
            RepetirDiaAtual(notaDoPlantao);
        }
        else
        {
            // Sucesso: Vai pro próximo dia
            AvancarParaProximoDia(notaDoPlantao);
        }
    }

    private void RepetirDiaAtual(string nota)
    {
        // Não aumenta o CurrentDay. Apenas reseta as horas.
        currentHour = startHour;
        currentMinute = 0;
        
        SaveCurrentTime();
        UpdateClockText();

        // Passa a nota para o DayTransition exibir
        if (DayTransition.Instance != null)
            DayTransition.Instance.BeginDay(CurrentDay, StartDay, false, nota);
    }

    private void AvancarParaProximoDia(string nota)
    {
        CurrentDay++;
        currentHour = startHour;
        currentMinute = 0;

        SaveCurrentTime();
        UpdateClockText();

        if (DayTransition.Instance != null)
            DayTransition.Instance.BeginDay(CurrentDay, StartDay, false, nota);
    }
}