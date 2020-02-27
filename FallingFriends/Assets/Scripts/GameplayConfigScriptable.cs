using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfig", menuName = "GameplayConfigScriptable")]
public class GameplayConfigScriptable : ScriptableObject
{
    public GameplayConfig GameplayConfig => _gameplayConfig;
    [SerializeField] private GameplayConfig _gameplayConfig;
}