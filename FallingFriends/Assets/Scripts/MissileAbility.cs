using System;
using System.Management.Instrumentation;
using FMOD;
using UnityEngine;

[Serializable]
public class MissileAbility : MonoBehaviour, IAbility
{
    [SerializeField] private GameObject _explosionPrefab;
    private Cell _spawnCell;
    private const int TICKSTOEXPLODE = 5;
    private const int EXPLOSIONDISTANCE = 0;
    private int _currentTicksToExplode;
    private GridManager _gridManager;
    private float _speed;
    private int _playerId;

    private Ray _ray;
    private RaycastHit _raycastHit;
    private int _layerMask = (1 << 9) | (1 << 10);
    
    public void Init(Cell spawnCell, GridManager gridManager, int playerId)
    {
        GameplayController.AbilityUsageTick += Tick;
        _spawnCell = spawnCell;
        _currentTicksToExplode = 0;
        _gridManager = gridManager;
        _speed = 2 / GameplayController.TickDuration;
        _playerId = playerId;
    }

    void Update()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;
        
        _ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(_ray, out _raycastHit, 10.0f, _layerMask))
        {
            _spawnCell = _raycastHit.collider.gameObject.GetComponent<Cell>();
        }
        else
        {
            _spawnCell = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            if (player.playerId != _playerId)
            {
                _spawnCell = player.currentCell;
                Explode();
            }
        }
    }

    public void Tick()
    {
        _currentTicksToExplode++;
        if (_currentTicksToExplode >= TICKSTOEXPLODE)
        {
            GameplayController.AbilityUsageTick -= Tick;
            Explode();
        }
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
                if (cellObjective.cellState == CellState.Walkable || cellObjective.cellState == CellState.Indestructible)
                {
                    var explosion = Instantiate(_explosionPrefab, cellObjective.PlayerWorldPosition.position, Quaternion.identity);
                    Destroy(explosion, GameplayController.TickDuration * 0.9f); // This should be an animation or effect.
                    cellObjective.AutoDestroy();
                }
            }
        }

        if (_spawnCell != null)
        {
            var centerExplosion = Instantiate(_explosionPrefab, _spawnCell.PlayerWorldPosition.position,
                Quaternion.identity);
            Destroy(centerExplosion, GameplayController.TickDuration * 0.9f); // This should be an animation or effect.
            _spawnCell.AutoDestroy();
            Destroy();
        }
        else
        {
            var centerExplosion = Instantiate(_explosionPrefab, transform.position,
                Quaternion.identity);
            Destroy(centerExplosion, GameplayController.TickDuration * 0.9f); // This should be an animation or effect.
            Destroy();
        }
        
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.MissileExplosion);
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