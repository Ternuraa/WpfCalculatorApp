using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfCalculatorApp
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5178") };
        public ObservableCollection<SavedNumber> Numbers { get; set; } = new ObservableCollection<SavedNumber>();

        public MainWindow()
        {
            InitializeComponent();
            SavedNumbersList.ItemsSource = Numbers;
            LoadSavedNumbers();
        }

        private async 
        Task LoadSavedNumbers()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("/numbers");
                var numbers = JsonConvert.DeserializeObject<SavedNumber[]>(response);
                Numbers.Clear();
                foreach (var number in numbers)
                {
                    Numbers.Add(number);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PerformOperation(string operation, double leftOperand, double rightOperand)
        {
            try
            {
                var payload = new
                {
                    leftOperand,
                    rightOperand
                };

                var response = await _httpClient.PostAsync($"/numbers/{operation}",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();

                var result = JsonConvert.DeserializeObject<OperationResult>(await response.Content.ReadAsStringAsync());
                ResultBlock.Text = $"Результат: {result.Result}";

                // Обновляем список сохраненных результатов
                await LoadSavedNumbers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения операции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Addition_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LeftOperand.Text, out var leftOperand) && double.TryParse(RightOperand.Text, out var rightOperand))
            {
                await PerformOperation("addition", leftOperand, rightOperand);
            }
            else
            {
                MessageBox.Show("Введите правильные числа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Subtraction_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LeftOperand.Text, out var leftOperand) && double.TryParse(RightOperand.Text, out var rightOperand))
            {
                await PerformOperation("subtraction", leftOperand, rightOperand);
            }
            else
            {
                MessageBox.Show("Введите правильные числа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Multiplication_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LeftOperand.Text, out var leftOperand) && double.TryParse(RightOperand.Text, out var rightOperand))
            {
                await PerformOperation("multiplication", leftOperand, rightOperand);
            }
            else
            {
                MessageBox.Show("Введите правильные числа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Division_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LeftOperand.Text, out var leftOperand) && double.TryParse(RightOperand.Text, out var rightOperand))
            {
                if (rightOperand == 0)
                {
                    MessageBox.Show("Нельзя делить на ноль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await PerformOperation("division", leftOperand, rightOperand);
            }
            else
            {
                MessageBox.Show("Введите правильные числа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SavedNumbersList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Проверяем, был ли выбран элемент
            if (SavedNumbersList.SelectedItem is SavedNumber selectedNumber)
            {
                // Записываем выбранное число в первое текстовое поле
                LeftOperand.Text = selectedNumber.Number.ToString();

                // Сбрасываем выделение, чтобы можно было выбрать это число снова
                SavedNumbersList.SelectedItem = null;
            }
        }
    }

    public class SavedNumber
    {
        public int Id { get; set; }
        public double Number { get; set; }
    }

    public class OperationResult
    {
        public int Id { get; set; }
        public double Result { get; set; }
    }
}