using CommonLib.GridEngine;

public class Map : GridMap
{
    private static readonly float CELL_SIZE = 1f;

    private uint _ownerId;
    public uint Owner
    {
        get
        {
            return _ownerId;
        }
        set
        {
            _ownerId = value;
        }
    }

    private uint _background;
    public uint Background
    {
        get
        {
            return _background;
        }
        set
        {
            _background = value;
        }
    }

    public Map(int width, int height, uint uid) : base(CELL_SIZE, width, height, uid)
    {

    }
}
