using UnityEngine;

public class StormEvent : IEvent
{
    private int _currentGameTicks;
    private int _ownCurrentTicks;
    private readonly EventsController _eventsController;
    private readonly GridManager _gridManager;
    private readonly int _gameTicksPerOwnTick;
    private readonly int _ownObjectiveTicks;
    private readonly GameObject _cloudPrefab;
    private readonly int _gameTicksForFeedbackBeforeEvent;
    private const string _eventName = "STORM";

    public StormEvent(
        EventsController eventsController,
        GridManager gridManager,
        int gameTicksPerOwnTick,
        int ownObjectiveTicks,
        int gameTicksForFeedbackBeforeEvent,
        GameObject cloudPrefab
    )
    {
        _eventsController = eventsController;
        _gridManager = gridManager;
        _currentGameTicks = 0;
        _gameTicksPerOwnTick = gameTicksPerOwnTick;
        _ownCurrentTicks = 0;
        _ownObjectiveTicks = ownObjectiveTicks;
        _cloudPrefab = cloudPrefab;
        _gameTicksForFeedbackBeforeEvent = GetCorrectTicksForFeedback(gameTicksForFeedbackBeforeEvent);
    }

    public void Tick()
    {
        _currentGameTicks++;
        if (_currentGameTicks % _gameTicksPerOwnTick == 0)
        {
            _ownCurrentTicks++;
            _eventsController._stormGameTotalTicks++;
            Activate();
            if (_ownCurrentTicks >= _ownObjectiveTicks)
            {
                _eventsController.EventEnded();
            }
            _eventsController._progressTimeBar.fillAmount = 1.0f - ((float)_ownCurrentTicks / _ownObjectiveTicks);
        }
        else if ((_currentGameTicks + _gameTicksForFeedbackBeforeEvent) % _gameTicksPerOwnTick == 0)
        {
            ActivateFeedback();
        }
    }

    public void ActivateFeedback()
    {
        var width = _gridManager.Grid.GetLength(1);
        var height = _gridManager.Grid.GetLength(0);
        GameObject cloud = null;
        
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                if (_gridManager.Grid[row, column].cellState == CellState.Destroyed ||
                    _gridManager.Grid[row, column].cellState == CellState.Indestructible)
                {
                    continue;
                }

                if (row + column == _eventsController._stormGameTotalTicks)
                {
                    cloud = Object.Instantiate(_cloudPrefab, _gridManager.Grid[row, column].PlayerWorldPosition.position,
                        Quaternion.identity);
                    _gridManager.Grid[row, column].ShowFeedback();
                }
                else if ((height - row - 1) + (width - column - 1) == _eventsController._stormGameTotalTicks)
                {
                    cloud = Object.Instantiate(_cloudPrefab, _gridManager.Grid[row, column].PlayerWorldPosition.position,
                        Quaternion.identity);
                    _gridManager.Grid[row, column].ShowFeedback();
                }
                else if ((height - row - 1) + column == _eventsController._stormGameTotalTicks)
                {
                    cloud = Object.Instantiate(_cloudPrefab, _gridManager.Grid[row, column].PlayerWorldPosition.position,
                        Quaternion.identity);
                    _gridManager.Grid[row, column].ShowFeedback();                    
                }
                else if (row + (width - column - 1) == _eventsController._stormGameTotalTicks)
                {
                    cloud = Object.Instantiate(_cloudPrefab, _gridManager.Grid[row, column].PlayerWorldPosition.position,
                        Quaternion.identity);
                    _gridManager.Grid[row, column].ShowFeedback();
                }

                if (cloud != null)
                {
                    Object.Destroy(cloud, 1.9f * GameplayController.TickDuration);
                    SoundManager.Instance.PlayOneShot(SoundManager.Instance.Storm);
                }
                cloud = null;
            }
        }
    }

    public void Activate()
    {
        //_eventsController.ProgressTimeBarParent.SetActive(true);

        var width = _gridManager.Grid.GetLength(1);
        var height = _gridManager.Grid.GetLength(0);
        
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                if (_gridManager.Grid[row, column].cellState == CellState.Destroyed)
                {
                    continue;
                }
                if (row + column == _eventsController._stormGameTotalTicks - 1)
                {
                    _gridManager.Grid[row, column].AutoDestroy();
                }
                else if ((height - row - 1) + (width - column - 1) == _eventsController._stormGameTotalTicks - 1)
                {
                    _gridManager.Grid[row, column].AutoDestroy();
                }
                else if ((height - row - 1) + column == _eventsController._stormGameTotalTicks - 1)
                {
                    _gridManager.Grid[row, column].AutoDestroy();                    
                }
                else if (row + (width - column - 1) == _eventsController._stormGameTotalTicks - 1)
                {
                    _gridManager.Grid[row, column].AutoDestroy();
                }
            }
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