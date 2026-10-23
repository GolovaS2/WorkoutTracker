using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WorkoutTracker.Data;

namespace WorkoutTracker
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseHelper _dbHelper;
        private DataTable _allExercises = new DataTable();
        private bool _isLoadingCategories = false;

        public MainWindow()
        {
            InitializeComponent();

            _dbHelper = new DatabaseHelper();

            LoadCategories();
            LoadExercises("", "Все категории");
        }

        private void LoadCategories()
        {
            _isLoadingCategories = true;

            try
            {
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add("Все категории");

                string query = @"
                    SELECT DISTINCT Category
                    FROM Exercises
                    WHERE Category IS NOT NULL
                    ORDER BY Category;";

                DataTable categories = _dbHelper.ExecuteQuery(query);

                foreach (DataRow row in categories.Rows)
                {
                    cmbCategory.Items.Add(row["Category"].ToString());
                }

                cmbCategory.SelectedIndex = 0;
            }
            finally
            {
                _isLoadingCategories = false;
            }
        }

        private void LoadExercises(string searchText, string category)
        {
            string query = @"
                SELECT Id, Name, Category, MuscleGroup
                FROM Exercises";

            List<string> conditions = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                conditions.Add("Name LIKE @searchPattern");

                parameters.Add(new SqlParameter(
                    "@searchPattern",
                    SqlDbType.NVarChar,
                    200)
                {
                    Value = "%" + searchText.Trim() + "%"
                });
            }

            if (!string.IsNullOrWhiteSpace(category) &&
                category != "Все категории")
            {
                conditions.Add("Category = @category");

                parameters.Add(new SqlParameter(
                    "@category",
                    SqlDbType.NVarChar,
                    100)
                {
                    Value = category
                });
            }

            if (conditions.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }

            query += " ORDER BY Name;";

            DataTable exercises = _dbHelper.ExecuteQuery(
                query,
                parameters.ToArray());

            dgExercises.ItemsSource = exercises.DefaultView;

            tbRecordCount.Text = $"Записей: {exercises.Rows.Count}";

            if (exercises.Rows.Count > 0)
            {
                tbStatus.Text = "Данные загружены.";
            }
            else
            {
                tbStatus.Text = "Упражнения не найдены.";
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.Clear();

            if (cmbCategory.Items.Count > 0)
            {
                cmbCategory.SelectedIndex = 0;
            }

            LoadExercises("", "Все категории");
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = tbSearch.Text;
            string category = cmbCategory.SelectedItem?.ToString()
                              ?? "Все категории";

            LoadExercises(searchText, category);
        }

        private void CmbCategory_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (_isLoadingCategories || cmbCategory.SelectedItem == null)
            {
                return;
            }

            LoadExercises(
                tbSearch.Text,
                cmbCategory.SelectedItem.ToString());
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.Clear();

            if (cmbCategory.Items.Count > 0)
            {
                cmbCategory.SelectedIndex = 0;
            }

            LoadExercises("", "Все категории");
        }
    }
}