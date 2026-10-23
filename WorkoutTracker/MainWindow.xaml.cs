using System;
using System.Data;
using System.Windows;
using WorkoutTracker.Data;

namespace WorkoutTracker
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper _dbHelper;

        private DataTable _allExercises;

        public MainWindow()
        {
            InitializeComponent();

            _dbHelper = new DatabaseHelper();

            tbStatus.Text = "Приложение запущено. Нажмите 'Загрузить упражнения'.";

            this.Title = "Персональный дневник тренировок v1.0";
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tbStatus.Text = "Загрузка данных из базы...";

                btnLoad.IsEnabled = false;

                string query = "SELECT Id, Name, Category, MuscleGroup FROM Exercises ORDER BY Name";

                DataTable exercisesTable = _dbHelper.ExecuteQuery(query);

                if (exercisesTable.Rows.Count > 0)
                {
                    dgExercises.ItemsSource = exercisesTable.DefaultView;

                    tbStatus.Text = $"Успешно загружено упражнений: {exercisesTable.Rows.Count}";
                }
                else
                {
                    MessageBox.Show("Таблица упражнений пуста.\n\n" +
                                    "Добавьте данные в базу данных через SQL Server Management Studio.",
                                    "Информация",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    tbStatus.Text = "Данные не найдены. Добавьте упражнения в БД.";

                    dgExercises.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке данных:\n\n{ex.Message}",
                                "Критическая ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                tbStatus.Text = "Ошибка загрузки данных";
            }
            finally
            {
                btnLoad.IsEnabled = true;
            }
        }
    }
}