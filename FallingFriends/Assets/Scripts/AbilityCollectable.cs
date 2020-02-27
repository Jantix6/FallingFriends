using UnityEngine;

public class AbilityCollectable : MonoBehaviour, ICollectable
{
    public AbilityType AbilityType;
    public GameObject AbilityUsable;
    private Cell _spawnCell;
    private GameplayController _gameplayController;
    
    public void Spawn(Cell spawnCell, GameplayController gameplayController, AbilityType abilityType, GameObject abilityUsable)
    {
        _spawnCell = spawnCell;
        _gameplayController = gameplayController;
        AbilityType = abilityType;
        AbilityUsable = abilityUsable;
    }

    public void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            CollectedByPlayer(player);
            Destroy();
        }
    }

    public void CollectedByPlayer(Player player)
    {
        _spawnCell.ChangeCellContent(CellContent.Player);
        _spawnCell.CollectableOnTop = null;
        player.RewardAbility(AbilityType, AbilityUsable);
        _gameplayController.SpawnAbilities(1);
    }

    public void Destroy()
    {
        _spawnCell.ChangeCellContent(CellContent.Empty);
        Destroy(gameObject);
    }

    public void DestroyedOwnCell()
    {
        _spawnCell.ChangeCellContent(CellContent.Empty);
        _gameplayController.SpawnAbilities(1);
        Destroy(gameObject);
    }
    
}