using UnityEngine;
using System.IO;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Project.Menus;
using Project.SceneLoading;
using TDK.SaveSystem;
using System.Threading.Tasks;
using TDK.SceneSystem;
using Eflatun.SceneReference;

public class GameplayController : MonoBehaviour
{
    public enum State { Paused, Playing, Transitioning }
    public static GameplayController Instance { get; private set; }
    [SerializeField] public State state = State.Transitioning;

    [SerializeField] private TransitionScreenController _tsc;

    [Header("Important Scenes")]
    [SerializeField] private SceneReference _pauseScene;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ------------ Transitions ------------

    public void QuitToDesktop()
    {
        SetGameState(State.Transitioning);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public async Task QuitToMainMenu()
    {
        SetGameState(State.Transitioning);
        await _tsc.EnterTransitionScreen();
        SaveManager.Instance.SaveWorld();
        MenuManager.Instance.ToMenu(null, false);
        await AppController.Instance.ToMainMenu();
    }

    public async Task FinishGame()
    {
        SetGameState(State.Transitioning);
        await _tsc.EnterTransitionScreen();
        SaveManager.Instance.SaveWorld();
        await AppController.Instance.ToCreditsSequence();
    }

    public async Task Sleep()
    {
        SetGameState(State.Transitioning);
        await _tsc.EnterTransitionScreen();
        StoryFlagManager.Instance.OnTimePassing();
        SaveManager.Instance.SaveWorld();
        await _tsc.ExitTransitionScreen();
        SetGameState(State.Playing);
    }

    public async Task Death()
    {
        SetGameState(State.Transitioning);
        // TODO: add duck falling and eating shit?
        await _tsc.EnterTransitionScreen();
        SaveManager.Instance.SaveWorld();
        await _tsc.ExitTransitionScreen();
        SetGameState(State.Playing);
    }

    public void Escape()
    {
        if (state == State.Paused) _ = ResumeGame();
        else if (state == State.Playing) _ = PauseGame();
    }

    public async Task PauseGame()
    {
        SetGameState(State.Transitioning);
        await SceneServices.LoadScene(_pauseScene);
        SetGameState(State.Paused);
    }

    public async Task ResumeGame()
    {
        SetGameState(State.Transitioning);
        MenuManager.Instance.ToMenu(null, false);
        await SceneServices.UnloadScene(_pauseScene);
        SetGameState(State.Playing);
    }

    // ------------ Other Functions ------------

    private void SetGameState(State gameState)
    {
        state = gameState;

        switch (state)
        {
            case State.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case State.Playing:
                Time.timeScale = 1f;
                // Cursor.lockState = CursorLockMode.Locked;
                // Cursor.visible = false;
                break;
            case State.Transitioning:
                Time.timeScale = 0f;
                break;
        }
    }
}

