using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using SimpleToDo.models;

namespace SimpleToDo.components
{
    public partial class TaskItem : DockPanel
    {
        private readonly string _tableName = "todo";
        private readonly ListBox _parentListBox;
        public readonly ToDo _toDo;
        private static Action<string, long>? DeleteRecord { get; set; }
        private static Action<string, long, object>? UpdateRecord { get; set; }

        public TaskItem
        (
            ToDo toDo,
            string tableName,
            ListBox parentListBox,
            Action<string, long, object>? ParentUpdateRecord,
            Action<string, long>? ParentDeleteRecord
        )
        {
            _toDo = toDo;
            _tableName = tableName;
            _parentListBox = parentListBox;
            DeleteRecord = ParentDeleteRecord;
            UpdateRecord = ParentUpdateRecord;

            // DockPanelの初期設定
            Height = 35;
            Width = 423;
            LastChildFill = false;

            // CheckBoxの初期設定
            CheckBox CheckBoxTask = new()
            {
                Content = _toDo.Task ?? "",
                Height = 33,
                Width = 371,
                IsChecked = _toDo.Checked,
            };
            CheckBoxTask.Click += CheckBoxTask_Click;

            // Buttonの初期設定
            Button DeleteButton = new()
            {
                Height = 33,
                Width = 52,
                BorderBrush = Brushes.Transparent, // 要素を透明化する
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
            };

            // Deleteアイコンの初期設定
            PackIcon DeleteIcon = new()
            {
                Kind = PackIconKind.Delete,
            };

            DeleteButton.Content = DeleteIcon;
            DeleteButton.Click += DeleteButton_Click;

            // ContextMenuの初期設定
            ContextMenu TaskContextMenu = new();
            MenuItem EditMenuItem = new()
            {
                Header = "編集",
                Name = "Edit",                
            };
            EditMenuItem.Click += EditContextMenu_Click;
            TaskContextMenu.Items.Add(EditMenuItem);
           

            // DockPanelにコンポーネントを追加
            Children.Add(CheckBoxTask);
            Children.Add(DeleteButton);
            this.ContextMenu = TaskContextMenu;

            _parentListBox.Items.Add(this);
        }

        private void CheckBoxTask_Click(object sender, RoutedEventArgs e)
        {
            _toDo.Checked = ((CheckBox)sender).IsChecked ?? false;
            UpdateRecord?.Invoke(_tableName, _toDo.Id, new ToDo
            {
                Task = _toDo.Task,
                Checked = _toDo.Checked
            });
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _parentListBox.Items.Remove(this);
            DeleteRecord?.Invoke(_tableName, _toDo.Id);
        }

        private void EditContextMenu_Click(object sender, RoutedEventArgs e)
        {
              // taskContentを編集する機能
        }
    }
}