using System;
using UnityEngine;

[Serializable]
public class Cell : MonoBehaviour
{
    public ICollectable CollectableOnTop;
    public Transform PlayerWorldPosition;
    public Vector2 gridPosition;
    public CellState cellState;
    public Teleport OnTopTeleport;
    [SerializeField] private CellContent _cellContent;
    [SerializeField] private Animation _animationComponent;
    [SerializeField] private AnimationClip _feedbackClip;
    [SerializeField] private AnimationClip _actionClip;
    
    //Variables for pathfinding
    public Cell Parent;
    public float DistanceToTarget;
    public float Cost;
    

    public void AutoDestroy()
    {
        if (cellState == CellState.Indestructible)
        {
            return;
        }
        if (OnTopTeleport != null)
        {
            OnTopTeleport.Destroy();
        }
        CheckIfNeedsToDestroyAbilityOnTop();
        foreach (var player in GameManager.Players)
        {
            if (gridPosition == player.currentCell.gridPosition)
            {
                player.DieByCell();
            }
            else if (gridPosition == player.nextCell.gridPosition)
            {
                player.DieInFront();
            }
        }
        ChangeCellState(CellState.Destroyed);
        
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.CubeFalling);
    }

    private void CheckIfNeedsToDestroyAbilityOnTop()
    {
        foreach (var ability in GameplayController.Instance._abilitiesUsablesParent.GetComponentsInChildren<IAbility>())
        {
            ability.CheckIfNeedsToBeDestroyed(this);
        }
    }

    public void ShowFeedback()
    {
        if (cellState == CellState.Indestructible)
        {
            return;
        }
        _animationComponent.clip = _feedbackClip;
        _animationComponent[_feedbackClip.name].speed = 1 / GameplayController.TickDuration;
        _animationComponent.Play();
    }
    
    public void ChangeCellContent(CellContent newCellContent)
    {
        _cellContent = newCellContent;
    }

    public void ChangeCellState(CellState newCellState)
    {
        switch (cellState)
        {
            case CellState.Walkable:
                break;
            case CellState.Destroyed:
                break;
            case CellState.Obstacle:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        switch (newCellState)
        {
            case CellState.Walkable:
                break;
            case CellState.Destroyed:
                cellState = newCellState;
                CollectableOnTop?.DestroyedOwnCell();
                
                PlayerDestroyAnimation();
                break;
            case CellState.Obstacle:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newCellState), newCellState, null);
        }
        cellState = newCellState;
    }

    private void PlayerDestroyAnimation()
    {
        _animationComponent.clip = _actionClip;
        _animationComponent[_actionClip.name].speed = 1 / GameplayController.TickDuration;
        _animationComponent.Play();
    }

    public bool CanSpawnAbility()
    {
        return (_cellContent == CellContent.Empty && (cellState == CellState.Walkable || cellState == CellState.Indestructible));
    }

    public bool CanBeDestroyed()
    {
        return cellState != CellState.Destroyed && cellState != CellState.Indestructible;
    }

    public bool IsWalkable()
    {
        return cellState == CellState.Walkable || cellState == CellState.Indestructible;
    }
    
    public bool CanSpawnTeleport()
    {
        return (IsWalkable() && _cellContent == CellContent.Empty);
    }

    public bool CanSpawnExplosion()
    {
        return cellState == CellState.Walkable || cellState == CellState.Indestructible;
    }
}