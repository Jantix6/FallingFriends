using UnityEngine;

public class MineAbility : MonoBehaviour, IAbility
{
    [SerializeField] private GameObject _explosionPrefab;
    private Cell _spawnCell;
    private const int TICKSTOEXPLODE = 10;
    private const int EXPLOSIONDISTANCE = 0;
    private int _currentTicksToExplode;
    private GridManager _gridManager;
    private int _playerId;
    private GameplayController _gameplayController;

    public void Init(Cell spawnCell, GridManager gridManager, int playerId)
    {
        //GameplayController.AbilityUsageTick += Tick;
        Invoke(nameof(Explode), 10.0f);
        _spawnCell = spawnCell;
        _currentTicksToExplode = 0;
        _gridManager = gridManager;
        _playerId = playerId;
    }

    public void Tick()
    {
        
    }

    private void Explode()
    {
        CameraShake.Instance.Shake();
        for (int distance = 0; distance < EXPLOSIONDISTANCE; distance++)
        {
            for (int directionNum = 0; directionNum < 4; directionNum++)
            {
                var dir = (Direction) directionNum;
                var result = _gridManager.GetNewCellAccordingToDirectionAndDistance(_spawnCell, dir, distance + 1);
                var cellObjective = result.Cell;
                if (cellObjective.cellState == CellState.Walkable)
                {
                    var explosion = Instantiate(_explosionPrefab, cellObjective.PlayerWorldPosition.position, Quaternion.identity);
                    Destroy(explosion, GameplayController.TickDuration * 0.9f); // This should be an animation or effect.
                    cellObjective.AutoDestroy();
                }
            }
        }
        var centerExplosion = Instantiate(_explosionPrefab, _spawnCell.PlayerWorldPosition.position, Quaternion.identity);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.LandmineExplosion);
        Destroy(centerExplosion, GameplayController.TickDuration * 0.9f); // This should be an animation or effect.
        _spawnCell.AutoDestroy();
        Destroy();
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player == null)
            return;
        
        if (player.playerId == _playerId)
        {
            return;
        }
        Explode();
    }

    public void Destroy()
    {
        GameplayController.AbilityUsageTick -= Tick;
        Destroy(gameObject);
    }
    
    public void CheckIfNeedsToBeDestroyed(Cell cell)
    {
        if (cell == _spawnCell)
        {
            Destroy();
        }
    }
    
}