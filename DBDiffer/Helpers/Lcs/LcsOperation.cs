namespace DBDiffer.Helpers.Lcs
{
    public enum LcsOperation
    {
        Remove = -1,
        Right = Remove,
        Add = 1,
        Down = Add,
        Skip = 0,
        Equal = Skip
    };
}
