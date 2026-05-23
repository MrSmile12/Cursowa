using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

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
            BtnSave.IsEnabled = false;
            TextMetrics.Text = "Виконується...";
            TextDebug.Text = "";
            CanvasVisualization.Children.Clear();

            if (!int.TryParse(TextSize.Text, out int size) || size <= 0) size = 100;
            if (!int.TryParse(TextMin.Text, out int min)) min = 0;
            if (!int.TryParse(TextMax.Text, out int max)) max = 100;

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
                                              .Select(int.Parse).ToArray();
                    if (originalArray.Length == 0) throw new Exception();
                    size = originalArray.Length;
                }
                catch
                {
                    TextMetrics.Text = "Помилка: Введіть числа через пробіл!";
                    BtnSort.IsEnabled = true;
                    return;
                }
            }
            else
            {
                originalArray = GenerateArray(size, min, max, stateIndex);
            }

           
            bool useVisualization = CheckVisualize.IsChecked == true && size <= 300;
            bool useDebug = CheckDebug.IsChecked == true && size <= 100;

            if (CheckVisualize.IsChecked == true && size > 300) TextDebug.Text += "[СИСТЕМА]: Візуалізацію вимкнено (більше 300 елементів).\n";
            if (CheckDebug.IsChecked == true && size > 100) TextDebug.Text += "[СИСТЕМА]: Логування вимкнено (більше 100 елементів).\n";

            int stepCounter = 1;

            await Task.Run(() =>
            {
                sortedArray = (int[])originalArray.Clone();
                
               
                sorter.Sort(sortedArray, ascending, ref metrics, (currentArray) =>
                {
                    if (!useVisualization && !useDebug) return;

                    int[] clonedArray = (int[])currentArray.Clone(); 

                    Dispatcher.UIThread.Post(() =>
                    {
                        if (useVisualization) DrawVisualization(clonedArray);
                        if (useDebug) TextDebug.Text += $"Крок {stepCounter++}: {string.Join(" ", clonedArray)}\n";
                    });

                    
                    Thread.Sleep(useVisualization ? 25 : 5); 
                });
            });

            
            DrawVisualization(sortedArray);
            TextOriginal.Text = string.Join(" ", originalArray.Take(500)) + (size > 500 ? " ... (показано перші 500)" : "");
            TextSorted.Text = string.Join(" ", sortedArray.Take(500)) + (size > 500 ? " ... (показано перші 500)" : "");
            TextMetrics.Text = $"Час: {metrics.ExecutionTime.TotalMilliseconds:F4} мс | Порівнянь: {metrics.Comparisons} | Перестановок: {metrics.Swaps}";

            BtnSort.IsEnabled = true;
            BtnSave.IsEnabled = true;
        }

        
        private void DrawVisualization(int[] arr)
        {
            CanvasVisualization.Children.Clear();
            if (arr == null || arr.Length == 0) return;

            
            double width = CanvasVisualization.Bounds.Width;
            double height = CanvasVisualization.Bounds.Height;
            
            
            if (width == 0 || height == 0) 
            {
                width = 650; height = 400; 
            }

            double barWidth = width / arr.Length;
            int max = arr.Max();
            int min = arr.Min();
            
            double range = max - min;
            if (range == 0) range = 1;

            for (int i = 0; i < arr.Length; i++)
            {
                
                double normalizedHeight = ((arr[i] - min) / range) * (height - 20) + 10; 
                
                var rect = new Rectangle
                {
                    Fill = Brushes.SteelBlue,
                    Width = barWidth > 1 ? barWidth - 1 : barWidth, 
                    Height = normalizedHeight
                };

                Canvas.SetLeft(rect, i * barWidth);
                Canvas.SetBottom(rect, 0);

                CanvasVisualization.Children.Add(rect);
            }
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


                string cleanText = new string(input
                    .Where(c => char.IsDigit(c) || c == ' ' || c == '-' || c == ',' || c == ';').ToArray());


                if (input != cleanText)
                {
                    int caret = textBox.CaretIndex;
                    textBox.Text = cleanText;


                    textBox.CaretIndex = Math.Max(0, caret - 1);
                }
            }
        }

        
        private async void BtnCompare_Click(object sender, RoutedEventArgs e)
        {
            
            BtnSort.IsEnabled = false;
            BtnCompare.IsEnabled = false;
            BtnSave.IsEnabled = false;
            
            
            MainTabControl.SelectedIndex = 3; 
            TextComparison.Text = "Виконується порівняльний аналіз. Зачекайте...\n\n";

            
            if (!int.TryParse(TextSize.Text, out int size) || size <= 0) size = 1000;
            if (!int.TryParse(TextMin.Text, out int min)) min = -1000;
            if (!int.TryParse(TextMax.Text, out int max)) max = 1000;
            
            int stateIndex = ComboState.SelectedIndex;
            bool ascending = ComboDirection.SelectedIndex == 0;

            int[] originalArray = null;

            
            string manualText = TextManualArray.Text?.Trim();
            if (!string.IsNullOrEmpty(manualText))
            {
                try
                {
                    originalArray = manualText.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    if (originalArray.Length == 0) throw new Exception();
                }
                catch
                {
                    TextComparison.Text = "Помилка: Введіть числа через пробіл!";
                    BtnSort.IsEnabled = true; BtnCompare.IsEnabled = true;
                    return;
                }
            }
            else
            {
                originalArray = GenerateArray(size, min, max, stateIndex);
            }

            
            string resultText = $"=== ПОРІВНЯЛЬНИЙ АНАЛІЗ ({originalArray.Length} елементів) ===\n";
            
            string stateText = (ComboState.SelectedItem as ComboBoxItem)?.Content?.ToString() 
                               ?? ComboState.SelectedItem?.ToString() 
                               ?? "Невідомо";

            resultText += $"Стан масиву: {stateText}\n\n";
            
           
            resultText += string.Format("{0,-22} | {1,-15} | {2,-15} | {3,-15}\n", "Алгоритм", "Час (мс)", "Порівняння", "Перестановки");
            resultText += new string('-', 76) + "\n";

           
            var sorters = new ISorter[] { new QuickSort(), new HeapSort(), new IntroSort(), new SmoothSort() };

            await Task.Run(() =>
            {
                foreach (var sorter in sorters)
                {
                    
                    int[] arrCopy = (int[])originalArray.Clone();
                    SortMetrics metrics = new SortMetrics();

                    
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    
                    sorter.Sort(arrCopy, ascending, ref metrics, null);

                    
                    resultText += string.Format("{0,-22} | {1,-15:F4} | {2,-15} | {3,-15}\n", 
                        sorter.Name, 
                        metrics.ExecutionTime.TotalMilliseconds, 
                        metrics.Comparisons, 
                        metrics.Swaps);
                }
            });

            
            TextComparison.Text = resultText;
            
           
            BtnSort.IsEnabled = true;
            BtnCompare.IsEnabled = true;
            BtnSave.IsEnabled = true;
        }}
}
