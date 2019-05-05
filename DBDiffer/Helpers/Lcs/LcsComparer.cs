using System;

namespace DBDiffer.Helpers.Lcs
{
    internal static class LcsComparer
    {
        public static LcsComparisonResult Compare(string[] a, string[] b)
        {
            var cols = a.Length;
            var rows = b.Length;

            var prefix = FindPrefix(a, b);
            var suffix = prefix < cols && prefix < rows ? FindSuffix(a, b) : 0;

            var remove = suffix + prefix - 1;
            cols -= remove;
            rows -= remove;
            var matrix = CreateMatrix(cols, rows);

            for (var j = cols - 1; j >= 0; j--)
                for (var i = rows - 1; i >= 0; i--)
                    matrix[i][j] = Backtrack(matrix, a, b, prefix, j, i);

            return new LcsComparisonResult
            {
                Prefix = prefix,
                Matrix = matrix,
                Suffix = suffix
            };
        }

        public static void Reduce(LcsComparisonResult comparisonResult, Action<LcsOperation, int, int> reduceAction)
        {
            int i, j, k;
            LcsOperation op;

            var m = comparisonResult.Matrix;

            // reduce shared prefix
            var l = comparisonResult.Prefix;
            for (i = 0; i < l; ++i)
                reduceAction(LcsOperation.Skip, i, i);

            // reduce longest change span
            k = i;
            l = m.Length;
            i = 0;
            j = 0;
            while (i < l)
            {
                op = m[i][j].Operation;
                reduceAction(op, i + k, j + k);

                switch (op)
                {
                    case LcsOperation.Skip: ++i; ++j; break;
                    case LcsOperation.Right: ++j; break;
                    case LcsOperation.Down: ++i; break;
                }
            }

            // reduce shared suffix
            i += k;
            j += k;
            l = comparisonResult.Suffix;

            for (k = 0; k < l; k++)
                reduceAction(LcsOperation.Skip, i + k, j + k);
        }

        private static int FindPrefix(string[] a, string[] b)
        {
            var l = Math.Min(a.Length, b.Length);

            int i = 0;
            for (; i < l; i++)
                if (a[i] != b[i])
                    return i;

            return i;
        }

        private static int FindSuffix(string[] a, string[] b)
        {
            var al = a.Length - 1;
            var bl = b.Length - 1;
            var l = Math.Min(al, bl);

            int i = 0;
            for (; i < l; i++)
                if (a[al - i] != b[bl - i])
                    return i;

            return i;
        }

        private static MatrixElement Backtrack(MatrixElement[][] matrix, string[] a, string[] b, int start, int j, int i)
        {
            if (j + start >= a.Length && i + start >= b.Length)
                return new MatrixElement { Value = matrix[i + 1][j + 1].Value, Operation = LcsOperation.Skip };

            if (j + start < a.Length && i + start < b.Length && string.CompareOrdinal(a[j + start], b[i + start]) == 0)
                return new MatrixElement { Value = matrix[i + 1][j + 1].Value, Operation = LcsOperation.Skip };

            if (matrix[i][j + 1].Value < matrix[i + 1][j].Value)
                return new MatrixElement { Value = matrix[i][j + 1].Value + 1, Operation = LcsOperation.Right };

            return new MatrixElement { Value = matrix[i + 1][j].Value + 1, Operation = LcsOperation.Down };
        }

        private static MatrixElement[][] CreateMatrix(int cols, int rows)
        {
            var m = new MatrixElement[rows + 1][];

            // fill the last row
            var lastrow = m[rows] = new MatrixElement[cols + 1];
            for (var j = 0; j < cols; j++)
                lastrow[j] = new MatrixElement { Value = cols - j, Operation = LcsOperation.Right };

            // fill the last col
            for (var i = 0; i < rows; i++)
            {
                m[i] = new MatrixElement[cols + 1];
                m[i][cols] = new MatrixElement { Value = rows - i, Operation = LcsOperation.Down };
            }

            // fill the last cell
            m[rows][cols] = new MatrixElement { Value = 0, Operation = LcsOperation.Skip };

            return m;
        }
    }
}
