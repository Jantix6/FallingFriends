using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Cell currentCell;
    public Cell teleportingCell;
    Player player;
    
    public void Init(Cell currentCell, Cell teleportingCell)
    {
        this.currentCell = currentCell;
        this.teleportingCell = teleportingCell;
    }

    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<Player>();
        if (player != null)
        {
            if (player.currentCell != currentCell)
            {
                player._teleport = this;
            }
        }
    }

    public void TeleportToCell()
    {
        player.transform.position = teleportingCell.PlayerWorldPosition.position;
        player.currentCell = teleportingCell;
        player.nextCell = teleportingCell;
        player._teleport = null;
    }

    public void Destroy()
    {
        currentCell.ChangeCellContent(CellContent.Empty);
        if (teleportingCell != null)
        {
            teleportingCell.OnTopTeleport.DestroyAsSecond();
        }
        Pathfinding.TeleportInstantiated = false;
        Destroy(transform.parent.gameObject);
    }

    public void DestroyAsSecond()
    {
        currentCell.ChangeCellContent(CellContent.Empty);
        Pathfinding.TeleportInstantiated = false;
        Destroy(transform.parent.gameObject);
    }
}