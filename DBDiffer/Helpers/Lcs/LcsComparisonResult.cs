namespace DBDiffer.Helpers.Lcs
{
    public struct LcsComparisonResult
    {
        public int Prefix { get; set; }
        public MatrixElement[][] Matrix { get; set; }
        public int Suffix { get; set; }
    }
}
