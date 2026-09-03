using System.Threading.Tasks;
using UnityEngine;

public class InGameCutsceneManager : MonoBehaviour
{
    public static InGameCutsceneManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    [SerializeField] private Animator _animator;

    private bool _isPlaying;

    public void StartScriptedEvent(string cutsceneName, bool lockInputs)
    {
        if (_isPlaying)
        {
            Debug.LogWarning("Cannot start in game cutscene while an in game cutscene is playing.");
            return;
        }
        AppController.Instance.InputsAllActive(lockInputs);
        _animator.Play(cutsceneName);
    }

    public void StopScriptedEvent(string cutsceneName, bool lockInputs) // for looping events
    {
        _animator.SetTrigger("Stop");
    }

    public void PlayCutscene(string cutsceneName, bool useTransitionScreen)
    {
        if (_isPlaying)
        {
            Debug.LogWarning("Cannot start in game cutscene while an in game cutscene is playing.");
            return;
        }
        _ = GameplayController.Instance.InGameCutsceneStart(_animator, cutsceneName, useTransitionScreen);
    }

    public void OnStateExit()
    {
        if (GameplayController.Instance._state == GameplayController.State.Cutscene)
            _ = GameplayController.Instance.InGameCutsceneStop(false);
        AppController.Instance.InputsAllActive(true); // safety
        _animator.ResetTrigger("Stop");
        _isPlaying = false;
    }
}
