public interface IAbility
{
    void Init(Cell spawnCell, GridManager gridManager, int playerId);
    void Tick();
    void Destroy();
    void CheckIfNeedsToBeDestroyed(Cell cell);
}