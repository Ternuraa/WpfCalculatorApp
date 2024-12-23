using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace WpfCalculatorApp.ViewModels
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        public CalculatorViewModel()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5178") };
            Numbers = new ObservableCollection<SavedNumber>();
            LoadSavedNumbersCommand = new RelayCommand(async _ => await LoadSavedNumbers());
            PerformOperationCommand = new RelayCommand(async operation => await PerformOperation(operation.ToString()));

            LoadSavedNumbersCommand.Execute(null);
        }

        // Свойства для ввода чисел
        private double _leftOperand;
        public double LeftOperand
        {
            get => _leftOperand;
            set
            {
                _leftOperand = value;
                OnPropertyChanged(nameof(LeftOperand));
            }
        }

        private double _rightOperand;
        public double RightOperand
        {
            get => _rightOperand;
            set
            {
                _rightOperand = value;
                OnPropertyChanged(nameof(RightOperand));
            }
        }

        // Результат
        private string _result;
        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        // Список сохраненных чисел
        public ObservableCollection<SavedNumber> Numbers { get; }

        // Команды
        public ICommand LoadSavedNumbersCommand { get; }
        public ICommand PerformOperationCommand { get; }

        // Загрузка сохранённых чисел
        private async Task LoadSavedNumbers()
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

        // Выполнение операции
        private async Task PerformOperation(string operation)
        {
            try
            {
                var payload = new
                {
                    leftOperand = LeftOperand,
                    rightOperand = RightOperand
                };

                var response = await _httpClient.PostAsync($"/numbers/{operation}",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();

                var result = JsonConvert.DeserializeObject<OperationResult>(await response.Content.ReadAsStringAsync());
                Result = $"{result.Result}";

                // Обновляем список сохранённых чисел
                await LoadSavedNumbers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения операции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SavedNumber _selectedNumber;
        public SavedNumber SelectedNumber
        {
            get => _selectedNumber;
            set
            {
                _selectedNumber = value;
                OnPropertyChanged(nameof(SelectedNumber));
                if (_selectedNumber != null)
                {
                    LeftOperand = _selectedNumber.Number; // Привязка к LeftOperand
                }
            }
        }
    }

    // Реализация RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}