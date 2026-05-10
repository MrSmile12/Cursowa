using System;
using System.Diagnostics;
using System.Linq;

namespace SortingApp
{
    public class SortMetrics
    {
        public long Comparisons { get; set; } = 0;
        public long Swaps { get; set; } = 0;
        public TimeSpan ExecutionTime { get; set; }
    }

    public interface ISorter
    {
        string Name { get; }
        void Sort(int[] array, bool ascending, ref SortMetrics metrics);
    }

    public class HeapSort : ISorter
    {
        public string Name => "Heap Sort";

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int n = array.Length;

            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(array, n, i, ascending, ref metrics);

            for (int i = n - 1; i > 0; i--)
            {
                Swap(array, 0, i, ref metrics);
                Heapify(array, i, 0, ascending, ref metrics);
            }

            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void Heapify(int[] array, int n, int i, bool ascending, ref SortMetrics metrics)
        {
            int extreme = i; 
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n)
            {
                metrics.Comparisons++;
                bool condition = ascending ? array[left] > array[extreme] : array[left] < array[extreme];
                if (condition) extreme = left;
            }

            if (right < n)
            {
                metrics.Comparisons++;
                bool condition = ascending ? array[right] > array[extreme] : array[right] < array[extreme];
                if (condition) extreme = right;
            }

            if (extreme != i)
            {
                Swap(array, i, extreme, ref metrics);
                Heapify(array, n, extreme, ascending, ref metrics);
            }
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            if (i != j) metrics.Swaps++;
        }
    }

    public class IntroSort : ISorter
    {
        public string Name => "Intro Sort";

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int depthLimit = 2 * (int)Math.Floor(Math.Log(array.Length, 2));
            IntroSortRec(array, 0, array.Length - 1, depthLimit, ascending, ref metrics);
            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void IntroSortRec(int[] array, int start, int end, int depthLimit, bool ascending, ref SortMetrics metrics)
        {
            int size = end - start + 1;
            if (size < 16)
            {
                InsertionSort(array, start, end, ascending, ref metrics);
                return;
            }
            if (depthLimit == 0)
            {
                HeapSortFallback(array, start, end, ascending, ref metrics);
                return;
            }

            int pivot = Partition(array, start, end, ascending, ref metrics);
            IntroSortRec(array, start, pivot - 1, depthLimit - 1, ascending, ref metrics);
            IntroSortRec(array, pivot + 1, end, depthLimit - 1, ascending, ref metrics);
        }

        private int Partition(int[] array, int left, int right, bool ascending, ref SortMetrics metrics)
        {
            int pivot = array[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                metrics.Comparisons++;
                bool condition = ascending ? array[j] <= pivot : array[j] >= pivot;
                if (condition)
                {
                    i++;
                    Swap(array, i, j, ref metrics);
                }
            }
            Swap(array, i + 1, right, ref metrics);
            return i + 1;
        }

        private void InsertionSort(int[] array, int start, int end, bool ascending, ref SortMetrics metrics)
        {
            for (int i = start + 1; i <= end; i++)
            {
                int key = array[i];
                int j = i - 1;
                
                while (j >= start)
                {
                    metrics.Comparisons++;
                    bool condition = ascending ? array[j] > key : array[j] < key;
                    if (!condition) break;

                    array[j + 1] = array[j];
                    metrics.Swaps++;
                    j--;
                }
                array[j + 1] = key;
            }
        }

        private void HeapSortFallback(int[] array, int start, int end, bool ascending, ref SortMetrics metrics)
        {
            int n = end - start + 1;
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(array, n, i, start, ascending, ref metrics);

            for (int i = n - 1; i > 0; i--)
            {
                Swap(array, start, start + i, ref metrics);
                Heapify(array, i, 0, start, ascending, ref metrics);
            }
        }

        private void Heapify(int[] array, int n, int i, int offset, bool ascending, ref SortMetrics metrics)
        {
            int extreme = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n)
            {
                metrics.Comparisons++;
                if (ascending ? array[offset + left] > array[offset + extreme] : array[offset + left] < array[offset + extreme])
                    extreme = left;
            }
            if (right < n)
            {
                metrics.Comparisons++;
                if (ascending ? array[offset + right] > array[offset + extreme] : array[offset + right] < array[offset + extreme])
                    extreme = right;
            }
            if (extreme != i)
            {
                Swap(array, offset + i, offset + extreme, ref metrics);
                Heapify(array, n, extreme, offset, ascending, ref metrics);
            }
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            if (i != j) metrics.Swaps++;
        }
    }

    public class SmoothSort : ISorter
    {
        public string Name => "Smooth Sort";

        private readonly int[] LP = { 1, 1, 3, 5, 9, 15, 25, 41, 67, 109, 177, 287, 465, 753, 1219, 1973, 3193, 5167, 8361, 13529, 21891, 35421, 57313, 92735 };

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int n = array.Length;
            if (n <= 1) return;

            int p = 1;
            int b = 1;
            int c = 1;

            for (int i = 0; i < n; i++)
            {
                if ((p & 3) == 3)
                {
                    Sift(array, i, b, c, ascending, ref metrics);
                    p = (p >> 2) + 1;
                    int temp = b + c + 1; c = b; b = temp;
                }
                else if (n - i - 1 >= b)
                {
                    Trinkle(array, i, p, b, c, ascending, ref metrics);
                    p <<= 1;
                    while (b > 1) { int temp = b - c - 1; b = c; c = temp; p <<= 1; }
                    p++;
                }
            }
            Trinkle(array, n - 1, p, b, c, ascending, ref metrics);

            for (int i = n - 1; i > 0; i--)
            {
                if (b == 1)
                {
                    p--;
                    while (p > 0 && (p & 1) == 0)
                    {
                        p >>= 1;
                        int temp = b + c + 1; c = b; b = temp;
                    }
                }
                else
                {
                    p--;
                    int temp1 = b - c - 1; b = c; c = temp1;
                    p = (p << 1) + 1;
                    Trinkle(array, i - 1 - b, p, b, c, ascending, ref metrics);
                    p = (p << 1) + 1;
                    Trinkle(array, i - 1, p, b, c, ascending, ref metrics);
                }
            }

            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void Sift(int[] m, int r, int b, int c, bool ascending, ref SortMetrics metrics)
        {
            while (b >= 3)
            {
                int r2 = r - b + c;
                int r1 = r - 1;
                
                metrics.Comparisons++;
                bool cond1 = ascending ? m[r1] > m[r2] : m[r1] < m[r2];
                int r3 = cond1 ? r1 : r2;

                metrics.Comparisons++;
                bool cond2 = ascending ? m[r] >= m[r3] : m[r] <= m[r3];
                if (cond2) break;

                Swap(m, r, r3, ref metrics);

                if (r3 == r1) { r = r1; b = c; c = b - c - 1; }
                else { r = r2; int temp = b - c - 1; b = temp; c = c - b - 1; }
            }
        }

        private void Trinkle(int[] m, int r, int p, int b, int c, bool ascending, ref SortMetrics metrics)
        {
            while (p > 0)
            {
                while ((p & 1) == 0)
                {
                    p >>= 1;
                    int temp = b + c + 1; c = b; b = temp;
                }
                int r3 = r - b;
                if (p == 1) break;

                metrics.Comparisons++;
                if (ascending ? m[r3] <= m[r] : m[r3] >= m[r]) break;

                if (b == 1)
                {
                    Swap(m, r, r3, ref metrics);
                    r = r3;
                }
                else
                {
                    int r2 = r - b + c;
                    int r1 = r - 1;
                    
                    metrics.Comparisons++;
                    bool cond = ascending ? m[r1] > m[r2] : m[r1] < m[r2];
                    int r4 = cond ? r1 : r2;

                    metrics.Comparisons++;
                    if (ascending ? m[r3] >= m[r4] : m[r3] <= m[r4])
                    {
                        Swap(m, r, r3, ref metrics);
                        r = r3;
                    }
                    else
                    {
                        Swap(m, r, r4, ref metrics);
                        if (cond) { r = r1; b = c; c = b - c - 1; }
                        else { r = r2; int temp = b - c - 1; b = temp; c = c - b - 1; }
                        Sift(m, r, b, c, ascending, ref metrics);
                        break;
                    }
                }
                p--;
            }
            Sift(m, r, b, c, ascending, ref metrics);
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            if (i != j) metrics.Swaps++;
        }
    }

    public class QuickSort : ISorter
    {
        public string Name => "Quick Sort";

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics)
        {
            Stopwatch sw = Stopwatch.StartNew();
            SortProcess(array, 0, array.Length - 1, ascending, ref metrics);
            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void SortProcess(int[] array, int left, int right, bool ascending, ref SortMetrics metrics)
        {
            if (left < right)
            {
                int pivotIndex = Partition(array, left, right, ascending, ref metrics);
                SortProcess(array, left, pivotIndex - 1, ascending, ref metrics);
                SortProcess(array, pivotIndex + 1, right, ascending, ref metrics);
            }
        }

        private int Partition(int[] array, int left, int right, bool ascending, ref SortMetrics metrics)
        {
            int pivot = array[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                metrics.Comparisons++;
                bool condition = ascending ? array[j] <= pivot : array[j] >= pivot;

                if (condition)
                {
                    i++;
                    Swap(array, i, j, ref metrics);
                }
            }
            Swap(array, i + 1, right, ref metrics);
            return i + 1;
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            if (i != j) metrics.Swaps++;
        }
    }
}