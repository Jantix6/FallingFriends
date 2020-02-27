using System.Collections.Generic;
using UnityEngine;

public class EarthquakeEvent : IEvent
{
    private EventsController _eventsController;
    private GridManager _gridManager;
    private int _currentGameTicks;
    private int _gameTicksPerOwnTick;
    private int _ownCurrentTicks;
    private int _ownObjectiveTicks;
    private int _gameTicksForFeedbackBeforeEvent;
    private int _cellsToDestroyWithEachEvent;
    private List<Cell> _cellsToDestroy;
    public const string _eventName = "EARTHQUAKE";

    public EarthquakeEvent(
        EventsController eventsController, 
        GridManager gridManager, 
        int gameTicksPerOwnTick, 
        int ownObjectiveTicks, 
        int gameTicksForFeedbackBeforeEvent,
        int cellsToDestroyWithEachEvent
    )
    {
        _eventsController = eventsController;
        _gridManager = gridManager;
        _currentGameTicks = 0;
        _gameTicksPerOwnTick = gameTicksPerOwnTick;
        _ownCurrentTicks = 0;
        _ownObjectiveTicks = ownObjectiveTicks;
        _gameTicksForFeedbackBeforeEvent = GetCorrectTicksForFeedback(gameTicksForFeedbackBeforeEvent);
        _cellsToDestroyWithEachEvent = cellsToDestroyWithEachEvent;
        _cellsToDestroy = new List<Cell>();
    }

    public void Tick()
    {
        _currentGameTicks++;
        if (_currentGameTicks % _gameTicksPerOwnTick == 0)
        {
            _ownCurrentTicks++;
            Activate();
            if (_ownCurrentTicks >= _ownObjectiveTicks)
            {
                _eventsController.EventEnded();
            }
            _eventsController._progressTimeBar.fillAmount = 1.0f - ((float)_ownCurrentTicks / _ownObjectiveTicks);
        }
        else if ((_currentGameTicks + _gameTicksForFeedbackBeforeEvent) % (_gameTicksPerOwnTick) == 0)
        {
            ActivateFeedback();
        }
    }

    public void ActivateFeedback()
    {
        _cellsToDestroy.Clear();
        var possibleDestroyedPositions = new List<Vector2>();
        for (int i = 0; i < _gridManager.Grid.GetLength(0); i++)
        {
            for (int j = 0; j < _gridManager.Grid.GetLength(1); j++)
            {
                var currentCell = _gridManager.Grid[i, j];
                if (currentCell.CanBeDestroyed())
                {
                    possibleDestroyedPositions.Add(currentCell.gridPosition);
                    
                }
            }
        }

        if (possibleDestroyedPositions.Count > 0)
        {
            for (int i = 0; i < _cellsToDestroyWithEachEvent; i++)
            {
                var randomCellPos = possibleDestroyedPositions[Random.Range(0, possibleDestroyedPositions.Count)];
                var cellToDestroy = _gridManager.Grid[(int) randomCellPos.x, (int) randomCellPos.y];
                _cellsToDestroy.Add(cellToDestroy);
                cellToDestroy.ShowFeedback();
            }
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.Earthquake);
        }
        else
        {
            Debug.LogError("NO CELLS CAN BE DESTROYED");
        }
    }

    public void Activate()
    {
        _eventsController._progressTimeBar.enabled = true;
        foreach (var cellToDestroy in _cellsToDestroy)
        {
            cellToDestroy.AutoDestroy();
        }
    }

    private int GetCorrectTicksForFeedback(int ticksForFeedbackBeforeEvent)
    {
        if (ticksForFeedbackBeforeEvent > _gameTicksPerOwnTick)
        {
            return _gameTicksPerOwnTick;
        }
        return ticksForFeedbackBeforeEvent;
    }

    public string GetEventName()
    {
        return _eventName;
    }
}