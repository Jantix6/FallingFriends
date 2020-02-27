using System;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GridConfigScriptable[] _gridConfigScriptables;

    [SerializeField] private GameplayConfigScriptable _gameplayConfigScriptable;

    [SerializeField] private GameplayController _gameplayController;

    [SerializeField] private CameraMovementController _cameraMovementController;

    //public static Player[] Players;
    public static List<Player> Players;
    public static int NumberOfPlayers;
    public GameState GameState;
    public Dropdown _playersDropdown;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        GameState = GameState.StartScreen;
        var gridConfigs = new GridConfig[_gridConfigScriptables.Length];
        for (var index = 0; index < _gridConfigScriptables.Length; index++)
        {
            gridConfigs[index] = _gridConfigScriptables[index].GridConfig;
        }
        _gameplayController.CustomStart(gridConfigs, _gameplayConfigScriptable.GameplayConfig);
        _playersDropdown.onValueChanged.AddListener(UpdatePlayers);
        SoundManager.Instance.PlayEvent("MenuMusic");
    }

    void Update()
    {
        switch (GameState)
        {
            case GameState.StartScreen:
                CanvasController.Instance.StartScreenUpdate();
                break;
            case GameState.MainMenu:
                break;
            case GameState.PlayersScreen:
                CanvasController.Instance.PlayersPreviewScreenUpdate();
                break;
            case GameState.Playing:
                _gameplayController.CustomUpdate();
                foreach (var player in Players)
                {
                    player.CustomUpdate();
                }
                _cameraMovementController.CustomUpdate();
                break;
            case GameState.EndOfRound:
                CanvasController.Instance.EndRoundScreenUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
#if UNITY_EDITOR
        TestingUtils();
#endif      
    }

    public void StartGame()
    {
        UpdatePlayers(_playersDropdown.value);
        ChangeState(GameState.Playing);
        _gameplayController.StartGame();
        _cameraMovementController.CustomStart();
    }

    private void UpdatePlayers(int value)
    {
        var playersArray = new Player[value + 2];
        Players = playersArray.ToList();
        NumberOfPlayers = Players.Count;
        Debug.Log($"Players set to {NumberOfPlayers}");
    }

    public void ChangeState(GameState newState)
    {
        switch (GameState)
        {
            case GameState.StartScreen:
                CanvasController.Instance._startScreenCanvas.SetActive(false);
                break;
            case GameState.MainMenu:
                CanvasController.Instance._mainMenuScreenCanvas.SetActive(false);
                break;
            case GameState.PlayersScreen:                
                break;
            case GameState.Playing:
                break;
            case GameState.EndOfRound:
                CanvasController.Instance._endRoundCanvas.SetActive(false);
                CanvasController.Instance.ResetReadyPlayerState();
                SoundManager.Instance.StopEvent("WinRound");
                SoundManager.Instance.StopEvent("Draw");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (newState)
        {
            case GameState.StartScreen:
                CanvasController.Instance._startScreenCanvas.SetActive(true);
                break;
            case GameState.MainMenu:
                CanvasController.Instance._mainMenuScreenCanvas.SetActive(true);
                break;
            case GameState.PlayersScreen:
                foreach (var particle in CanvasController.Instance._endOfRoundParticles)
                {
                    particle.SetActive(false);
                }
                foreach (var particle in CanvasController.Instance._endOfGameParticles)
                {
                    particle.SetActive(false);
                }
                break;
            case GameState.Playing:
                SoundManager.Instance.StopEvent("MenuMusic");
                switch (CanvasController._speedMultiplier)
                {
                    case 0.7f:
                        SoundManager.Instance.PlayEvent("EasyDifficult");
                        break;
                    case 1.0f:
                        SoundManager.Instance.PlayEvent("MediumDifficult");
                        break;
                    case 1.3f:
                        SoundManager.Instance.PlayEvent("HardDifficult");
                        break;
                }
                CanvasController.Instance.ResetReadyPlayerState();
                GameState = GameState.Playing;
                break;
            case GameState.EndOfRound:
                SoundManager.Instance.StopAllEvents();
                SoundManager.Instance.PlayEvent("WinRound");
                CanvasController.Instance._endRoundCanvas.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        GameState = newState;
    }

    public void ChangeStateForNumber(int nextChange)
    {
        switch (nextChange)
        {
            case 2:
                ChangeState(GameState.MainMenu);
                break;
            case 3:
                ChangeState(GameState.PlayersScreen);
                break;
            case 4:
                ChangeState(GameState.Playing);
                break;
        }
    }

    private void TestingUtils()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            Time.timeScale = 3.0f;
        }
        else
        {
            if (Input.GetKey(KeyCode.X))
            {
                Time.timeScale = 0.2f;
            }
            else
            {
                Time.timeScale = 1.0f;
            }
        }
    }

    public void EndOfTournament()
    {
        foreach (var particle in CanvasController.Instance._endOfRoundParticles)
        {
            particle.SetActive(false);
        }
        foreach (var particle in CanvasController.Instance._endOfGameParticles)
        {
            particle.SetActive(false);
        }
        ChangeState(GameState.MainMenu);
        _cameraMovementController.gameObject.transform.position =
            _cameraMovementController._initCameraPosition.position;
        CanvasController.Instance.SwitchCanvasButtonController(CanvasController._buttonForSwitchCanvasControl);
        
        SoundManager.Instance.StopAllEvents();
        SoundManager.Instance.PlayEvent("WinMusic");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if UNITY_STANDALONE_WIN
        Application.Quit();
#endif
    }
}