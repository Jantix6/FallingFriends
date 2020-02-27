using UnityEngine;

public interface ICollectable
{
    void Spawn(Cell spawnCell, GameplayController gameplayController, AbilityType abilityType, GameObject abilityUsable);
    void OnTriggerEnter(Collider other);
    void CollectedByPlayer(Player player);
    void Destroy();
    void DestroyedOwnCell();
}