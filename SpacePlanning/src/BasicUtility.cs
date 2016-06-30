using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacePlanning
{
    internal class BasicUtility
    {

        #region - Public Methods

        //generate a list of ints between given numbers
        public static List<int> GenerateList(int start=0, int end = 10)
        {
            if (start > end)
            {
                int temp = end;
                end = start;
                start = temp;
            }

            List<int> intList = new List<int>();
            int range = end - start;
            for (int i = 0; i < range; i++) intList.Add(i + start);
            return intList;
        }

        //randomize an input list of integers
        public static List<int>RandomizeList(List<int> indices, Random ran = null)
        {
            if (ran == null) ran = new Random();  
            int n = indices.Count;
            while (n > 1)
            {
                n--;
                int k = ran.Next(n + 1);
                int value = indices[k];
                indices[k] = indices[n];
                indices[n] = value;
            }
            return indices.Select(x => x).ToList();      
        }

        //returns a random object
        public static Random RandomMaker(int seed =0)
        {
            return new Random(seed);
        }

        //sorts input list of double and returns the indices 
        public static List<int> SortIndex(List<double> A)
        {
            var sorted = A
                        .Select((x, i) => new KeyValuePair<double, int>(x, i))
                        .OrderBy(x => x.Key)
                        .ToList();
            //get the keys, in a list
            List<double> B = sorted.Select(x => x.Key).ToList();
            //get the values in a list
            List<int> idx = sorted.Select(x => x.Value).ToList();
            //return the indices list
            return idx;
        }

        //random double numbers between two decimals
        public static double RandomBetweenNumbers(Random rn, double max, double min)
        {
            double num = rn.NextDouble() * (max - min) + min;
            return num;
        }

        //checks if a number is within a certain range of another or not
        // returns -1 , if within range, 1 if greater, 0 if smaller
        public static int CheckWithinRange(double number, double comparingNum, double eps = 0)
        {
            if (comparingNum >= number - eps && comparingNum <= number + eps) return -1;
            else if (comparingNum > number - eps) return 1;
            else return 0;
        }

        //normalize a list within an inout range
        public static List<double> NormalizeList(List<double> numList, double lowValue = 0, double highValue = 100)
        {
            if (lowValue > highValue) { double temp = lowValue; lowValue = highValue; highValue = temp; }
            List<int> indexList = new List<int>();
            double outRange = highValue - lowValue;
            for (int n = 0; n < numList.Count; n++) indexList.Add(n);
            List<int> sortedIndices = Quicksort(numList);
            double minAng = numList[sortedIndices[0]];
            sortedIndices.Reverse();
            double maxAng = numList[sortedIndices[0]];
            double inpRange = maxAng - minAng;
            List<double> numListNormalized = new List<double>();

            for (int n = 0; n < numList.Count; n++)
            {
                double slope = 1.0 * outRange / inpRange;
                double output = lowValue + Math.Round(slope * (numList[n] - minAng));
                numListNormalized.Add(output);
            }
            return numListNormalized;
        }

        //quicksort algorithm with list input
        public static List<int> Quicksort(List<double> aList)
        {
            List<int> indexList = new List<int>();
            for (int n = 0; n < aList.Count; n++) indexList.Add(n);
            int left = 0, right = aList.Count - 1;
            double[] a = new double[aList.Count];
            int[] index = new int[indexList.Count];
            for (int m = 0; m < aList.Count; m++)
            {
                a[m] = aList[m];
                index[m] = indexList[m];
            }

            if (right <= left) return null;
            int i = Partition(ref a, ref index, left, right);
            QuicksortInternal(a, index, left, i - 1);
            QuicksortInternal(a, index, i + 1, right);
            List<int> sortedIndex = new List<int>();
            for (int j = 0; j < index.Length; j++) sortedIndex.Add(index[j]);
            return sortedIndex;
        }

        //toggle input value between 0 and 1
        public static int ToggleInputInt(int value = 0)
        {
            if (value == 0) return 1;
            else return 0;
        }


        //binary search algo with double
        public static int BinarySearch(List<double> inputArray, double key)
        {
            int min = 0;
            int max = inputArray.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                if (key == inputArray[mid]) return ++mid - 1;
                else if (key < inputArray[mid]) max = mid - 1;
                else min = mid + 1;
            }
            return -1;
        }

        //toggle input value randomly between 0 and 1
        public static int ToggleRandomInt()
        {
            double num = new Random().NextDouble();
            if (num > 0.5) return 1;
            else return 0;
        }

        #endregion


        #region - Private Methods

        //quicksort algorithm
        internal static List<int> QuicksortInternal(double[] a, int[] index, int left, int right)
        {
            if (right <= left) return null;
            int i = Partition(ref a, ref index, left, right);
            QuicksortInternal(a, index, left, i - 1);
            QuicksortInternal(a, index, i + 1, right);
            List<int> sortedIndex = new List<int>();
            for (int j = 0; j < index.Length; j++) sortedIndex.Add(index[j]);
            return sortedIndex;
        }

        // used by quicksortinternal
        // partition a[left] to a[right], assumes left < right for Quicksort
        internal static int Partition(ref double[] a, ref int[] index,
        int left, int right)
        {
            int i = left - 1;
            int j = right;
            while (true)
            {
                while (IsLess(a[++i], a[right])) ;
                while (IsLess(a[right], a[--j]))
                    if (j == left) break;
                if (i >= j) break;
                Exchange(a, index, i, j);
            }
            Exchange(a, index, i, right);
            return i;
        }

        //return true/ false if x is less than y
        internal static bool IsLess(double x, double y)
        {
            return (x < y);
        }

        // exchange two indices in an array
        internal static void Exchange(double[] a, int[] index, int i, int j)
        {
            double swap = a[i];
            a[i] = a[j];
            a[j] = swap;
            int b = index[i];
            index[i] = index[j];
            index[j] = b;
        }


        //cleans duplicate indices from a list
        internal static List<double> CleanDuplicateIndices(List<double> exprList)
        {
            var dups = exprList.GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();
            List<double> distinct = exprList.Distinct().ToList();
            for (int i = 0; i < distinct.Count; i++)
            {
                double dis = distinct[i];
                for (int j = 0; j < exprList.Count; j++)
                {
                    if (dis == exprList[j]) break;
                }
            }
            return dups;

        }
        

        #endregion
 

  
    


    }



}
