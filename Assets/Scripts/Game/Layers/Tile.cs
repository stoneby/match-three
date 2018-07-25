using UnityEngine;

public class Tile : MonoBehaviour
{
    public TILE_TYPE type;
    public Node node;

    float borderPosition = 0.52f;

    public bool NoTile()
    {
        return (type == TILE_TYPE.NONE || type == TILE_TYPE.PASSTHROUGH);
    }

    public bool TileNode(Node check)
    {
        return (check != null && check.tile.type == TILE_TYPE.LIGHT || check.tile.type == TILE_TYPE.DARK);
    }
}
