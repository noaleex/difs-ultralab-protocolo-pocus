using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;


    [Header("Configuração")]
    [SerializeField] private int startHour = 8;
    [SerializeField] private int endHour = 16;


    public int CurrentDay
    {
        get;
        private set;
    }


    public int CurrentHour
    {
        get
        {
            return currentHour;
        }
    }


    public int CurrentMinute
    {
        get
        {
            return currentMinute;
        }
    }


    private int currentHour;
    private int currentMinute;

    private float secondCounter;

    private bool timerRunning;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // =================================================
        // EXISTE UM ESTADO SALVO?
        // =================================================

        if (GameSession.HasSavedTime)
        {
            GameSession.LoadTime(
                out int savedDay,
                out int savedHour,
                out int savedMinute
            );


            CurrentDay =
                savedDay;


            currentHour =
                savedHour;


            currentMinute =
                savedMinute;


            UpdateClockText();


            Debug.Log(
                $"[Timer] Estado restaurado: " +
                $"Dia {CurrentDay} - " +
                $"{currentHour:00}:{currentMinute:00}"
            );


            StartDay();

            return;
        }


        // =================================================
        // NOVA PARTIDA
        // =================================================

        CurrentDay = 1;

        currentHour =
            startHour;

        currentMinute = 0;


        SaveCurrentTime();

        UpdateClockText();


        if (DayTransition.Instance != null)
        {
            DayTransition.Instance.BeginDay(
                CurrentDay,
                StartDay
            );
        }
        else
        {
            Debug.LogError(
                "DayTransition.Instance não encontrado!"
            );

            StartDay();
        }
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!timerRunning)
            return;


        secondCounter +=
            Time.deltaTime;


        if (secondCounter >= 1f)
        {
            secondCounter -= 1f;

            AddMinute();
        }
    }


    // =====================================================
    // ADICIONAR MINUTO
    // =====================================================

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


        // =================================================
        // FIM DO DIA
        // =================================================

        if (currentHour >= endHour)
        {
            timerRunning = false;

            EndDay();
        }
    }


    // =====================================================
    // SALVAR HORÁRIO
    // =====================================================

    public void SaveCurrentTime()
    {
        GameSession.SaveTime(
            CurrentDay,
            currentHour,
            currentMinute
        );
    }


    // =====================================================
    // FIM DO DIA
    // =====================================================

    private void EndDay()
    {
        if (DayTransition.Instance == null)
        {
            Debug.LogError(
                "DayTransition.Instance não encontrado!"
            );

            return;
        }


        DayTransition.Instance
            .CheckPatientsAtEndOfDay(
                NextDay
            );
    }


    // =====================================================
    // PRÓXIMO DIA
    // =====================================================

    private void NextDay()
    {
        CurrentDay++;

        currentHour =
            startHour;

        currentMinute = 0;


        SaveCurrentTime();

        UpdateClockText();


        DayTransition.Instance.BeginDay(
            CurrentDay,
            StartDay
        );
    }


    // =====================================================
    // COMEÇAR DIA
    // =====================================================

    private void StartDay()
    {
        secondCounter = 0f;

        timerRunning = true;
    }


    // =====================================================
    // PULAR PARA PRÓXIMO DIA
    // =====================================================

    public void SkipToNextDay()
    {
        if (!timerRunning)
            return;


        timerRunning = false;


        SaveCurrentTime();


        EndDay();
    }


    // =====================================================
    // ATUALIZAR TEXTO
    // =====================================================

    private void UpdateClockText()
    {
        if (timerText == null)
            return;


        timerText.text =
            $"{currentHour:00}:{currentMinute:00}";
    }

    private void OnDisable()
    {
        if (GameSession.HasSavedTime)
        {
            SaveCurrentTime();
        }
    }
}