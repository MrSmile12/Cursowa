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
        // --- ФУНКЦІЯ ВАЛІДАЦІЇ (Тільки цифри та мінус) ---
        private void NumberValidation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string input = textBox.Text;
                
                // Дозволяємо порожній рядок або лише один мінус (для початку вводу від'ємних чисел)
                if (string.IsNullOrEmpty(input) || input == "-") return;

                // Якщо введено щось окрім числа (наприклад, букви)
                if (!int.TryParse(input, out _))
                {
                    // Фільтруємо текст: залишаємо тільки цифри, а мінус - лише якщо він на 1-й позиції
                    string cleanText = new string(input.Where((c, i) => char.IsDigit(c) || (i == 0 && c == '-')).ToArray());
                    
                    int caret = textBox.CaretIndex;
                    textBox.Text = cleanText;
                    
                    // Повертаємо курсор на правильне місце, щоб він не стрибав
                    textBox.CaretIndex = Math.Max(0, caret - 1); 
                }
            }
        }

        // --- ФУНКЦІЯ ЗБЕРЕЖЕННЯ У ФАЙЛ ---
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Формуємо текст для збереження
                string content = "=== РЕЗУЛЬТАТИ СОРТУВАННЯ ===\n";
                content += TextMetrics.Text + "\n";
                content += "-----------------------------------\n";
                content += "ЗГЕНЕРОВАНИЙ МАСИВ:\n";
                content += TextOriginal.Text + "\n\n";
                content += "ВІДСОРТОВАНИЙ МАСИВ:\n";
                content += TextSorted.Text + "\n";

                // Зберігаємо файл прямо на Робочий Стіл комп'ютера
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "Результати_Сортування.txt");

                await File.WriteAllTextAsync(filePath, content);

                // Даємо візуальний фідбек користувачу
                BtnSave.Content = "ЗБЕРЕЖЕНО НА РОБОЧИЙ СТІЛ!";
                await Task.Delay(2000); // Чекаємо 2 секунди
                BtnSave.Content = "ЗБЕРЕГТИ В ФАЙЛ";
            }
            catch (Exception ex)
            {
                TextMetrics.Text = $"Помилка збереження: {ex.Message}";
            }
        }
    }
}