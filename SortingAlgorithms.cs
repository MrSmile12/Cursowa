using System;
using System.Collections.Generic;
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
        // Додано Action<int[]> onStep для сповіщення про зміну стану масиву
        void Sort(int[] array, bool ascending, ref SortMetrics metrics, Action<int[]> onStep = null);
    }

    public class HeapSort : ISorter
    {
        public string Name => "Heap Sort";

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics, Action<int[]> onStep = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int n = array.Length;

            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(array, n, i, ascending, ref metrics, onStep);

            for (int i = n - 1; i > 0; i--)
            {
                Swap(array, 0, i, ref metrics, onStep);
                Heapify(array, i, 0, ascending, ref metrics, onStep);
            }

            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void Heapify(int[] array, int n, int i, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
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
                Swap(array, i, extreme, ref metrics, onStep);
                Heapify(array, n, extreme, ascending, ref metrics, onStep);
            }
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics, Action<int[]> onStep)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            if (i != j) 
            {
                metrics.Swaps++;
                onStep?.Invoke(array); // Сповіщаємо про зміну
            }
        }
    }

    public class IntroSort : ISorter
    {
        public string Name => "Intro Sort";

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics, Action<int[]> onStep = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int depthLimit = 2 * (int)Math.Floor(Math.Log(array.Length, 2));
            IntroSortRec(array, 0, array.Length - 1, depthLimit, ascending, ref metrics, onStep);
            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void IntroSortRec(int[] array, int start, int end, int depthLimit, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
        {
            int size = end - start + 1;
            if (size < 16)
            {
                InsertionSort(array, start, end, ascending, ref metrics, onStep);
                return;
            }
            if (depthLimit == 0)
            {
                HeapSortFallback(array, start, end, ascending, ref metrics, onStep);
                return;
            }

            int pivot = Partition(array, start, end, ascending, ref metrics, onStep);
            IntroSortRec(array, start, pivot - 1, depthLimit - 1, ascending, ref metrics, onStep);
            IntroSortRec(array, pivot + 1, end, depthLimit - 1, ascending, ref metrics, onStep);
        }

        private int Partition(int[] array, int left, int right, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
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
                    Swap(array, i, j, ref metrics, onStep);
                }
            }
            Swap(array, i + 1, right, ref metrics, onStep);
            return i + 1;
        }

        private void InsertionSort(int[] array, int start, int end, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
        {
            for (int i = start + 1; i <= end; i++)
            {
                int key = array[i];
                int j = i - 1;
                bool shifted = false;
                
                while (j >= start)
                {
                    metrics.Comparisons++;
                    bool condition = ascending ? array[j] > key : array[j] < key;
                    if (!condition) break;

                    array[j + 1] = array[j];
                    metrics.Swaps++;
                    onStep?.Invoke(array); // Сповіщаємо про зміщення
                    j--;
                    shifted = true;
                }
                array[j + 1] = key;
                if (shifted) onStep?.Invoke(array); // Сповіщаємо про встановлення ключа
            }
        }

        private void HeapSortFallback(int[] array, int start, int end, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
        {
            int n = end - start + 1;
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(array, n, i, start, ascending, ref metrics, onStep);

            for (int i = n - 1; i > 0; i--)
            {
                Swap(array, start, start + i, ref metrics, onStep);
                Heapify(array, i, 0, start, ascending, ref metrics, onStep);
            }
        }

        private void Heapify(int[] array, int n, int i, int offset, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
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
                Swap(array, offset + i, offset + extreme, ref metrics, onStep);
                Heapify(array, n, extreme, offset, ascending, ref metrics, onStep);
            }
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics, Action<int[]> onStep)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            if (i != j) 
            {
                metrics.Swaps++;
                onStep?.Invoke(array);
            }
        }
    }

    public class SmoothSort : ISorter
    {
        public string Name => "Smooth Sort";

        // Числа Леонардо для визначення розмірів дерев
        private readonly int[] L = { 1, 1, 3, 5, 9, 15, 25, 41, 67, 109, 177, 287, 465, 753, 1219, 1973, 3193, 5167, 8361, 13529, 21891, 35421, 57313, 92735, 150049, 242785, 392835, 635621, 1028457, 1664079, 2692537, 4356617, 7049155 };

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics, Action<int[]> onStep = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int n = array.Length;
            
            if (n < 2) 
            {
                sw.Stop();
                metrics.ExecutionTime = sw.Elapsed;
                return;
            }

            List<int> forest = new List<int>();

            // КРОК 1: Побудова лісу
            for (int i = 0; i < n; i++)
            {
                if (forest.Count >= 2 && forest[forest.Count - 2] == forest[forest.Count - 1] + 1)
                {
                    forest.RemoveAt(forest.Count - 1);
                    forest[forest.Count - 1]++; 
                }
                else
                {
                    forest.Add(forest.Count > 0 && forest[forest.Count - 1] == 1 ? 0 : 1);
                }
                
                Trinkle(array, forest, i, ascending, ref metrics, onStep);
            }

            // КРОК 2: Деконструкція лісу
            for (int i = n - 1; i > 0; i--)
            {
                int order = forest[forest.Count - 1];
                forest.RemoveAt(forest.Count - 1);

                if (order > 1)
                {
                    int rightRoot = i - 1;
                    int leftRoot = rightRoot - L[order - 2];

                    forest.Add(order - 1);
                    Trinkle(array, forest, leftRoot, ascending, ref metrics, onStep);

                    forest.Add(order - 2);
                    Trinkle(array, forest, rightRoot, ascending, ref metrics, onStep);
                }
            }

            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void Trinkle(int[] array, List<int> forest, int rootIndex, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
        {
            int currentRoot = rootIndex;
            int currentTreeIndex = forest.Count - 1;

            while (currentTreeIndex > 0)
            {
                int prevRoot = currentRoot - L[forest[currentTreeIndex]];

                // Якщо prevRoot більший (порушення порядку коренів для max-heap)
                if (ComparePairs(array, prevRoot, currentRoot, ascending, ref metrics))
                {
                    int order = forest[currentTreeIndex];
                    
                    // Перевіряємо дітей поточного дерева, щоб не нашкодити купі
                    if (order > 1)
                    {
                        int rightChild = currentRoot - 1;
                        int leftChild = rightChild - L[order - 2];

                        // Якщо дитина більша за prevRoot, зупиняємо переміщення по коренях
                        if (ComparePairs(array, leftChild, prevRoot, ascending, ref metrics) || 
                            ComparePairs(array, rightChild, prevRoot, ascending, ref metrics))
                        {
                            break;
                        }
                    }

                    Swap(array, prevRoot, currentRoot, ref metrics, onStep);
                    currentRoot = prevRoot;
                    currentTreeIndex--;
                }
                else
                {
                    break;
                }
            }
            
            SiftDown(array, forest[currentTreeIndex], currentRoot, ascending, ref metrics, onStep);
        }

        private void SiftDown(int[] array, int order, int rootIndex, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
        {
            int currentOrder = order;
            int currentRoot = rootIndex;

            while (currentOrder > 1)
            {
                int rightChild = currentRoot - 1;
                int leftChild = rightChild - L[currentOrder - 2];

                int maxChild = rightChild;
                int maxOrder = currentOrder - 2;

                if (ComparePairs(array, leftChild, rightChild, ascending, ref metrics))
                {
                    maxChild = leftChild;
                    maxOrder = currentOrder - 1; 
                }

                if (ComparePairs(array, maxChild, currentRoot, ascending, ref metrics))
                {
                    Swap(array, maxChild, currentRoot, ref metrics, onStep);
                    currentRoot = maxChild;
                    currentOrder = maxOrder;
                }
                else
                {
                    break;
                }
            }
        }

        private bool ComparePairs(int[] array, int i, int j, bool ascending, ref SortMetrics metrics)
        {
            metrics.Comparisons++;
            return ascending ? array[i] > array[j] : array[i] < array[j];
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics, Action<int[]> onStep)
        {
            if (i == j) return;
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            
            metrics.Swaps++;
            onStep?.Invoke(array);
        }
    }

    public class QuickSort : ISorter
    {
        public string Name => "Quick Sort";

        public void Sort(int[] array, bool ascending, ref SortMetrics metrics, Action<int[]> onStep = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            SortProcess(array, 0, array.Length - 1, ascending, ref metrics, onStep);
            sw.Stop();
            metrics.ExecutionTime = sw.Elapsed;
        }

        private void SortProcess(int[] array, int left, int right, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
        {
            if (left < right)
            {
                int pivotIndex = Partition(array, left, right, ascending, ref metrics, onStep);
                SortProcess(array, left, pivotIndex - 1, ascending, ref metrics, onStep);
                SortProcess(array, pivotIndex + 1, right, ascending, ref metrics, onStep);
            }
        }

        private int Partition(int[] array, int left, int right, bool ascending, ref SortMetrics metrics, Action<int[]> onStep)
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
                    Swap(array, i, j, ref metrics, onStep);
                }
            }
            Swap(array, i + 1, right, ref metrics, onStep);
            return i + 1;
        }

        private void Swap(int[] array, int i, int j, ref SortMetrics metrics, Action<int[]> onStep)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            if (i != j) 
            {
                metrics.Swaps++;
                onStep?.Invoke(array); 
            }
        }
    }
}