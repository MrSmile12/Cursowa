using System;
using System.Diagnostics;


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

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int n = array.Length;

            
            for (int gap = n / 2; gap > 0; gap /= 2)
            {
                for (int i = gap; i < n; i++)
                {
                    int temp = array[i];
                    int j;

                    for (j = i; j >= gap; j -= gap)
                    {
                        metrics.Comparisons++;
                        
                        
                        bool condition = ascending 
                            ? array[j - gap] > temp 
                            : array[j - gap] < temp;

                        if (!condition) 
                            break;

                        array[j] = array[j - gap];
                        metrics.Swaps++;
                    }
                    
                    array[j] = temp;
                    
                    if (i != j) 
                    {
                        metrics.Swaps++;
                    }
                }
            }

            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
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