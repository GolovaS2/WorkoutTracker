using System;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using WorkoutTracker.Data;

namespace WorkoutTracker
{
    // Окно для добавления нового упражнения в справочник.
    public partial class AddExerciseWindow : Window
    {
        private DatabaseHelper _dbHelper;

        // Свойство, которое будет содержать результат добавления
        public bool IsAdded { get; private set; }

        public AddExerciseWindow()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();

            // Загружаем категории в ComboBox
            LoadCategories();

            // Устанавливаем фокус на поле названия при открытии окна
            Loaded += (s, e) => tbName.Focus();
        }

        // Загружает существующие категории в ComboBox.
        private void LoadCategories()
        {
            try
            {
                string query = "SELECT DISTINCT Category FROM Exercises ORDER BY Category";
                DataTable categoriesTable = _dbHelper.ExecuteQuery(query);

                // Очищаем ComboBox и добавляем пустую строку для подсказки
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(""); // Пустая строка как подсказка

                foreach (DataRow row in categoriesTable.Rows)
                {
                    cmbCategory.Items.Add(row["Category"].ToString());
                }

                // Выбираем пустую строку по умолчанию
                cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        // Обработчик нажатия на кнопку "Добавить".
        // Проверяет введенные данные и сохраняет новое упражнение в БД.
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Получаем данные из полей
                string name = tbName.Text.Trim();
                string category = cmbCategory.Text.Trim();
                string muscleGroup = tbMuscleGroup.Text.Trim();

                // 2. Валидация
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Введите название упражнения.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbName.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(category))
                {
                    MessageBox.Show("Введите категорию упражнения.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbCategory.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(muscleGroup))
                {
                    MessageBox.Show("Введите группу мышц.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbMuscleGroup.Focus();
                    return;
                }

                // 3. Проверка на дубликат (существует ли уже такое упражнение)
                string checkQuery = "SELECT COUNT(*) FROM Exercises WHERE Name = @name";
                var checkParams = new List<KeyValuePair<string, object>>
                {
                    new("@name", name)
                };

                DataTable checkResult = _dbHelper.ExecuteQueryWithParams(checkQuery, checkParams);
                int existingCount = 0;

                if (checkResult.Rows.Count > 0 && checkResult.Rows[0][0] != DBNull.Value)
                {
                    existingCount = Convert.ToInt32(checkResult.Rows[0][0]);
                }

                if (existingCount > 0)
                {
                    MessageBox.Show($"Упражнение '{name}' уже существует в базе данных.\n\nПожалуйста, введите другое название.",
                                    "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbName.Clear();
                    tbName.Focus();
                    return;
                }

                // 4. Формируем запрос на вставку
                string query = "INSERT INTO Exercises (Name, Category, MuscleGroup) VALUES (@name, @category, @muscleGroup)";

                var parameters = new List<KeyValuePair<string, object>>
                {
                    new("@name", name),
                    new("@category", category),
                    new("@muscleGroup", muscleGroup)
                };

                // 5. Выполняем запрос
                int rowsAffected = _dbHelper.ExecuteNonQueryWithParams(query, parameters);

                if (rowsAffected > 0)
                {
                    IsAdded = true;
                    MessageBox.Show($"Упражнение '{name}' успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Установка DialogResult автоматически закрывает окно, если оно открыто через ShowDialog()
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Не удалось добавить упражнение. Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик нажатия на кнопку "Отмена".
        // Закрывает окно без сохранения.
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsAdded = false;
            this.DialogResult = false; // Окно закроется автоматически
        }
    }
}
