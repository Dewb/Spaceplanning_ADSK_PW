using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacePlanning
{
    internal class BasicUtility
    {

        internal static List<int> SortIndex(List<double> A)
        {
            var sorted = A
                        .Select((x, i) => new KeyValuePair<double, int>(x, i))
                        .OrderBy(x => x.Key)
                        .ToList();

            List<double> B = sorted.Select(x => x.Key).ToList();
            List<int> idx = sorted.Select(x => x.Value).ToList();

            return idx;
        }



        //random double numbers between two decimals
        internal static double RandomBetweenNumbers(Random rn, double max, double min)
        {

            double num = rn.NextDouble() * (max - min) + min;
            return num;
        }
        
        internal static List<int> quicksort(double[] a, int[] index, int left, int right)
        {
            if (right <= left) return null;
            int i = partition(ref a, ref index, left, right);
            quicksort(a, index, left, i - 1);
            quicksort(a, index, i + 1, right);
            List<int> sortedIndex = new List<int>();
            for(int j = 0; j < index.Length; j++)
            {
                sortedIndex.Add(index[j]);
            }

            return sortedIndex;
        }

        //toggle input value between 0 and 1
        internal static int RandomToggleInputInt()
        {
            Random rn = new Random();
            double num = rn.NextDouble();


            if (num >0.5)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //toggle input value between 0 and 1
        internal static int toggleInputInt(int value = 0)
        {
            if(value == 0)
            {
                return 1;
            }else
            {
                return 0;
            }
        }

        // partition a[left] to a[right], assumes left < right
        private static int partition(ref double[] a, ref int[] index,
        int left, int right)
        {
            int i = left - 1;
            int j = right;
            while (true)
            {
                while (less(a[++i], a[right]))      // find item on left to swap
                    ;                               // a[right] acts as sentinel
                while (less(a[right], a[--j]))      // find item on right to swap
                    if (j == left) break;           // don't go out-of-bounds
                if (i >= j) break;                  // check if pointers cross
                exch(a, index, i, j);               // swap two elements into place
            }
            exch(a, index, i, right);               // swap with partition element
            return i;
        }

        // is x < y ?
        private static bool less(double x, double y)
        {
            return (x < y);
        }

        // exchange a[i] and a[j]
        private static void exch(double[] a, int[] index, int i, int j)
        {
            double swap = a[i];
            a[i] = a[j];
            a[j] = swap;
            int b = index[i];
            index[i] = index[j];
            index[j] = b;
        }


        //////BINARY SEARCH
        internal static int  BinarySearch(List<int> inputArray, int key)
        {
            int min = 0;
            int max = inputArray.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                if (key == inputArray[mid])
                {
                    return ++mid-1;
                }
                else if (key < inputArray[mid])
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }
            return -1;
        }


        //////BINARY SEARCH with Double
        internal static int BinarySearchDouble(List<double> inputArray, double key)
        {
            int min = 0;
            int max = inputArray.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                if (key == inputArray[mid])
                {
                    return ++mid - 1;
                }
                else if (key < inputArray[mid])
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }
            return -1;
        }




    }
}
