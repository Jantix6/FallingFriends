using UnityEngine;

[CreateAssetMenu(fileName = "GridConfigScriptable", menuName = "Grid Config Scriptable", order = 1)]
public class GridConfigScriptable : ScriptableObject
{
    public GridConfig GridConfig => _gridConfig;
    [SerializeField] private GridConfig _gridConfig;
}