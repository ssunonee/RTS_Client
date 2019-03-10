public class UnitDTO
{
    public int Id { get; set; }
    public Pos2D ParentPos { get; set;}
    public Pos2D NextPos { get; set; }
    public bool Moving { get; set; }
    public float MoveProgress { get; set; }

    public override string ToString()
    {
        var res = "";
        res += "Id: " + Id;
        res += "; ParentPos: x=" + ParentPos.X + " y=" + ParentPos.Y;
        res += "; NextPos: " + (NextPos == null ? "null"
            : "x="  + NextPos.X + " y = " + NextPos.Y);
        res += "; Moving: " + Moving;
        res += "; MoveProgress: " + MoveProgress;
        return res;
    }
}

public class Pos2D
{
    public int X { get; set; }
    public int Y { get; set; }
}
