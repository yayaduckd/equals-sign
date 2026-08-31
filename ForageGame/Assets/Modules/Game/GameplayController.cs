using UnityEngine;
using Project.Menus;
using TDK.SaveSystem;
using System.Threading.Tasks;
using TDK.SceneSystem;
using Eflatun.SceneReference;
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
        SetGameState(State.Transitioning);

        await Task.Delay(Mathf.CeilToInt(1 * 1000)); //animation (1 sec.)

        await _tsc.FadeOutAsync();
        await AwaitPadding();

        Player.Instance.gameObject.SetActive(false);
        Player.Instance.transform.position = new(73, 11.5f, 75);

        SaveManager.Instance.SaveWorld();
        await SceneServices.UnloadScene(_worldScene);
        await SceneServices.LoadScene(_worldScene);
        Player.Instance.gameObject.SetActive(true);
        SaveManager.Instance.LoadWorld();


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
        _saveManager.SelectWorld(worldId);
        _saveManager.LoadWorld();

        ///IMPORTANT: scene loading isn't actually fully 'done' at this point
        /// Awake() and OnEnable() have been run, but physics and terrain stuff come later
        /// That's why padding is required, but we shouldn't have a time,
        /// Rather frames (loading frames are slow rememba)
        /// This should ensure terrains are actually loaded in time before we start
        /// ~Lars
        await Task.Yield(); // end of current frame
        await Task.Yield(); // end of next frame (physics runs here)

        //await WaitForTerrainsReady();
        Debug.Log("[GameplayController]: Terrains are ready! starting in 100ms");

        await AwaitPadding();
        _tsc.FadeIn();
        SetGameState(State.Playing);
    }

    /// <summary>
    /// Because scene loading being done does not mean scene loading is actually done
    /// Now goes unused, but might be required later once loading times increase when cave is put in ther world too.
    /// </summary>
    /// <returns></returns>
    private async Task WaitForTerrainsReady()
    {
        // Give Unity's terrain system a frame to register colliders
        await Task.Yield();

        var terrains = GameObject.FindObjectsByType<Terrain>(FindObjectsSortMode.None);

        float timeout = 10f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            bool allReady = true;

            foreach (var terrain in terrains)
            {
                var tc = terrain.GetComponent<TerrainCollider>();
                if (tc == null || !tc.enabled)
                {
                    allReady = false;
                    break;
                }

                // Check that terrain data is actually populated
                if (terrain.terrainData == null ||
                    terrain.terrainData.alphamapWidth == 0)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady) return;

            elapsed += Time.deltaTime;
            await Task.Yield();
        }

        Debug.LogWarning("WaitForTerrainsReady timed out after 10s — proceeding anyway.");
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

