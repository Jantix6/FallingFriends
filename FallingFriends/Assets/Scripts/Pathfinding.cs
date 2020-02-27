using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Pathfinding
{
    public static bool TeleportInstantiated = false;
    private GridManager _gridManager;
    private Cell[,] Grid2;

    private int GridRows
    {
        get { return Grid2.GetLength(0); }
    }

    int GridCols
    {
        get { return Grid2.GetLength(1); }
    }

    public Pathfinding(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    public Stack<Cell> FindPath(Vector2 Start, Vector2 End)
    {
        Grid2 = _gridManager.Grid;
        Cell start = Grid2[(int)Start.x, (int)Start.y];
        Cell end = Grid2[(int)End.x, (int)End.y];

        Stack<Cell> Path = new Stack<Cell>();
        List<Cell> OpenList = new List<Cell>();
        List<Cell> ClosedList = new List<Cell>();
        List<Cell> adjacentCells;
        Cell current = start;

        OpenList.Add(start);

        while (OpenList.Count != 0 && !ClosedList.Exists(x => x.gridPosition == end.gridPosition))
        {
            current = OpenList[0];
            OpenList.Remove(current);
            ClosedList.Add(current);
            adjacentCells = GetAdjacentCells(current);


            foreach (Cell cell in adjacentCells)
            {
                if (!ClosedList.Contains(cell) && cell.IsWalkable())
                {
                    if (!OpenList.Contains(cell))
                    {
                        cell.Parent = current;
                        cell.DistanceToTarget = Math.Abs(cell.gridPosition.x - end.gridPosition.x) +
                                             Math.Abs(cell.gridPosition.y - end.gridPosition.y);
                        cell.Cost = 1 + cell.Parent.Cost;
                        OpenList.Add(cell);
                        OpenList = OpenList.OrderBy(GetDistance).ToList<Cell>();
                    }
                }
            }
        }

        if (!ClosedList.Exists(x => x.gridPosition == end.gridPosition))
        {
            return null;
        }

        Cell temp = ClosedList[ClosedList.IndexOf(current)];
        while (temp.Parent != start && temp != null)
        {
            Path.Push(temp);
            temp = temp.Parent;
            //Debug.Log("cell : " + temp.gridPosition);
        }
        return Path;
    }

    private List<Cell> GetAdjacentCells(Cell n)
    {
        
        // TODO: Check if tile is a teleport so we update the adjacent tiles accordingly.
        
        List<Cell> adjacentCells = new List<Cell>();

        int row = -1;
        int col = -1;
        
        if (n.OnTopTeleport != null)
        {
            row = (int) n.OnTopTeleport.teleportingCell.gridPosition.x;
            col = (int) n.OnTopTeleport.teleportingCell.gridPosition.y;
        }
        else
        {
            row = (int) n.gridPosition.x;
            col = (int) n.gridPosition.y;
        }
        
        //Debug.Log($"row : {row} + col : {col}");
        
        if (row + 1 < GridRows)
        {
            adjacentCells.Add(Grid2[row + 1, col]);
        }

        if (row - 1 >= 0)
        {
            adjacentCells.Add(Grid2[row - 1, col]);
        }

        if (col - 1 >= 0)
        {
            adjacentCells.Add(Grid2[row, col - 1]);
        }

        if (col + 1 < GridCols)
        {
            adjacentCells.Add(Grid2[row, col + 1]);
        }

        return adjacentCells;
    }

    private float GetDistance(Cell cell)
    {
        if (cell.DistanceToTarget != -1 && cell.Cost != -1)
            return cell.DistanceToTarget + cell.Cost;
        else
            return -1;
    }
}