using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public enum GameplayState
{
    NotStarted, StartOfGameCountdown, Playing, Pause
}

public class GameplayController : MonoBehaviour
{
    public static GameplayController Instance { get; private set; }
    
    public static Action AbilityUsageTick;
    public static Action GameplayTick;
    public static Action AbilitySpawnTick;
    public static float TickDuration;
    public static float MovementTime;
    public static float PauseTime;
    public static float BaseTickDuration;
    public static float BaseMovementTime;
    public static float BasePauseTime;

    [SerializeField] private EventsController _eventsController;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private TextMeshProUGUI _playerWonText;
    [SerializeField] private Transform _gridParent;
    [SerializeField] private GameObject[] _normalCellsPrefabs;
    [SerializeField] private GameObject[] _obstacleCellsPrefabs;
    [SerializeField] private GameObject _indestructiblePrefab;
    [SerializeField] private Transform _playersParent;
    [SerializeField] private GameObject[] _playersPrefabs;
    [SerializeField] private Transform _abilitiesCollectablesParent;
    public Transform _abilitiesUsablesParent;
    [SerializeField] private GameObject[] _abilityCollectablePrefabs;
    [SerializeField] private GameObject[] _abilityUsablePrefabs;
    [SerializeField] private GameObject _teleportPrefabBlue;
    [SerializeField] private GameObject _teleportPrefabOrange;
    [SerializeField] private Transform _teleportsParent;
    [SerializeField] private PlayerHud[] _playerHuds;
    [SerializeField] private GameObject _mainMenuCanvas;
    [SerializeField] private GameObject _gameplayCanvas;
    [SerializeField] private GameObject _winnerCanvas;
    [SerializeField] private RenderTexture[] _renderTextures;
    //[SerializeField] private GameObject _jumpParticles;


    private float _tickObjectiveTime;
    private float _endOfMovementObjectiveTime;
    private GridConfig[] _gridConfigs;
    private GameplayConfig _gameplayConfig;
    private GridManager _gridManager;
    private GameplayState _gameplayState;
    private float _startGameCountdownTimer;
    private static Pathfinding _pathfinding;
    private List<int> _playersDeadThisTurnIds;
    private List<int> _playersDeadThisGameIds;
    private Player playerAlive;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void CustomStart(GridConfig[] gridConfigs, GameplayConfig gameplayConfig)
    {
        _gridConfigs = gridConfigs;
        _gameplayConfig = gameplayConfig;
        BaseMovementTime = _gameplayConfig.MovementTimeMs * 0.001f;
        BasePauseTime = _gameplayConfig.PauseTimeMs * 0.001f;
        BaseTickDuration = MovementTime + PauseTime;
        MovementTime = BaseMovementTime;
        PauseTime = BasePauseTime;
        TickDuration = BaseTickDuration;
        _gridManager = new GridManager(_gridConfigs, _normalCellsPrefabs, _obstacleCellsPrefabs, _indestructiblePrefab, _gridParent);
        _gameplayState = GameplayState.NotStarted;
        DeactivatePlayerHuds();
        _eventsController.CustomStart(_gridManager, gameplayConfig);
        _pathfinding = new Pathfinding(_gridManager);
    }

    public void StartGame()
    {
        foreach (var hud in _playerHuds)
        {
            hud.Deactivate();
        }
        MovementTime /= CanvasController._speedMultiplier;
        PauseTime /= CanvasController._speedMultiplier;
        TickDuration = MovementTime + PauseTime;
        _mainMenuCanvas.SetActive(false);
        _gameplayCanvas.SetActive(true);
        GenerateMap();
        var spawnContents = GenerateSpawnPositions(_gridManager.Grid);
        SpawnPlayers(spawnContents);
        DestroyAbilities();
        SpawnAbilities(_gameplayConfig.MaxAbilitiesInMap);
        UpdateObjectiveTickTime();
        ChangeGameplayState(GameplayState.StartOfGameCountdown);
        AbilityUsageTick += () => { };
        GameplayTick += () => { };
        AbilitySpawnTick += () => { };
        _eventsController.Init(_gameplayConfig.TicksToStartEvent);
        _playersDeadThisGameIds = new List<int>();
        _playersDeadThisTurnIds = new List<int>();
    }

    public void CustomUpdate()
    {
        switch (_gameplayState)
        {
            case GameplayState.NotStarted:
                break;
            case GameplayState.StartOfGameCountdown:
                UpdateCountDownTimer();
                break;
            case GameplayState.Playing:
                if (Time.time >= _tickObjectiveTime)
                {
                    _eventsController.Tick();
                    AbilityUsageTick.Invoke();
                    GameplayTick.Invoke();
                    //CheckIfMapNeedsTeleport();
                    AbilitySpawnTick.Invoke();
                    UpdateObjectiveTickTime();
                    CheckIfGameEnded();
                }
                break;
            case GameplayState.Pause:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RestartGame()
    {
        MovementTime = BaseMovementTime / CanvasController._speedMultiplier;
        PauseTime = BasePauseTime / CanvasController._speedMultiplier;
        TickDuration = MovementTime + PauseTime;
        _gameplayState = GameplayState.NotStarted;
        _playersDeadThisGameIds.Clear();
        _playersDeadThisTurnIds.Clear();
        GenerateMap();
        DestroyAbilities();
        SpawnAbilities(_gameplayConfig.MaxAbilitiesInMap);
        UpdateObjectiveTickTime();
        ChangeGameplayState(GameplayState.StartOfGameCountdown);
        DestroyTeleports();
        var spawnContents = GenerateSpawnPositions(_gridManager.Grid);
        _eventsController.Reset();
        _eventsController._stormGameTotalTicks = 0;
        CanvasController.Instance._PreviewsPlayersObject.SetActive(false);
        for (var playerId = 0; playerId < GameManager.Players.Count; playerId++)
        {
            var player = GameManager.Players[playerId];
            player.Revive(spawnContents[playerId]);
        }
        foreach (var particle in CanvasController.Instance._endOfRoundParticles)
        {
            particle.SetActive(false);
        }
        foreach (var particle in CanvasController.Instance._endOfGameParticles)
        {
            particle.SetActive(false);
        }
    }

    private void GameOver()
    {
        _gameplayCanvas.SetActive(false);
        _winnerCanvas.SetActive(true);
        _winnerCanvas.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Player " + (playerAlive.playerId + 1);
        _winnerCanvas.transform.GetChild(2).GetChild(0).GetChild(1).GetComponent<RawImage>().texture = _renderTextures[playerAlive.playerId];
        GameManager.Instance.EndOfTournament();
        PlayEndOfGameAnimation(playerAlive.playerId);
    }

    private void GenerateMap()
    {
        _gridManager.InitMap();
    }

    private void SpawnPlayers(SpawnContent[] spawnContents)
    {
        DestroyPlayers();
        for (int i = 0; i < GameManager.NumberOfPlayers; i++)
        {
            var newI = i;
            var go = Instantiate(_playersPrefabs[i], spawnContents[newI].SpawnCell.PlayerWorldPosition.position, Quaternion.identity, _playersParent);
            GameManager.Players[newI] = go.GetComponent<Player>();
            GameManager.Players[newI].Init(
                spawnContents[newI].SpawnCell, 
                newI, 
                _gridManager, 
                this, 
                spawnContents[newI].SpawnDirection, 
                _playerHuds[newI]
            );
            spawnContents[newI].SpawnCell.ChangeCellContent(CellContent.Player);
            GameplayTick += GameManager.Players[newI].Move;
        }
    }

    public void SpawnAbilities(int numberOfAbilitiesToSpawn)
    {
        var possibleSpawns = new List<Vector2>();
        for (int i = 0; i < _gridManager.Grid.GetLength(0); i++)
        {
            for (int j = 0; j < _gridManager.Grid.GetLength(1); j++)
            {
                var currentCell = _gridManager.Grid[i, j];
                if (currentCell.CanSpawnAbility())
                {
                    var isDistant = true;
                    for (int playerIndex = 0; playerIndex < GameManager.Players.Count; playerIndex++)
                    {
                        if (!CellIsDistantToPlayer(currentCell, GameManager.Players[playerIndex], _gameplayConfig.AbilitiesToPlayersSpawnDistance))
                        {
                            isDistant = false;
                        }
                    }
                    if (isDistant)
                    {
                        possibleSpawns.Add(currentCell.gridPosition);
                    }
                }
            }
        }

        if (possibleSpawns.Count > 0)
        {
            for (int i = 0; i < numberOfAbilitiesToSpawn; i++)
            {
                var randomCellPos = possibleSpawns[UnityEngine.Random.Range(0, possibleSpawns.Count)];
                var cellToSpawn = _gridManager.Grid[(int) randomCellPos.x, (int) randomCellPos.y];
                var go = Instantiate(
                    _abilityCollectablePrefabs[UnityEngine.Random.Range(0, _abilityCollectablePrefabs.Length)],
                    cellToSpawn.PlayerWorldPosition.position,
                    Quaternion.identity,
                    _abilitiesCollectablesParent
                );
                cellToSpawn.ChangeCellContent(CellContent.Collectable);
                var index = GetAbilityIndexForTypeAndUsable();
                var abilityCollectable = go.GetComponent<ICollectable>();
                abilityCollectable.Spawn(
                    _gridManager.Grid[(int) randomCellPos.x, (int) randomCellPos.y],
                    this, (AbilityType)index, _abilityUsablePrefabs[index] //make the type and gameobject be decided randomly
                );
                _gridManager.Grid[(int) randomCellPos.x, (int) randomCellPos.y].CollectableOnTop = abilityCollectable;
                possibleSpawns.Remove(randomCellPos);
            }
        }
        else
        {
            Debug.Log("NO SPACE FOR ABILITIES");
        }
    }

    private int GetAbilityIndexForTypeAndUsable()
    {
        var random = Random.Range(0, 100.0f);
        if (random <= _gameplayConfig.BombSpawnProbability)
        {
            //bomb
            return 0;
        }

        if (random <= _gameplayConfig.BombSpawnProbability + _gameplayConfig.MissileSpawnProbability)
        {
            //missile
            return 1;
        }
        
        if (random <= _gameplayConfig.BombSpawnProbability + _gameplayConfig.MissileSpawnProbability + _gameplayConfig.MineSpawnProbability)
        {
            //mine
            return 2;
        }
        
        Debug.LogError($"ERROR AT DECIDING BOOST, RANDOM IS {random}");
        return -1;
    }

    private bool CellIsDistantToPlayer(Cell currentCell, Player player, int distance)
    {
        return (((Math.Abs(currentCell.gridPosition.x - player.currentCell.gridPosition.x) +
                 Mathf.Abs(currentCell.gridPosition.y - player.currentCell.gridPosition.y)) >= distance) &&
                ((Math.Abs(currentCell.gridPosition.x - player.nextCell.gridPosition.x) +
                  Mathf.Abs(currentCell.gridPosition.y - player.nextCell.gridPosition.y)) >= distance));
    }

    public void ChangeGameplayState(GameplayState newGameplayState)
    {
        switch (_gameplayState)
        {
            case GameplayState.NotStarted:
                break;
            case GameplayState.StartOfGameCountdown:
                DisableStartOfGameCountdownTimer();
                break;
            case GameplayState.Playing:
                break;
            case GameplayState.Pause:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        switch (newGameplayState)
        {
            case GameplayState.NotStarted:
                break;
            case GameplayState.StartOfGameCountdown:
                StartCountdownTimer();
                break;
            case GameplayState.Playing:
                break;
            case GameplayState.Pause:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newGameplayState), newGameplayState, null);
        }
        _gameplayState = newGameplayState;
    }

    private void CheckIfGameEnded()
    {
        if (AllPlayersDead())
        {
            ChangeGameplayState(GameplayState.NotStarted);
            // TODO: add draw action. (Recorrer _playersDeadThisTurnIds per saber quins han empatat).
            _countdownText.gameObject.SetActive(true);
            _countdownText.text = "DRAW";
            Debug.Log("DRAW");
            foreach (var player in GameManager.Players)
            {
                GameplayTick -= player.Move;
            }
            _playerWonText.text = "NO ONE";
            CanvasController.Instance._PreviewsPlayersObject.SetActive(true);
            Invoke(nameof(EndOfRound), 1.0f);
            SoundManager.Instance.PlayEvent("Draw");
            Invoke(nameof(PlayDrawAnimation), 1.2f);
        }
        else if (OnePlayerAlive())
        {
            ChangeGameplayState(GameplayState.NotStarted);
            playerAlive = GameManager.Players.Find(player => !player.IsDead());
            GameManager.Players[playerAlive.playerId].GameWon();
            foreach (var player in GameManager.Players)
            {
                GameplayTick -= player.Move;
            }
            CanvasController.Instance._PreviewsPlayersObject.SetActive(true);
            if (playerAlive._gamesWon == CanvasController._numberOfWins)
            {
                Invoke(nameof(GameOver), 1.0f);
            }
            else
            {
                _playerWonText.text = "PLAYER " + (playerAlive.playerId + 1);
                Invoke(nameof(PlayAnimation), 1.2f);
                Invoke(nameof(EndOfRound), 1.0f);
            }
        }
        _playersDeadThisTurnIds.Clear();
    }

    private void PlayAnimation()
    {
        PlayerWinAnimation(playerAlive.playerId);
    }

    private void PlayDrawAnimation()
    {
        PlayerWinAnimation(-1);
    }

    private void PlayerWinAnimation(int playerAlivePlayerId = -1)
    {
        switch (playerAlivePlayerId)
        {
            case -1:
                CanvasController.Instance._playersPreviewsAnimators[0].Play("Loose1");
                CanvasController.Instance._playersPreviewsAnimators[1].Play("Loose2");
                CanvasController.Instance._playersPreviewsAnimators[2].Play("Loose3");
                CanvasController.Instance._playersPreviewsAnimators[3].Play("Loose4");
                break;
            case 0:
                CanvasController.Instance._playersPreviewsAnimators[playerAlivePlayerId].Play("WinUnicorn");
                CanvasController.Instance._playersPreviewsAnimators[1].Play("Loose2");
                CanvasController.Instance._playersPreviewsAnimators[2].Play("Loose3");
                CanvasController.Instance._playersPreviewsAnimators[3].Play("Loose4");
                CanvasController.Instance._endOfRoundParticles[playerAlivePlayerId].SetActive(true);
                break;
            case 1:
                CanvasController.Instance._playersPreviewsAnimators[0].Play("Loose1");
                CanvasController.Instance._playersPreviewsAnimators[playerAlivePlayerId].Play("WinBurguer");
                CanvasController.Instance._playersPreviewsAnimators[2].Play("Loose3");
                CanvasController.Instance._playersPreviewsAnimators[3].Play("Loose4");
                CanvasController.Instance._endOfRoundParticles[playerAlivePlayerId].SetActive(true);
                break;
            case 2:
                CanvasController.Instance._playersPreviewsAnimators[0].Play("Loose1");
                CanvasController.Instance._playersPreviewsAnimators[1].Play("Loose2");
                CanvasController.Instance._playersPreviewsAnimators[playerAlivePlayerId].Play("WinRobot");
                CanvasController.Instance._playersPreviewsAnimators[3].Play("Loose4");
                CanvasController.Instance._endOfRoundParticles[playerAlivePlayerId].SetActive(true);
                break;
            case 3:
                CanvasController.Instance._playersPreviewsAnimators[0].Play("Loose1");
                CanvasController.Instance._playersPreviewsAnimators[1].Play("Loose2");
                CanvasController.Instance._playersPreviewsAnimators[2].Play("Loose3");
                CanvasController.Instance._playersPreviewsAnimators[playerAlivePlayerId].Play("WinCupcake");
                CanvasController.Instance._endOfRoundParticles[playerAlivePlayerId].SetActive(true);
                break;
        }
    }
    
    private void PlayEndOfGameAnimation(int winnerId)
    {
        switch (winnerId)
        {
            case 0:
                CanvasController.Instance._playersPreviewsAnimators[winnerId].Play("WinUnicorn");
                CanvasController.Instance._endOfGameParticles[winnerId].SetActive(true);
                break;
            case 1:
                CanvasController.Instance._playersPreviewsAnimators[winnerId].Play("WinBurguer");
                CanvasController.Instance._endOfGameParticles[winnerId].SetActive(true);
                break;
            case 2:
                CanvasController.Instance._playersPreviewsAnimators[winnerId].Play("WinRobot");
                CanvasController.Instance._endOfGameParticles[winnerId].SetActive(true);
                break;
            case 3:
                CanvasController.Instance._playersPreviewsAnimators[winnerId].Play("WinCupcake");
                CanvasController.Instance._endOfGameParticles[winnerId].SetActive(true);
                break;
        }
    }

    private bool AllPlayersDead()
    {
        var deadPlayers = 0;
        foreach (var player in GameManager.Players)
        {
            if (player._playerMovementState == PlayerMovementState.Dead)
            {
                deadPlayers++;
            }
        }
        return deadPlayers == GameManager.Players.Count;
    }
    
    private bool OnePlayerAlive()
    {
        var alivePlayers = 0;
        foreach (var player in GameManager.Players)
        {
            if (player._playerMovementState != PlayerMovementState.Dead)
            {
                alivePlayers++;
            }
        }
        return alivePlayers == 1;
    }

    public void PlayerDied(int deadPlayerId)
    {
        _playersDeadThisTurnIds.Add(deadPlayerId);
        _playersDeadThisGameIds.Add(deadPlayerId);
    }

    private void EndOfRound()
    {
        GameManager.Instance.ChangeState(GameState.EndOfRound);
    }

    private void DisableStartOfGameCountdownTimer()
    {
        _countdownText.gameObject.SetActive(false);
    }

    private void StartCountdownTimer()
    {
        _startGameCountdownTimer = 3.0f;
        UpdateCountDownTimer();
        _countdownText.gameObject.SetActive(true);
    }

    private void UpdateCountDownTimer()
    {
        _startGameCountdownTimer -= Time.deltaTime;
        _countdownText.text = ((int) (_startGameCountdownTimer + 1.0f)).ToString();
        if (_startGameCountdownTimer <= 0.0f)
        {
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.StartGong);
            ChangeGameplayState(GameplayState.Playing);
        }
    }

    private void UpdateObjectiveTickTime()
    {
        _tickObjectiveTime = Time.time + TickDuration;
    }

    private void DestroyPlayers()
    {
        for (int i = 0; i < _playersParent.childCount; i++)
        {
            Destroy(_playersParent.GetChild(i).gameObject);
        }
    }

    private void DestroyAbilities()
    {
        for (int i = 0; i < _abilitiesCollectablesParent.transform.childCount; i++)
        {
            _abilitiesCollectablesParent.GetChild(i).GetComponent<ICollectable>().Destroy();
        }
        for (int i = 0; i < _abilitiesUsablesParent.transform.childCount; i++)
        {
            _abilitiesUsablesParent.GetChild(i).GetComponent<IAbility>().Destroy();
        }
    }

    private void DestroyTeleports()
    {
        for (int i = 0; i < _teleportsParent.transform.childCount; i++)
        {
            _teleportsParent.GetChild(i).GetComponentInChildren<Teleport>().DestroyAsSecond();
        }
    }

    private SpawnContent[] GenerateSpawnPositions(Cell[,] grid)
    {
        var spawnContents = new[]
        {
            new SpawnContent(grid[1, 1], Direction.Right),
            new SpawnContent(grid[1, grid.GetLength(1) - 2], Direction.Left),
            new SpawnContent(grid[grid.GetLength(0) - 2, 1], Direction.Right),
            new SpawnContent(grid[grid.GetLength(0) - 2, grid.GetLength(1) - 2], Direction.Left)
        };
        return spawnContents;
    }

    private void DeactivatePlayerHuds()
    {
        foreach (var playerHud in _playerHuds)
        {
            playerHud.Deactivate();
        }
    }

    public void CheckIfMapNeedsTeleport()
    {
        if (Pathfinding.TeleportInstantiated)
        {
            return;
        }
        if (_gameplayState != GameplayState.Playing)
        {
            return;
        }
        
        var mapIsBroken = false;
        var i = 0;
        for (i = 0; i < GameManager.NumberOfPlayers - 1 && !mapIsBroken; i++)
        {
            if (_pathfinding.FindPath(GameManager.Players[i].currentCell.gridPosition,
                    GameManager.Players[i + 1].currentCell.gridPosition) == null)
            {
                mapIsBroken = true;
            }
        }

        i--;

        if (mapIsBroken)
        {
            var possibleSpawnsForPlayer1 = new List<Vector2>();
            GetTeleportPositions(GameManager.Players[i].currentCell.gridPosition, ref possibleSpawnsForPlayer1);
            var possibleSpawnsForPlayer2 = new List<Vector2>();
            GetTeleportPositions(GameManager.Players[i + 1].currentCell.gridPosition, ref possibleSpawnsForPlayer2);
            if (possibleSpawnsForPlayer1.Count > 0 && possibleSpawnsForPlayer2.Count > 0)
            {
                SpawnTeleports(possibleSpawnsForPlayer1, possibleSpawnsForPlayer2);
                Pathfinding.TeleportInstantiated = true;
            }
        }
    }

    private void GetTeleportPositions(Vector2 position, ref List<Vector2> possibleSpawns)
    {
        var positionsToCheck = new List<Vector2>();

        if (GridManager.IsInMap(position + Vector2.up))
        {
            var positionToCheck = position + Vector2.up;
            if (_gridManager.Grid[(int) positionToCheck.x, (int) positionToCheck.y].CanSpawnTeleport())
            {
                if (!possibleSpawns.Contains(positionToCheck))
                {
                    positionsToCheck.Add(positionToCheck);
                    possibleSpawns.Add(positionToCheck);
                }
            }
        }
        if (GridManager.IsInMap(position + Vector2.down))
        {
            var positionToCheck = position + Vector2.down;
            if (_gridManager.Grid[(int) positionToCheck.x, (int) positionToCheck.y].CanSpawnTeleport())
            {
                if (!possibleSpawns.Contains(positionToCheck))
                {
                    positionsToCheck.Add(positionToCheck);
                    possibleSpawns.Add(positionToCheck);
                }
            }
        }
        if (GridManager.IsInMap(position + Vector2.right))
        {
            var positionToCheck = position + Vector2.right;
            if (_gridManager.Grid[(int) positionToCheck.x, (int) positionToCheck.y].CanSpawnTeleport())
            {
                if (!possibleSpawns.Contains(positionToCheck))
                {
                    positionsToCheck.Add(positionToCheck);
                    possibleSpawns.Add(positionToCheck);
                }
            }
        }
        if (GridManager.IsInMap(position + Vector2.left))
        {
            var positionToCheck = position + Vector2.left;
            if (_gridManager.Grid[(int) positionToCheck.x, (int) positionToCheck.y].CanSpawnTeleport())
            {
                if (!possibleSpawns.Contains(positionToCheck))
                {
                    positionsToCheck.Add(positionToCheck);
                    possibleSpawns.Add(positionToCheck);
                }
            }
        }

        foreach (var positionToCheck in positionsToCheck)
        {
            GetTeleportPositions(positionToCheck, ref possibleSpawns);
        }
    }

    private void SpawnTeleports(List<Vector2> possibleSpawnsForPlayer1, List<Vector2> possibleSpawnsForPlayer2)
    {
        var player1teleportposition = possibleSpawnsForPlayer1[Random.Range(0, possibleSpawnsForPlayer1.Count - 1)];
        var player2teleportposition = possibleSpawnsForPlayer2[Random.Range(0, possibleSpawnsForPlayer2.Count - 1)];

        var cell1 = _gridManager.Grid[(int) player1teleportposition.x, (int) player1teleportposition.y];
        var cell2 = _gridManager.Grid[(int) player2teleportposition.x, (int) player2teleportposition.y];

        var teleport1Object = Instantiate(_teleportPrefabBlue, cell1.transform.position + Vector3.up, Quaternion.identity, _teleportsParent);
        var teleport2Object = Instantiate(_teleportPrefabOrange, cell2.transform.position + Vector3.up, Quaternion.identity, _teleportsParent);

        var teleport1 = teleport1Object.GetComponentInChildren<Teleport>();
        var teleport2 = teleport2Object.GetComponentInChildren<Teleport>();

        cell1.OnTopTeleport = teleport1;
        cell2.OnTopTeleport = teleport2;

        teleport1.Init(cell1, cell2);
        teleport2.Init(cell2, cell1);

    }
}