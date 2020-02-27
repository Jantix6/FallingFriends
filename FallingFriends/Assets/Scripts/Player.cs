using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Cell currentCell;
    public Cell nextCell;
    public int playerId;
    
    public PlayerMovementState _playerMovementState;
    
    private GridManager _gridManager;
    private GameplayController _gameplayController;
    private InputController _inputController;
    private float _currentDistance;
    public AbilityType _currentAbilityType;
    public GameObject _currentAbilityUsable;
    private GameObject _abilityUsableWaitingForSpawn;
    private AbilityType _abilityTypeWaitingForSpawn;
    private Cell _cellWaitingForAbilityToSpawn;
    private bool _canUseAbilityThisTurn;
    private PlayerHud _playerHud;
    [HideInInspector] public int _gamesWon;
    public bool _haveToPlayWithKeyboard;
    public Teleport _teleport;
    public int _directionController;
    private Animator _gameplayAnimator;
    private Animator _headAnimator;
    [SerializeField] private AnimationCurve _animationCurve;
    public GameObject _invertedControlsParticles;
    [SerializeField] private GameObject _jumpGrassParticles;
    [SerializeField] private GameObject _jumpSandParticles;
    [SerializeField] private GameObject _jumpGenericParticles;

    public void Init(
        Cell currentCell,
        int playerId,
        GridManager gridManager,
        GameplayController gameplayController,
        Direction direction,
        PlayerHud playerHud
    )
    {
        _haveToPlayWithKeyboard = CanvasController._playingKeyboard;
        _playerMovementState = PlayerMovementState.Idle;
        this.playerId = playerId;
        this.currentCell = currentCell;
        nextCell = this.currentCell;
        _gridManager = gridManager;
        _gameplayController = gameplayController;
        _inputController = GetComponent<InputController>();
        _inputController.CustomStart(direction);
        _currentDistance = 0.0f;
        _currentAbilityType = AbilityType.None;
        _playerHud = playerHud;
        playerHud.Init(playerId, this);
        _gamesWon = 0;
        _directionController = 1;
        _inputController.FaceToDirection();
        _teleport = null;
        _gameplayAnimator = GetComponentInChildren<Animator>();
    }

    public void CustomUpdate()
    {
        switch (_playerMovementState)
        {
            case PlayerMovementState.Idle:
                _inputController.UpdateMovementInput(playerId, false);
                _inputController.UpdateAbilityInputs(playerId);
                _gameplayAnimator.Play("SimpleIdle");
                break;
            case PlayerMovementState.Moving:
                _gameplayAnimator.Play("Jump");
                _currentDistance += Time.deltaTime;
                transform.position = Vector3.Lerp(currentCell.PlayerWorldPosition.position,
                    nextCell.PlayerWorldPosition.position, _currentDistance / GameplayController.MovementTime);
                var progress = _animationCurve.Evaluate(_currentDistance / GameplayController.MovementTime);
                _gameplayAnimator.SetFloat("CurveValue", progress);
                if (HasArrivedToCell())
                {
                    ActivateGroundParticles();
                    currentCell.ChangeCellContent(CellContent.Empty);
                    if (_teleport != null)
                    {
                        _teleport.TeleportToCell();
                    }
                    UpdateCurrentCell();
                    _currentDistance = 0.0f;
                    ChangeState(PlayerMovementState.Idle);
                    if (_abilityTypeWaitingForSpawn == AbilityType.Missile)
                    {
                        Invoke(nameof(SpawnAbilityObject), 0.1f);
                    }
                }
                else
                {
                    _inputController.UpdateMovementInput(playerId, true);
                    _inputController.UpdateAbilityInputs(playerId);
                }
                break;
            case PlayerMovementState.Dead:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ActivateGroundParticles()
    {
        if (currentCell.transform.parent.CompareTag("Grass"))
        {
            _jumpGrassParticles.SetActive(true);
            _jumpSandParticles.SetActive(false);
            _jumpGenericParticles.SetActive(false);
        }
        else if (currentCell.transform.parent.CompareTag("Sand"))
        {
            _jumpGrassParticles.SetActive(false);
            _jumpSandParticles.SetActive(true);
            _jumpGenericParticles.SetActive(false);
        }
        else
        {
            _jumpGrassParticles.SetActive(false);
            _jumpSandParticles.SetActive(false);
            _jumpGenericParticles.SetActive(true);
        }
    }

    public void ChangeState(PlayerMovementState newState)
    {
        switch (_playerMovementState)
        {
            case PlayerMovementState.Idle:
                break;
            case PlayerMovementState.Moving:
                break;
            case PlayerMovementState.Dead:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        switch (newState)
        {
            case PlayerMovementState.Idle:
                break;
            case PlayerMovementState.Moving:
                _currentDistance = 0.0f;
                break;
            case PlayerMovementState.Dead:
                _invertedControlsParticles.SetActive(false);
                _jumpGrassParticles.SetActive(false);
                _jumpSandParticles.SetActive(false);
                _jumpGenericParticles.SetActive(false);
                _playerMovementState = newState;
                GameplayController.GameplayTick -= Move;
                _playerHud.Deactivate();
                _gameplayController.PlayerDied(playerId);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _playerMovementState = newState;
    }

    public void Move()
    {
        var result = _gridManager.GetNewCellAccordingToDirectionAndDistance(currentCell, _inputController.direction);
        nextCell = result.Cell;
        // TODO: Check if next cell is another type of cell, like an empty cell, or obstacle, etc.

        switch (result.Message)
        {
            case "Walkable":
                nextCell.ChangeCellContent(CellContent.Player);
                ChangeState(PlayerMovementState.Moving);
                ResetAbilityUsage();
                break;
            case "Obstacle":
                ResetAbilityUsage();
                break;
            case "Destroyed":
                ChangeState(PlayerMovementState.Dead);
                _gameplayAnimator.Play("DeathFallFront");
                ResetAbilityUsage();
                break;
            case "OutsideMap":
                DieInFront();
                ResetAbilityUsage();
                break;
        }
    }

    public void Revive(SpawnContent spawnContent)
    {
        gameObject.SetActive(true);
        enabled = true;
        GameplayController.GameplayTick += Move;
        currentCell = spawnContent.SpawnCell;
        nextCell = currentCell;
        _inputController.direction = spawnContent.SpawnDirection;
        _currentAbilityType = AbilityType.None;
        _currentAbilityUsable = null;
        _abilityTypeWaitingForSpawn = AbilityType.None;
        _abilityUsableWaitingForSpawn = null;
        ChangeState(PlayerMovementState.Idle);
        transform.position = currentCell.PlayerWorldPosition.position;
        currentCell.ChangeCellContent(CellContent.Player);
        _playerHud.Reactivate();
        _inputController.FaceToDirection();
        _inputController.storedDirection = _inputController.direction;
        _teleport = null;
        _directionController = 1;
        _invertedControlsParticles.SetActive(false);
        _playerHud.SetCurrentAbility();
        _jumpGrassParticles.SetActive(false);
        _jumpSandParticles.SetActive(false);
        _jumpGenericParticles.SetActive(false);
    }

    private void ResetAbilityUsage()
    {
        _canUseAbilityThisTurn = true;
    }

    public void RewardAbility(AbilityType abilityType, GameObject abilityUsable)
    {
        _currentAbilityType = abilityType;
        _currentAbilityUsable = abilityUsable;
        _playerHud.SetCurrentAbility(abilityType);
        _playerHud.PlayTakeAbilityAnimation();
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.PickUpAbility);
    }

    private void UseAbility()
    {
        _abilityTypeWaitingForSpawn = _currentAbilityType;
        _abilityUsableWaitingForSpawn = _currentAbilityUsable;
        _currentAbilityType = AbilityType.None;
        _currentAbilityUsable = null;
        _canUseAbilityThisTurn = false;
        _playerHud.SetCurrentAbility(_currentAbilityType);
        switch (_abilityTypeWaitingForSpawn)
        {
            case AbilityType.Bomb:
                _cellWaitingForAbilityToSpawn = currentCell;
                GameplayController.AbilityUsageTick += SpawnAbilityObject;
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.DropAbilty);
                break;
            case AbilityType.Missile:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.MissileThrow);
                break;
            case AbilityType.Mine:
                _cellWaitingForAbilityToSpawn = currentCell;
                GameplayController.AbilityUsageTick += SpawnAbilityObject;
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.DropAbilty);
                break;
            case AbilityType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SpawnAbilityObject()
    {
        switch (_abilityTypeWaitingForSpawn)
        {
            case AbilityType.Bomb:
                var bombObject = Instantiate(
                    _abilityUsableWaitingForSpawn, 
                    _cellWaitingForAbilityToSpawn.PlayerWorldPosition.position, 
                    Quaternion.identity, 
                    _gameplayController._abilitiesUsablesParent
                );
                var bombAbility = bombObject.GetComponent<IAbility>();
                bombAbility.Init(_cellWaitingForAbilityToSpawn, _gridManager, playerId);
                GameplayController.AbilityUsageTick -= SpawnAbilityObject;
                break;
            case AbilityType.Missile:
                var missileObject = Instantiate(
                    _abilityUsableWaitingForSpawn, 
                    currentCell.PlayerWorldPosition.position, 
                    transform.rotation, 
                    _gameplayController._abilitiesUsablesParent
                );
                var missileAbility = missileObject.GetComponent<IAbility>();
                missileAbility.Init(currentCell, _gridManager, playerId);
                break;
            case AbilityType.Mine:
                var mineObject = Instantiate(
                    _abilityUsableWaitingForSpawn, 
                    _cellWaitingForAbilityToSpawn.PlayerWorldPosition.position, 
                    Quaternion.identity, 
                    _gameplayController._abilitiesUsablesParent
                );
                var mine = mineObject.GetComponent<IAbility>();
                var spawnedOnPlayer = currentCell.gridPosition == _cellWaitingForAbilityToSpawn.gridPosition;
                mine.Init(_cellWaitingForAbilityToSpawn, _gridManager, playerId);
                GameplayController.AbilityUsageTick -= SpawnAbilityObject;
                break;
            case AbilityType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _abilityTypeWaitingForSpawn = AbilityType.None;
        _abilityUsableWaitingForSpawn = null;
    }

    public bool CanUseAbilitiesThisTurn()
    {
        return _canUseAbilityThisTurn;
    }

    private bool HasArrivedToCell()
    {
        return _currentDistance / GameplayController.MovementTime >= 1.0f;
    }
    
    private void UpdateCurrentCell()
    {
        currentCell = nextCell;
    }

    public bool IsDead()
    {
        return _playerMovementState == PlayerMovementState.Dead;
    }

    public void GameWon()
    {
        _gamesWon++;
        _playerHud.SetWins(_gamesWon);
    }

    public void InputAbility()
    {
        UseAbility();
    }

    public void Die()
    {
        ChangeState(PlayerMovementState.Dead);
    }

    public void DieByCell()
    {
        _gameplayAnimator.Play("DeathFall");
        ChangeState(PlayerMovementState.Dead);

        switch (playerId)
        {
            case 0:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.UnicornDeath);
                break;
            case 1:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.HamburgerDeath);
                break;
            case 2:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.RobotDeath);
                break;
            case 3:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.CupcakeDeath);
                break;
        }
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.WaterSplash);
    }

    public void DieInFront()
    {
        _gameplayAnimator.Play("DeathFallFront");
        ChangeState(PlayerMovementState.Dead);
        
        switch (playerId)
        {
            case 0:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.UnicornDeath);
                break;
            case 1:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.HamburgerDeath);
                break;
            case 2:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.RobotDeath);
                break;
            case 3:
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.CupcakeDeath);
                break;
        }
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.WaterSplash);
    }
}