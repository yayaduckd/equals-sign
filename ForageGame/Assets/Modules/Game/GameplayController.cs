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
using UnityEngine.InputSystem;
using TDK.PlayerSystem;

public class GameplayController : MonoBehaviour
{
    public static GameplayController Instance { get; private set; }

    public enum State { Paused, Playing, Transitioning, Cutscene }
    public State _state { get; private set; } = State.Transitioning;
    [SerializeField] private TransitionScreenController _tsc;
    [SerializeField] public SaveManager _saveManager;

    [Header("Scenes")]
    [SerializeField] private SceneReference _worldScene;
    [SerializeField] private SceneReference _pauseScene;
    [SerializeField] private SceneReference _cutscene;

    [Header("Story Flags")]
    [SerializeField] private StoryFlag firstNight;

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

        SaveManager.Instance.SaveWorld();
        AppController.Instance.Quit();
    }

    public async Task QuitToMainMenu()
    {
        SetGameState(State.Transitioning);
        await _tsc.FadeOutAsync();
        await AwaitPadding();

        SaveManager.Instance.SaveWorld();

        await AppController.Instance.ToMainMenu();
    }

    public async Task FinishGame()
    {
        SetGameState(State.Transitioning);
        await _tsc.FadeOutAsync();
        await AwaitPadding();

        SaveManager.Instance.SaveWorld();

        await AppController.Instance.ToCreditsSequence();
    }

    public async Task Sleep()
    {
        Player.Instance.gameObject.SetActive(false);

        SetGameState(State.Transitioning);
        await _tsc.FadeOutAsync();
        await AwaitPadding();

        bool firstNight = !StoryFlagManager.Instance.FlagActive(this.firstNight);
        StoryFlagManager.Instance.AddFlag(this.firstNight);// it does not matter if we keep adding the flag after the first night, nothing will change
        StoryFlagManager.Instance.OnTimePassing();
        SaveManager.Instance.SaveWorld();



        await SceneServices.UnloadScene(_worldScene);

        // if first night: do stuff
        // the following is kinda botched and defo not how this should be implemented...
        if (firstNight) // first night cutscene
        {
            await SceneServices.LoadScene(_cutscene);
            SetGameState(State.Cutscene);
            await AwaitPadding();
            await ImageCutsceneController.Instance.PlayFirstNightSequence();
            await AwaitPadding();
            SetGameState(State.Transitioning);
            await SceneServices.UnloadScene(_cutscene);
        }
        await SceneServices.LoadScene(_worldScene);
        Player.Instance.gameObject.SetActive(true);
        SaveManager.Instance.LoadWorld();


        await AwaitPadding();
        _tsc.FadeIn();
        SetGameState(State.Playing);
    }

    public async Task Death()
    {
        // TODO: add duck falling and eating shit?

        SetGameState(State.Transitioning);
        await _tsc.FadeOutAsync();
        await AwaitPadding();

        SaveManager.Instance.SaveWorld();
        // TODO: TP to spawn and lose items?

        await AwaitPadding();
        _tsc.FadeIn();
        SetGameState(State.Playing);
    }

    public void Escape()
    {
        if (_state == State.Paused) PauseMenuController.Instance.Escape();
        else if (_state == State.Playing) _ = PauseGame();
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
        await SceneServices.UnloadScene(_pauseScene);
        SetGameState(State.Playing);
    }

    public async Task LoadWorld(string worldId)
    {
        SetGameState(State.Transitioning);

        await SceneServices.LoadScene(_worldScene);
        // TODO if it is not a new save, delete all the items laying around in the world
        _saveManager.SelectWorld(worldId);
        _saveManager.LoadWorld();

        await AwaitPadding();
        _tsc.FadeIn();
        SetGameState(State.Playing);
    }

    public async Task LoadDebug()
    {
        SetGameState(State.Transitioning);
        _saveManager.SelectWorld("-1");

        await AwaitPadding();
        _tsc.FadeIn();
        SetGameState(State.Playing);
    }

    // ------------ Other Functions ------------

    private async Task AwaitPadding()
    {
        await Task.Delay(Mathf.CeilToInt(500)); //padding
    }

    private void SetGameState(State gameState)
    {
        _state = gameState;

        switch (_state)
        {
            case State.Paused:
                Time.timeScale = 0f;
                AppController.Instance.InputsAllActive(true);
                // Cursor.lockState = CursorLockMode.None;
                // Cursor.visible = true;
                break;
            case State.Playing:
                AppController.Instance.InputsAllActive(true);
                Time.timeScale = 1f;
                // Cursor.lockState = CursorLockMode.Locked;
                // Cursor.visible = false;
                break;
            case State.Transitioning:
                AppController.Instance.InputsAllActive(false);
                Time.timeScale = 0f;
                break;
            case State.Cutscene:
                AppController.Instance.InputsAllActive(false);
                Time.timeScale = 0f;
                break;
        }
    }
}

