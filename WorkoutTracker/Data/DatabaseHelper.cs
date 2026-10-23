using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace WorkoutTracker.Data
{
    public class DatabaseHelper
    {

        private const string V = "Data Source=DH-R207C04\\SQLEXPRESS;Initial Catalog=WorkoutDB;Integrated Security=True;TrustServerCertificate=True;";
        private readonly string _connectionString =
            V;

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public DataTable ExecuteQuery(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Ошибка базы данных: {ex.Message}\nНомер ошибки: {ex.Number}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Произошла ошибка: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return dataTable;
        }

        public int ExecuteNonQuery(string query)
        {
            int rowsAffected = -1;

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Ошибка базы данных: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Произошла ошибка: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return rowsAffected;
        }

        public DataTable ExecuteQueryWithParams(string query, List<KeyValuePair<string, object>> parameters)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}\n\n" +
                                $"Номер ошибки: {ex.Number}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }

            return dataTable;
        }

    }
}       