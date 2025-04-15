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
using System.Diagnostics;
using MaterialDesignThemes.Wpf; // Material Design Package
using System.IO;
using System.Data;
using MySqlX.XDevAPI.Relational;
using ToDoList.components;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ToDoList.models;

namespace ToDoList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private readonly DatabaseCrudManager dbManager;

        public MainWindow()
        {
            InitializeComponent();
            dbManager = new DatabaseCrudManager();
            LoadToDoData();
        }

        private void InitializeDatabase()
        {
            dbManager.DatabaseConnection();
            dbManager.OpenConnection();
        }

        private void LoadToDoData()
        {
            InitializeDatabase();

            DataTable dataTable = dbManager.ReadAllData("todo");

            List<ToDo> toDoList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                ToDo toDo = new()
                {
                    Id = Convert.ToInt32(row["Id"]), 
                    Task = row["Task"].ToString()
                };
                toDoList.Add(toDo);
            }

            toDoList.ForEach(toDo =>
            {
                string table = toDo.Table; 
                long id = toDo.Id;
                string? task = toDo.Task;
                TaskItem taskItem = new(table, id, task, ListBoxToDoList, (table, id) => dbManager.DeleteRecord(table, id));
            });

            dbManager.CloseConnection();
        }

        private void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string task = TextBoxInputTask.Text;
            ToDo toDo = new() { Task = task };
                        
            // データベースに追加
            long taskId = dbManager.CreateRecord(toDo.Table, toDo);

            // タスクアイテムの生成と追加
            TaskItem taskItem = new(toDo.Table, taskId, task, ListBoxToDoList, (table, id) => dbManager.DeleteRecord(table, id));

            TextBoxInputTask.Text = "";            
        }

        private void TextBoxInputTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppendTaskButton_Click(sender, e);
            }
        }
    }
}