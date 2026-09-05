using UnityEngine;
using Project.Menus;
using TDK.SaveSystem;
using Eflatun.SceneReference;
using TDK.SceneSystem;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


public class AppController : MonoBehaviour
{
    public static AppController Instance { get; private set; }

    private enum Boot { BootMainMenu, BootGameplay, MainMenu, Gameplay }
    [SerializeField] private Boot _bootmode = Boot.Gameplay;
    public enum State { Boot, MainMenu, Gameplay, Cutscene, Transitioning }
    public State _state { get; private set; } = State.Boot;

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

    void Start() // BOOT SEQUENCE
    {
        switch (_bootmode)
        {
            case Boot.BootMainMenu:
                _ = ToMainMenu();
                return;
            case Boot.BootGameplay:
                SaveServices.DeleteWorld("-1");
                _ = ToNewWorld("-1");
                return;
            case Boot.MainMenu:
                _state = State.MainMenu;
                Time.timeScale = 0f;
                return;
            case Boot.Gameplay:
                SaveServices.DeleteWorld("-1");
                SaveServices.CreateWorld("-1");
                GameplayController.Instance?.LoadDebug();
                _state = State.Gameplay;
                Time.timeScale = 1f;
                return;
        }
    }

    // ------------ Transitions ------------

    public void Quit()
    {
        _state = State.Transitioning;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public async Task ToMainMenu()
    {
        await TransitionTo(State.MainMenu);
        await MainMenuController.Instance.Load();
    }

    public async Task ToCreditsSequence()
    {
        await TransitionTo(State.Cutscene);
        await ImageCutsceneController.Instance.PlayOutroSequence();
        await TransitionTo(State.MainMenu);
        await MainMenuController.Instance.Load(loadCredits: true);
    }

    private readonly string[] _worldIds = { "1", "2", "3" };

    public async Task ToNewWorld(string worldId = null)
    {
        if (worldId == null || SaveServices.ExistsWorld(worldId))
            worldId = SaveServices.GetFreeWorldId(_worldIds);

        if (worldId == null)
        {
            Debug.LogWarning("[AppController] Cannot make any new worlds; ruh oh!.");
            return;
        }
        await TransitionTo(State.Cutscene);
        await ImageCutsceneController.Instance.PlayIntroSequence();
        await TransitionTo(State.Gameplay);
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
        await TransitionTo(State.Gameplay);
        await GameplayController.Instance.LoadWorld(worldId);
    }

    public void Escape()
    {
        if (_state == State.Gameplay)
            GameplayController.Instance.Escape();
        else if (_state == State.MainMenu)
            MainMenuController.Instance.Escape();
    }

    // ------------ Other Functions ------------

    private async Task TransitionTo(State newState)
    {
        if (_state == State.Transitioning)
        {
            Debug.LogError($"[AppController] Cannot transition to state {newState} while transitioning.");
            return;
        }
        SetInputsActive(false);
        if (_state == State.MainMenu)
        {
            await MainMenuController.Instance.ExitMainMenu();
        }
        _state = State.Transitioning;
        Time.timeScale = 0f;
        await SceneServices.UnloadAllScenes();
        switch (newState)
        {
            case State.MainMenu:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                await SceneServices.LoadScene(_mainMenuScene);
                SetInputsActive(true);
                break;
            case State.Gameplay:
                Time.timeScale = 1f;
                await SceneServices.LoadScene(_gameplayScene);
                SetInputsActive(true);
                break;
            case State.Cutscene:
                Time.timeScale = 1f;
                await SceneServices.LoadScene(_cutsceneScene);
                break;
        }
        _state = newState;
        Debug.Log($"[AppController] Entered State {_state}");
    }

    // Inputs

    [Header("Inputs")]
    [SerializeField] private InputActionAsset _inputMap;
    public bool IsInputsActive { get; private set; } = true; // this is for security checking:
    /// <summary>
    /// apparently; if you click the same button twice during the input collection loop, 
    /// then it queues your second input for the next frame, 
    /// and disabling the inputs only happens at the end of the input loop, 
    /// meaning you can get double clicks when you think you shouldent :(
    /// </summary>

    public void SetInputsActive(bool isActive)
    {
        if (isActive) _inputMap.Enable();
        else _inputMap.Disable();
        IsInputsActive = isActive;
    }
}