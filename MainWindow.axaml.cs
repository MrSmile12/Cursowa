using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
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

           
            string manualText = TextManualArray.Text?.Trim();
            
            if (!string.IsNullOrEmpty(manualText))
            {
                try
                {
                    
                    originalArray = manualText.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();
                    
                    if (originalArray.Length == 0) throw new Exception();
                    size = originalArray.Length; 
                }
                catch
                {
                    TextMetrics.Text = "Помилка: Введіть числа через пробіл (наприклад: 5 -2 10)!";
                    BtnSort.IsEnabled = true;
                    return; 
                }
            }
            else
            {
                
                originalArray = GenerateArray(size, min, max, stateIndex);
            }

            
            await Task.Run(() =>
            {
                sortedArray = (int[])originalArray.Clone();
                sorter.Sort(sortedArray, ascending, ref metrics);
            });

            TextOriginal.Text = string.Join(" ", originalArray.Take(500)) + (size > 500 ? " ... (показано перші 500)" : "");
            TextSorted.Text = string.Join(" ", sortedArray.Take(500)) + (size > 500 ? " ... (показано перші 500)" : "");
            TextMetrics.Text = $"Час: {metrics.ExecutionTime.TotalMilliseconds:F4} мс | Порівнянь: {metrics.Comparisons} | Перестановок: {metrics.Swaps}";

            BtnSort.IsEnabled = true;
            BtnSave.IsEnabled = true;
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
        
        private void NumberValidation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string input = textBox.Text;
                
                
                if (string.IsNullOrEmpty(input) || input == "-") return;

                
                if (!int.TryParse(input, out _))
                {
                    
                    string cleanText = new string(input.Where((c, i) => char.IsDigit(c) || (i == 0 && c == '-')).ToArray());
                    
                    int caret = textBox.CaretIndex;
                    textBox.Text = cleanText;
                    
                    
                    textBox.CaretIndex = Math.Max(0, caret - 1); 
                }
            }
        }

        
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string content = "=== РЕЗУЛЬТАТИ СОРТУВАННЯ ===\n";
                content += TextMetrics.Text + "\n";
                content += "-----------------------------------\n";
                content += "ЗГЕНЕРОВАНИЙ МАСИВ:\n";
                content += TextOriginal.Text + "\n\n";
                content += "ВІДСОРТОВАНИЙ МАСИВ:\n";
                content += TextSorted.Text + "\n";

                
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "Результати_Сортування.txt");

                await File.WriteAllTextAsync(filePath, content);

               
                BtnSave.Content = "ЗБЕРЕЖЕНО НА РС!";
                await Task.Delay(2000);
                BtnSave.Content = "ЗБЕРЕГТИ В ФАЙЛ";
            }
            catch (Exception ex)
            {
                TextMetrics.Text = $"Помилка збереження: {ex.Message}";
                
            }
            
        }
        
        private void ArrayInputValidation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string input = textBox.Text;
                if (string.IsNullOrEmpty(input)) return;

                
                string cleanText = new string(input.Where(c => char.IsDigit(c) || c == ' ' || c == '-' || c == ',' || c == ';').ToArray());
                
                
                if (input != cleanText)
                {
                    int caret = textBox.CaretIndex;
                    textBox.Text = cleanText;
                    
                    
                    textBox.CaretIndex = Math.Max(0, caret - 1);
                }
            }
        }}
}