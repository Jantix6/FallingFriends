public struct SpawnContent
{
    public Cell SpawnCell;
    public Direction SpawnDirection;

    public SpawnContent(Cell spawnCell, Direction spawnDirection)
    {
        SpawnCell = spawnCell;
        SpawnDirection = spawnDirection;
    }
}