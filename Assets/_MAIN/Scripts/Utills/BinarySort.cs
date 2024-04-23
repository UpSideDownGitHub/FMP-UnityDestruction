using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityFracture
{
    public interface IBinSortable
    {
        int bin { get; set; }
    }

    public class BinarySort
    {
        internal static int GetBinNumber(int i, int j, int n)
        {
            return (i % 2 == 0) ? (i * n) + j : (i + 1) * n - j - 1;
        }

        internal static T[] Sort<T>(T[] input, int lastIndex, int binCount) where T : IBinSortable
        {
            int[] count = new int[binCount];
            T[] output = new T[input.Length];

            #region Validation
            if (binCount <= 1)
            {
                return input;
            }

            if (lastIndex > input.Length)
            {
                lastIndex = input.Length;
            }
            #endregion

            for (int i = 0; i < lastIndex; i++)
            {
                int j = input[i].bin;
                count[j] += 1;
            }

            for (int i = 1; i < binCount; i++)
            {
                count[i] += count[i - 1];
            }

            for (int i = lastIndex - 1; i >= 0; i--)
            {
                int j = input[i].bin;
                count[j] -= 1;
                output[count[j]] = input[i];
            }

            for (int i = lastIndex; i < output.Length; i++)
            {
                output[i] = input[i];
            }

            return output;
        }

    }
}
