using System;
using UnityEngine;

[Serializable]
public class GameplayConfig
{
    [Header("Gameplay Variables")]
    public float MovementTimeMs;
    public float PauseTimeMs;
    
    [Header("Abilities Variables")]
    public int MaxAbilitiesInMap;
    public int AbilitiesToPlayersSpawnDistance;
    [Range(0.0f, 100.0f)]
    public float BombSpawnProbability;
    [Range(0.0f, 100.0f)]
    public float MissileSpawnProbability;
    [Range(0.0f, 100.0f)]
    public float MineSpawnProbability;
    
    [Header("Events Variables")]
    public int TicksToStartEvent;
    [Range(0.0f, 100.0f)]
    public float StormProbability;
    [Range(0.0f, 100.0f)]
    public float EarthquakeProbability;
    [Range(0.0f, 100.0f)]
    public float InvertControlsProbability;
    
    [Header("Storm Variables")]
    public int StormGameTicksPerOwnTick;
    public int StormOwnObjectiveTicks;
    public int StormGameTicksForFeedbackBeforeEvent;
    
    [Header("Earthquake Variables")]
    public int EarthquakeGameTicksPerOwnTick;
    public int EarthquakeOwnObjectiveTicks;
    public int EarthquakeGameTicksForFeedbackBeforeEvent;
    [Range(0, 20)]
    public int MinCellsToDestroyWithEachEvent;
    [Range(0, 20)]
    public int MaxCellsToDestroyWithEachEvent;
    
    [Header("Invert Controls Variables")]
    public int InvertControlsGameObjectiveTicks;
}