using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
    private int _framesBetweenCalculations = 3;
    private int _currentFramesCount = 0;
    private Transform _parentTransform;
    private Vector3 _startCenterPosition;
    private Vector3 _centerOfPlayers;
    private const float CAMERADISTANCE = 10f;

    [HideInInspector] public Transform _initCameraPosition;

    private void Start()
    {
        _initCameraPosition = transform.parent;
    }

    public void CustomStart()
    {
        // TODO: Calculate starting position;
        _parentTransform = transform.parent;
        _startCenterPosition = GridManager.GetCenterOfMap();
        _parentTransform.position = _startCenterPosition;
        var vectorToMove =
            new Vector3(transform.localPosition.x, transform.localPosition.y,
                transform.localPosition.z - CAMERADISTANCE);
        var vectorRelativeToParent = transform.TransformDirection(vectorToMove);
        transform.localPosition = vectorRelativeToParent;
    }

    public void CustomUpdate()
    {
        _currentFramesCount++;
        if (_currentFramesCount >= _framesBetweenCalculations)
        {
            _currentFramesCount = 0;
            _centerOfPlayers = CalculateCameraPosition();
        }

        var objectivePosition = _startCenterPosition * 0.8f + _centerOfPlayers * 0.2f;
        _parentTransform.position = Vector3.Lerp(_parentTransform.position, objectivePosition, Time.deltaTime);
    }

    private Vector3 CalculateCameraPosition()
    {
        var currentCenterOfPlayers = Vector3.zero;
        var playersAlive = 0;
        foreach (var player in GameManager.Players)
        {
            if (!player.IsDead())
            {
                currentCenterOfPlayers += player.transform.position;
                playersAlive++;
            }
        }

        if (playersAlive > 0)
        {
            currentCenterOfPlayers /= playersAlive;
            return currentCenterOfPlayers;
        }
        return _startCenterPosition;
    }
}