using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ADONETHW2
{
    public partial class MainWindow : Window
    {
        SqlConnection connection = null;
        SqlDataReader? reader = null;
        DataTable? table = null;

        SqlDataAdapter? adapter = null;
        DataSet? dataSet = null;
        SqlCommandBuilder? cmdBuilder = null;
        public MainWindow()
        {
            InitializeComponent();
            ConnectDatabase();
        }
        private void ConnectDatabase()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
.AddJsonFile("jsconfig1.json")
.Build();
            string connectionString = configuration.GetConnectionString("db1");
            connection = new SqlConnection(connectionString);
        }

        private void btnFill_Click(object sender, RoutedEventArgs e)
        {
            string selectSQL = "SELECT * FROM Authors;";
            adapter = new SqlDataAdapter(selectSQL, connection);
            cmdBuilder = new SqlCommandBuilder(adapter);
            table = new DataTable();
            adapter.Fill(table);
            Authors.ItemsSource = table.AsDataView();
        }


        //ALTER PROCEDURE usp_updateAuthors
        //@aId int, @aFirstName nvarchar(max), @aLastName nvarchar(max)
        //AS
        //BEGIN
        //UPDATE Authors SET FirstName = @aFirstName, LastName = @aLastName WHERE Id = @aId
        //END
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand updateCommand = new()
            {
                CommandText = "usp_updateAuthors",
                CommandType = CommandType.StoredProcedure,
                Connection = connection
            };
            
            updateCommand.Parameters.Add("aId", SqlDbType.Int);
            updateCommand.Parameters["aId"].SourceVersion = DataRowVersion.Original;
            updateCommand.Parameters["aId"].SourceColumn = "Id";


            updateCommand.Parameters.Add("aFirstName", SqlDbType.NVarChar);
            updateCommand.Parameters["aFirstName"].SourceVersion = DataRowVersion.Current;
            updateCommand.Parameters["aFirstName"].SourceColumn = "FirstName";

            updateCommand.Parameters.Add("aLastName", SqlDbType.NVarChar);
            updateCommand.Parameters["aLastName"].SourceVersion = DataRowVersion.Current;
            updateCommand.Parameters["aLastName"].SourceColumn = "LastName";

            adapter.UpdateCommand = updateCommand;

            try
            {
                adapter.Update(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand insertCommand = new SqlCommand("INSERT Authors VALUES(@id, @firstName, @lastname)", connection);
            insertCommand.Parameters.Add("id", SqlDbType.Int);
            insertCommand.Parameters["id"].SourceVersion = DataRowVersion.Current;
            insertCommand.Parameters["id"].SourceColumn = "Id";

            insertCommand.Parameters.Add("firstName", SqlDbType.NVarChar);
            insertCommand.Parameters["firstName"].SourceVersion = DataRowVersion.Current;
            insertCommand.Parameters["firstName"].SourceColumn = "FirstName";

            insertCommand.Parameters.Add("lastName", SqlDbType.NVarChar);
            insertCommand.Parameters["lastName"].SourceVersion = DataRowVersion.Current;
            insertCommand.Parameters["lastName"].SourceColumn = "LastName";

            adapter.InsertCommand = insertCommand;
            try
            {
                adapter.Update(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRow? row = (Authors.SelectedItem as DataRowView)?.Row;
            if (row is null)
                return;
            row.Delete();
            adapter.Update(table);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (adapter is null) return;
            table?.Clear();
            string txt = SearchBox.Text;
            adapter.SelectCommand.CommandText = $"SELECT * FROM Authors WHERE UPPER(FirstName) LIKE UPPER('%{txt}%') OR UPPER(LastName) LIKE UPPER('%{txt}%')";
            adapter.Fill(table);
            Authors.ItemsSource = table.AsDataView();
            adapter.SelectCommand.CommandText = "SELECT * FROM Authors";
        }
    }
}
