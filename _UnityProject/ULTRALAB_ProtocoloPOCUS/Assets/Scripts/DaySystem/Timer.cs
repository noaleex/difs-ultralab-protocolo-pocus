using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;


    [Header("Configuração")]
    [SerializeField] private int startHour = 8;
    [SerializeField] private int endHour = 9;


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
        //
        // Isso acontece quando voltamos de outra cena.
        //
        // Nesse caso NÃO devemos mostrar DayTransition.
        // Apenas restauramos o horário e continuamos.
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


        // =================================================
        // SALVAR ESTADO INICIAL
        // =================================================

        SaveCurrentTime();


        UpdateClockText();


        // =================================================
        // PRIMEIRO DIA
        // =================================================
        //
        // Somente aqui mostramos:
        //
        // "Dia 1..."
        //
        // Ao voltar de outra cena isso NÃO acontece.
        // =================================================

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


        // =================================================
        // SALVAR HORÁRIO
        // =================================================

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


        // =================================================
        // SALVAR NOVO DIA
        // =================================================

        SaveCurrentTime();


        UpdateClockText();


        // =================================================
        // AGORA SIM MOSTRAR DAY TRANSITION
        // =================================================

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
                "DayTransition.Instance não encontrado ao iniciar novo dia."
            );


            StartDay();
        }
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


    // =====================================================
    // SAIR DA CENA
    // =====================================================

    private void OnDestroy()
    {
        if (GameSession.HasSavedTime)
        {
            SaveCurrentTime();
        }
    }

    public void SkipToNextDay()
    {
        if (!timerRunning)
            return;

        timerRunning = false;
        SaveCurrentTime();

        DayTransition.Instance.CheckPatientsForSkipDay(OnSkipDayFinished);
    }

    private void OnSkipDayFinished(bool success)
    {
        if (success)
        {
            NextDay();
        }
        else
        {
            timerRunning = true;
        }
    }
}