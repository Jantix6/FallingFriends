using UnityEngine;

public class Explosion : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log($"player {player.playerId} touched explosion");
            player.DieByCell();
        }
    }
}