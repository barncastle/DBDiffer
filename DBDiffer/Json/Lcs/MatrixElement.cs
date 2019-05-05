namespace DBDiffer.Json.Lcs
{
    public struct MatrixElement
    {
        public int Value { get; set; }
        public LcsOperation Operation { get; set; }
    }
}
