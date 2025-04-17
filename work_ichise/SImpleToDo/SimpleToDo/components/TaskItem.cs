using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace SimpleToDo.components
{
    public partial class TaskItem : DockPanel
    {
        private readonly ListBox _parentListBox;
        private readonly string _tableName;
        private readonly long _taskId;
        private static Action<string, long>? DeleteRecord { get; set;}

        public TaskItem(
            string tableName, 
            long taskId, 
            string? taskContent, 
            ListBox parentListBox, 
            Action<string, long>? ParentDeleteRecord)
        {
            _parentListBox = parentListBox;
            _tableName = tableName;
            _taskId = taskId;
            DeleteRecord = ParentDeleteRecord;

            // DockPanelの初期設定
            Height = 35;
            Width = 423;
            LastChildFill = false;

            // CheckBoxの生成
            CheckBox CheckBoxTask = new()
            {
                Content = taskContent ?? "",
                Height = 33,
                Width = 371,
            };

            // Buttonの生成
            Button DeleteButton = new()
            {
                Height = 33,
                Width = 52,
                BorderBrush = Brushes.Transparent, // 要素を透明化する
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
            };

            // Deleteアイコンの生成
            PackIcon DeleteIcon = new()
            {
                Kind = PackIconKind.Delete,
            };

            DeleteButton.Content = DeleteIcon;
            DeleteButton.Click += DeleteButton_Click;

            // DockPanelにコンポーネントを追加
            Children.Add(CheckBoxTask);
            Children.Add(DeleteButton);

            _parentListBox.Items.Add(this);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _parentListBox.Items.Remove(this);
            DeleteRecord?.Invoke(_tableName, _taskId);
        }
    }
}

