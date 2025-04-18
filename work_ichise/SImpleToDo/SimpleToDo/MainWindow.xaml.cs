using System.Windows;
using System.Windows.Input;
using System.Data;
using SimpleToDo.components;
using SimpleToDo.models;
using SimpleToDo.utils;
using SimpleToDo.services;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace SimpleToDo
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

            // データベース接続文字列を取得
            string connectionStrings = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            // MySqlConnectionのファクトリメソッドを作成
            IDbConnection connectionFactory() => new MySqlConnection(connectionStrings);
            // DatabaseCrudManagerのインスタンスを作成
            dbManager = new DatabaseCrudManager(connectionFactory);

            LoadToDoData();
        }

        private void LoadToDoData()
        {
            DataTable dataTable = dbManager.ReadAllData("todo");

            List<ToDo> ToDoList = new DataConverter().ConvertDataTableToList(dataTable);

            ToDoList.ForEach(toDo =>
            {
                TaskItem taskItem = new(toDo, ToDoListBox, (table, id) => dbManager.DeleteRecord(table, id));
            });
        }

        private void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ToDo toDo = new() { Task = TextBoxInputTask.Text };
                        
            // データベースに追加
            long taskId = dbManager.CreateRecord(toDo.Table, toDo);

            // タスクアイテムの生成と追加
            TaskItem taskItem = new(toDo, ToDoListBox, (table, id) => dbManager.DeleteRecord(table, id));

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