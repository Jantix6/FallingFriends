using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GridManager
{
    public Cell[,] Grid => _grid;
    
    private static Cell[,] _grid;
    private readonly GridConfig[] _gridConfigs;
    private readonly GameObject[] _normalCellsPrefabs;
    private readonly GameObject[] _obstacleCellsPrefabs;
    private readonly GameObject _indestructiblePrefab;
    private readonly Transform _gridParent;

    public GridManager(GridConfig[] gridConfigs, GameObject[] normalCellsPrefabs, GameObject[] obstacleCellsPrefabs, GameObject indestructiblePrefab, Transform gridParent)
    {
        _gridConfigs = gridConfigs;
        _normalCellsPrefabs = normalCellsPrefabs;
        _obstacleCellsPrefabs = obstacleCellsPrefabs;
        _indestructiblePrefab = indestructiblePrefab;
        _gridParent = gridParent;
    }
    
    public void InitMap()
    {
        DestroyGrid();
        var gridConfig = _gridConfigs[Random.Range(0, _gridConfigs.Length)];
        var rows = gridConfig.Rows.Length;
        var columns = gridConfig.Rows[0].CellData.Length;
        _grid = new Cell[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                GameObject cell = null;
                var randomRotation = Quaternion.Euler(0.0f, 90.0f * Random.Range(0, 4), 0.0f);
                switch (gridConfig.Rows[row].CellData[column].cellState)
                {
                    case CellState.Walkable:
                        cell = Object.Instantiate(_normalCellsPrefabs[Random.Range(0, _normalCellsPrefabs.Length)],
                            GridToWorldPosition(new Vector2(row, column)), randomRotation, _gridParent);
                        break;
                    case CellState.Destroyed:
                        cell = Object.Instantiate(_normalCellsPrefabs[Random.Range(0, _normalCellsPrefabs.Length)],
                            GridToWorldPosition(new Vector2(row, column)), randomRotation, _gridParent);
                        break;
                    case CellState.Obstacle:
                        cell = Object.Instantiate(_obstacleCellsPrefabs[Random.Range(0, _obstacleCellsPrefabs.Length)],
                            GridToWorldPosition(new Vector2(row, column)), randomRotation, _gridParent);
                        break;
                    case CellState.Indestructible:
                        cell = Object.Instantiate(_indestructiblePrefab, GridToWorldPosition(new Vector2(row, column)),
                            randomRotation, _gridParent);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _grid[row, column] = cell.GetComponentInChildren<Cell>();
                _grid[row, column].gridPosition = new Vector2(row, column);
                _grid[row, column].cellState = gridConfig.Rows[row].CellData[column].cellState;
                cell.name = "Cell " + row + " " + column;
                if (_grid[row, column].cellState == CellState.Destroyed)
                {
                    _grid[row, column].ChangeCellState(CellState.Destroyed);
                }
            }
        }
    }

    private void DestroyGrid()
    {
        for (int i = 0; i < _gridParent.childCount; i++)
        {
            Object.Destroy(_gridParent.GetChild(i).gameObject);
        }
    }

    public CellResult GetNewCellAccordingToDirectionAndDistance(Cell currentCell, Direction currentDirection, int distance = 1)
    {
        var nextGridPosition = currentCell.gridPosition;
        switch (currentDirection)
        {
            case Direction.Up:
                nextGridPosition = new Vector2((int) currentCell.gridPosition.x - 1 * distance, (int) currentCell.gridPosition.y);
                break;
            case Direction.Down:
                nextGridPosition = new Vector2((int) currentCell.gridPosition.x + 1 * distance, (int) currentCell.gridPosition.y);
                break;
            case Direction.Left:
                nextGridPosition = new Vector2((int) currentCell.gridPosition.x, (int) currentCell.gridPosition.y - 1 * distance);
                break;
            case Direction.Right:
                nextGridPosition = new Vector2((int) currentCell.gridPosition.x, (int) currentCell.gridPosition.y + 1 * distance);
                break;
            case Direction.None:
                Debug.Log("direction is NONE");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentDirection), currentDirection, null);
        }

        if (IsInMap(nextGridPosition))
        {
            if (IsWalkable(nextGridPosition))
            {
                return new CellResult
                {
                    Cell = _grid[(int) nextGridPosition.x, (int) nextGridPosition.y],
                    Message = "Walkable"
                };
            }
            if (IsObstacle(nextGridPosition))
            {
                return new CellResult
                {
                    Cell = currentCell,
                    Message = "Obstacle"
                };
            }
            else
            {
                return new CellResult
                {
                    Cell = currentCell,
                    Message = "Destroyed"
                };
            }
        }

        return new CellResult
        {
            Cell = currentCell,
            Message = "OutsideMap"
        };
    }

    public static bool IsInMap(Vector2 nextPosition)
    {
        return nextPosition.x >= 0 &&
               nextPosition.x < _grid.GetLength(0) &&
               nextPosition.y >= 0 &&
               nextPosition.y < _grid.GetLength(1);
    }

    private bool IsWalkable(Vector2 nextPosition)
    {
        return _grid[(int) nextPosition.x, (int) nextPosition.y].cellState == CellState.Walkable || 
               _grid[(int) nextPosition.x, (int) nextPosition.y].cellState == CellState.Indestructible;
    }
    
    private bool IsObstacle(Vector2 nextPosition)
    {
        return _grid[(int) nextPosition.x, (int) nextPosition.y].cellState == CellState.Obstacle;
    }

    private Vector3 GridToWorldPosition(Vector2 gridPosition)
    {
        return new Vector3(gridPosition.y, 0, -gridPosition.x);
    }

    private Vector2 WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector2(-worldPosition.z, worldPosition.x);
    }

    public static Vector3 GetCenterOfMap()
    {
        var centerOfMap = Vector3.zero;
        centerOfMap += _grid[0, 0].PlayerWorldPosition.position;
        centerOfMap += _grid[0, _grid.GetLength(1) - 1].PlayerWorldPosition.position;
        centerOfMap += _grid[_grid.GetLength(0) - 1, 0].PlayerWorldPosition.position;
        centerOfMap += _grid[_grid.GetLength(0) - 1, _grid.GetLength(1) - 1].PlayerWorldPosition.position;
        centerOfMap *= 0.25f;
        return centerOfMap + Vector3.back * 2.0f;
    }
    
}