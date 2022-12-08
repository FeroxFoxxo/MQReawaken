namespace Web.AssetBundles.Models;

public class TreeInfo
{
    public readonly string Name;
    public readonly TreeInfo[] SubTrees;

    public TreeInfo(string name, TreeInfo[] subTrees)
    {
        Name = name;
        SubTrees = subTrees;
    }
}
