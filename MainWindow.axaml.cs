using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SortingApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnSort_Click(object sender, RoutedEventArgs e)
        {
            BtnSort.IsEnabled = false;
            TextMetrics.Text = "Виконується...";

            if (!int.TryParse(TextSize.Text, out int size) || size <= 0) size = 1000;
            if (!int.TryParse(TextMin.Text, out int min)) min = -1000;
            if (!int.TryParse(TextMax.Text, out int max)) max = 1000;

            int algoIndex = ComboAlgorithm.SelectedIndex;
            int stateIndex = ComboState.SelectedIndex;
            bool ascending = ComboDirection.SelectedIndex == 0;

            ISorter sorter = algoIndex switch
            {
                1 => new HeapSort(),
                2 => new SmoothSort(),
                3 => new IntroSort(),
                _ => new QuickSort()
            };

            int[] originalArray = null;
            int[] sortedArray = null;
            SortMetrics metrics = new SortMetrics();

            await Task.Run(() =>
            {
                originalArray = GenerateArray(size, min, max, stateIndex);
                sortedArray = (int[])originalArray.Clone();
                sorter.Sort(sortedArray, ascending, ref metrics);
            });

            TextOriginal.Text = string.Join(" ", originalArray.Take(500)) + (size > 500 ? " ... (показано перші 500)" : "");
            TextSorted.Text = string.Join(" ", sortedArray.Take(500)) + (size > 500 ? " ... (показано перші 500)" : "");
            TextMetrics.Text = $"Час: {metrics.ExecutionTime.TotalMilliseconds:F4} мс | Порівнянь: {metrics.Comparisons} | Перестановок: {metrics.Swaps}";

            BtnSort.IsEnabled = true;
        }

        private int[] GenerateArray(int size, int min, int max, int state)
        {
            Random rnd = new Random();
            int[] arr = new int[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = rnd.Next(min, max + 1);
            }

            if (state == 1) Array.Sort(arr);
            if (state == 2) { Array.Sort(arr); Array.Reverse(arr); }

            return arr;
        }
    }
}