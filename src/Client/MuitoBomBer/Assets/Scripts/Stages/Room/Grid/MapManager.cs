using CommonLib.GridEngine;

public class MapManager : GridManager<Map>
{
    protected override Map Instanciate(float cellSize, ushort width, ushort height, uint uid)
    {
        return new Map(width, height, uid);
    }
}
