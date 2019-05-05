namespace DBDiffer.Helpers.Lcs
{
    public struct MatrixElement
    {
        public int Value { get; set; }
        public LcsOperation Operation { get; set; }
    }
}
