
public class PlaygroundDTO
{
    public int Size { get; set; }
    public UnitDTO[] UnitInfos { get; set; }

    public override string ToString()
    {
        var res = "";
        res += "Size: " + Size;
        foreach(var info in UnitInfos)
        {
            res += "\n   " + info.ToString();
        }
        return res;
    }
}
