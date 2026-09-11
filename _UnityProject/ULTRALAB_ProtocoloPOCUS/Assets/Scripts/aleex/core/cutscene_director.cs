using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class cutscene_director : MonoBehaviour
{
    private enum completion_action { LoadTutorial, PlayNextCutscene }

    [SerializeField] private completion_action onCompleteAction = completion_action.LoadTutorial;
    [SerializeField] private string nextStepId = "";

    private PlayableDirector director;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        director.stopped += OnTimelineFinished;
    }

    private void OnDisable()
    {
        director.stopped -= OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector pd)
    {
        if (game_flow_manager.instance == null) return;

        if (onCompleteAction == completion_action.LoadTutorial)
        {
            game_flow_manager.instance.LoadGameplayTutorial();
        }
        else
        {
            game_flow_manager.instance.PlayCutscene(nextStepId);
        }
    }
}