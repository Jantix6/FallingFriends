using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertControlsEvent : IEvent {

	private int _currentGameTicks;
	private readonly EventsController _eventsController;
	private readonly int _gameObjectiveTicks;
	private const string _eventName = "INVERTED CONTROLS";
    
	public InvertControlsEvent(
		EventsController eventsController, 
		int gameObjectiveTicks 
	)
	{
		_currentGameTicks = 0;
		_eventsController = eventsController;
		_gameObjectiveTicks = gameObjectiveTicks;
		Activate();
	}
	
	public void Tick()
    {
        _currentGameTicks++;
        if (_currentGameTicks % _gameObjectiveTicks == 0)
        {

	        foreach (var player in GameManager.Players)
	        {
		        player._directionController = 1;
		        player._invertedControlsParticles.SetActive(false);
	        }
	        _eventsController.EventEnded();
        }
        _eventsController._progressTimeBar.fillAmount = 1.0f - ((float) _currentGameTicks / _gameObjectiveTicks);
    }

    public void Activate()
    {
	    _eventsController.ProgressTimeBarParent.SetActive(true);
        foreach (var player in GameManager.Players)
        {
            player._directionController = -1;
            player._invertedControlsParticles.SetActive(true);
        }
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.InvertedControls);
    }

    public string GetEventName()
    {
        return _eventName;
    }

    public void ActivateFeedback()
    {
	    
    }
}
