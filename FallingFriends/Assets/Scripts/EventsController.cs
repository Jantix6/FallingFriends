using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventsController: MonoBehaviour
{

    public GridManager GridManager => _gridManager;
    public bool EventActive => _eventActive;
    public Image _progressTimeBar;
    public GameObject ProgressTimeBarParent;
    public TextMeshProUGUI _eventName;

    [SerializeField] private GameObject _cloudPrefab;
    private GridManager _gridManager;
    private bool _eventActive;
    private int _currentTicks;
    private int _objectiveTicks;
    private IEvent _currentEvent;
    private GameplayConfig _gameplayConfig;
    public int _stormGameTotalTicks;

    public void CustomStart(GridManager gridManager, GameplayConfig gameplayConfig)
    {
        _gridManager = gridManager;
        _currentTicks = 0;
        _eventActive = false;
        _gameplayConfig = gameplayConfig;
        _stormGameTotalTicks = 0;
        _eventName.gameObject.SetActive(false);
    }

    public void Init(int objectiveTicks)
    {
        _objectiveTicks = objectiveTicks;
    }

    public void Reset()
    {
        _currentTicks = 0;
        _eventActive = false;
        _currentEvent = null;
        ProgressTimeBarParent.SetActive(false);
        _eventName.gameObject.SetActive(false);
    }

    public void Tick()
    {
        if (!_eventActive)
        {
            _currentTicks++;
            if (_currentTicks >= _objectiveTicks)
            {
                _currentTicks = 0;
                GenerateEvent();
                _eventActive = true;
            }
        }
        else
        {
            _currentEvent.Tick();
        }
    }

    private void GenerateEvent()
    {
        var random = Random.Range(0.0f, 100.0f);
        if (random <= _gameplayConfig.StormProbability)
        {
            _currentEvent = GenerateStormEvent();
        }
        else if (random <= _gameplayConfig.StormProbability + _gameplayConfig.EarthquakeProbability)
        {
            _currentEvent = GenerateEarthquakeEvent();
        }
        else if (random <= _gameplayConfig.StormProbability + _gameplayConfig.EarthquakeProbability + _gameplayConfig.InvertControlsProbability)
        {
            _currentEvent = GenerateInvertControlsEvent();
        }
        _eventName.gameObject.SetActive(true);
        _eventName.text = _currentEvent.GetEventName();
    }

    private IEvent GenerateStormEvent()
    {
        return new StormEvent(
            this, 
            _gridManager,
            _gameplayConfig.StormGameTicksPerOwnTick,
            _gameplayConfig.StormOwnObjectiveTicks,
            _gameplayConfig.StormGameTicksForFeedbackBeforeEvent,
            _cloudPrefab
            );
    }

    private IEvent GenerateEarthquakeEvent()
    {
        return new EarthquakeEvent(
            this,
            _gridManager,
            _gameplayConfig.EarthquakeGameTicksPerOwnTick,
            _gameplayConfig.EarthquakeOwnObjectiveTicks,
            _gameplayConfig.EarthquakeGameTicksForFeedbackBeforeEvent,
            Random.Range(_gameplayConfig.MinCellsToDestroyWithEachEvent, _gameplayConfig.MaxCellsToDestroyWithEachEvent)
        );
    }

    private IEvent GenerateInvertControlsEvent()
    {
        return new InvertControlsEvent(
            this,
            _gameplayConfig.InvertControlsGameObjectiveTicks
        );
    }

    public void EventEnded()
    {
        _currentTicks = 0;
        _eventActive = false;
        _currentEvent = null;
        _progressTimeBar.fillAmount = 1.0f;
        ProgressTimeBarParent.SetActive(false);
        _eventName.gameObject.SetActive(false);
    }
}