using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacePlanning
{
    public class BasicUtility
    {
        //returns a random object
        public static Random RandomMaker()
        {
            return new Random();
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
        internal static double RandomBetweenNumbers(Random rn, double max, double min)
        {
            double num = rn.NextDouble() * (max - min) + min;
            return num;
        }
        

        //checks if a number is within a certain range of another or not
        // returns -1 , if within range, 1 if greater, 0 if smaller
        internal static int CheckWithinRange(double number, double comparingNum, double eps = 0) 
        {
            if (comparingNum > number - eps && comparingNum < number + eps) return -1;
            else if (comparingNum > number - eps) return 1;
            else return 0;
        }


        //normalize a list within an inout range
        public static List<double> NormalizeList(List<double> numList, double lowValue = 0, double highValue = 100)
        {
            if (lowValue > highValue) { double temp = lowValue; lowValue = highValue; highValue = temp; }
            List<int> indexList = new List<int>();
            double range = highValue - lowValue;
            for (int n = 0; n < numList.Count; n++) indexList.Add(n);
            List<int> sortedIndices = Quicksort(numList, indexList, 0, numList.Count - 1);
            sortedIndices.Reverse();
            double maxAng = numList[sortedIndices[0]];
            List<double> numListNormalized = new List<double>();
            for (int n = 0; n < numList.Count; n++) numListNormalized.Add(((numList[n] * range / maxAng)+lowValue));
            return numListNormalized;
        }

        //quicksort algorithm with list input
        internal static List<int> Quicksort(List<double> aList,List<int> indexList, int left, int right)
        {
            double[] a = new double[aList.Count];
            int[] index = new int[indexList.Count];

            for (int m = 0; m < aList.Count; m++)
            {
                a[m] = aList[m];
                index[m] = indexList[m];
            }

            if (right <= left) return null;
            int i = Partition(ref a, ref index, left, right);
            Quicksort(a, index, left, i - 1);
            Quicksort(a, index, i + 1, right);
            List<int> sortedIndex = new List<int>();
            for (int j = 0; j < index.Length; j++)
            {
                sortedIndex.Add(index[j]);
            }

            return sortedIndex;
        }



        //quicksort algorithm
        internal static List<int> Quicksort(double[] a, int[] index, int left, int right)
        {
            if (right <= left) return null;
            int i = Partition(ref a, ref index, left, right);
            Quicksort(a, index, left, i - 1);
            Quicksort(a, index, i + 1, right);
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
            double num = new Random().NextDouble();
            if (num >0.5) return 1;
            else return 0;
        }

        //toggle input value between 0 and 1
        internal static int ToggleInputInt(int value = 0)
        {
            if(value == 0)
            {
                return 1;
            }else
            {
                return 0;
            }
        }

        // partition a[left] to a[right], assumes left < right for Quicksort
        internal static int Partition(ref double[] a, ref int[] index,
        int left, int right)
        {
            int i = left - 1;
            int j = right;
            while (true)
            {
                while (IsLess(a[++i], a[right]));
                while (IsLess(a[right], a[--j]))    
                    if (j == left) break;           
                if (i >= j) break;                
                Exchange(a, index, i, j);              
            }
            Exchange(a, index, i, right);             
            return i;
        }
        
        //return lesser of the two values
        internal static bool IsLess(double x, double y)
        {
            return (x < y);
        }

        // exchange two indices in an array
        private static void Exchange(double[] a, int[] index, int i, int j)
        {
            double swap = a[i];
            a[i] = a[j];
            a[j] = swap;
            int b = index[i];
            index[i] = index[j];
            index[j] = b;
        }


        //binary search algorithm
        internal static int  BinarySearch(List<int> inputArray, int key)
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


        //binary search algo with double
        internal static int BinarySearchDouble(List<double> inputArray, double key)
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

        //cleans duplicate indices from a list
        internal static List<double> DuplicateIndexes(List<double> exprList)
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
                    if (dis == exprList[j])
                    {
                        break;
                    }
                }
            }
            return dups;

        }


        // test binary search
        internal static List<int> TestBinarySearch(List<int> inp, int key)
        {
            List<int> indices = new List<int>();
            List<int> inpList = new List<int>();
            for (int i = 0; i < inp.Count; i++)
            {
                inpList.Add(inp[i]);
            }
            int value = 0;
            int prevValue = 10000000;
            int m = 1;
            while (value != -1)
            {
                value = BinarySearch(inpList, key);
                if (value > -1)
                {
                    inpList.RemoveAt(value);
                    if (value >= prevValue)
                    {
                        indices.Add(value + 1);
                        m += 1;
                    }
                    else indices.Add(value);
                }
                prevValue = value;

            }// end of while loop
            return indices;
        }


        // test quick sort algorithm
        internal static List<int> TestQuickSort(double[] main = null, int[] index = null, int tag = 1)
        {
            int left = 0;
            int right = index.Length - 1;
            int[] newIndex = new int[index.Length];
            for (int i = 0; i < index.Length; i++)
            {
                newIndex[i] = index[i];
            }

            return Quicksort(main, newIndex, left, right);
        }



    }



}
