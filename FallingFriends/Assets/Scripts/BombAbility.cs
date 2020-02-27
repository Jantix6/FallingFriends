using System;
using System.Collections;
using FMOD;
using UnityEngine;

[Serializable]
public class BombAbility : MonoBehaviour, IAbility
{
    [SerializeField] private GameObject _explosionPrefab;
    private Cell _spawnCell;
    private const int TICKSTOEXPLODE = 2;
    private const int EXPLOSIONDISTANCE = 1;
    private int _currentTicksToExplode;
    private GridManager _gridManager;
    private int _playerId;
    
    public void Init(Cell spawnCell, GridManager gridManager, int playerId)
    {
        GameplayController.AbilityUsageTick += Tick;
        _spawnCell = spawnCell;
        _currentTicksToExplode = 0;
        _gridManager = gridManager;
        _playerId = playerId;
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
                if (cellObjective.CanSpawnExplosion())
                {
                    var explosion = Instantiate(_explosionPrefab, cellObjective.PlayerWorldPosition.position, Quaternion.identity);
                    Destroy(explosion, GameplayController.TickDuration * 0.9f); // This should be an animation or effect.
                    cellObjective.AutoDestroy();
                }
            }
        }
        var centerExplosion = Instantiate(_explosionPrefab, _spawnCell.PlayerWorldPosition.position, Quaternion.identity);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.BombExplosion);
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