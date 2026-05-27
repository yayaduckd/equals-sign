using UnityEngine;
using System.IO;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Project.Menus;
using Project.SceneLoading;
using TDK.SaveSystem;
using Eflatun.SceneReference;
using TDK.SceneSystem;
using System.Threading.Tasks;

public class AppController : MonoBehaviour
{
    public static AppController Instance { get; private set; }

    private enum Boot { BootMainMenu, BootGameplay, MainMenu, Gameplay }
    [SerializeField] private Boot _bootmode = Boot.Gameplay;
    public enum State { MainMenu, Gameplay, Cutscene, Transitioning }
    [SerializeField] public State state = State.Transitioning;

    [Header("Scenes")]
    [SerializeField] private SceneReference _mainMenuScene;
    [SerializeField] private SceneReference _gameplayScene;
    [SerializeField] private SceneReference _cutsceneScene;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        switch (_bootmode)
        {
            case Boot.BootMainMenu:
                _ = ToMainMenu();
                return;
            case Boot.BootGameplay:
                _ = ToWorld();
                return;
            case Boot.MainMenu:
                SetGameState(State.MainMenu);
                return;
            case Boot.Gameplay:
                SetGameState(State.Gameplay);
                return;
        }
    }

    // ------------ Transitions ------------

    public void Quit()
    {
        SetGameState(State.Transitioning);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public async Task ToMainMenu()
    {
        SetGameState(State.Transitioning);
        await SceneServices.UnloadAllScenes();
        await SceneServices.LoadScene(_mainMenuScene);
        await MainMenuController.Instance.Load();
        SetGameState(State.MainMenu);
    }

    public async Task ToCreditsSequence()
    {
        SetGameState(State.Transitioning);
        await SceneServices.UnloadAllScenes();
        await SceneServices.LoadScene(_mainMenuScene);
        await MainMenuController.Instance.Load(loadCredits: true);
        SetGameState(State.MainMenu);
    }

    private readonly string[] _worldIds = { "1", "2", "3" };

    public async Task ToNewWorld(string worldId = null)
    {
        if (worldId == null || SaveServices.ExistsWorld(worldId))
            worldId = SaveServices.GetFreeWorldId(_worldIds);

        if (worldId == null)
        {
            Debug.LogWarning("Main: Cannot make any new worlds; ruh oh!.");
            return;
        }
        await SceneServices.UnloadAllScenes();
        await SceneServices.LoadScene(_gameplayScene);
        SetGameState(State.Gameplay);
        await GameplayController.Instance.LoadWorld(worldId);
    }

    public async Task ToWorld(string worldId = null)
    {
        worldId ??= PlayerPrefs.GetString("lastWorldUsed", null);
        if (worldId == null || !SaveServices.ExistsWorld(worldId))
        {
            await ToNewWorld();
            return;
        }
        await SceneServices.UnloadAllScenes();
        await SceneServices.LoadScene(_gameplayScene);
        SetGameState(State.Gameplay);
        await GameplayController.Instance.LoadWorld(worldId);
    }

    // ------------ Other Functions ------------

    private void SetGameState(State newState)
    {
        state = newState;

        switch (state)
        {
            case State.MainMenu:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case State.Gameplay:
                Time.timeScale = 1f;
                // Cursor.lockState = CursorLockMode.Locked;
                // Cursor.visible = false;
                break;
            case State.Cutscene:
                Time.timeScale = 0f;
                break;
            case State.Transitioning:
                Time.timeScale = 0f;
                break;
        }
    }
}