using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SocketHook.HostedWpfSample.Extensions;
using SocketHook.HostedWpfSample.Models;
using Image = System.Windows.Controls.Image;
// ReSharper disable PossibleNullReferenceException

namespace SocketHook.HostedWpfSample.Controls
{
    public class ProcessDataRow : DataGridRow
    {
        public int ProcessId { get; }
        public ProcessDataRow(ProcessTreeItem treeItem)
        {
            ProcessId = treeItem.ProcessId;
            Item = treeItem;
        }
    }

    public class ProcessTreeItem : TreeViewItem
    {
        private static readonly Brush _whiteBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f1f1"));
        private readonly ObservedProcess _process;

        public int ProcessId { get; }

        public ProcessTreeItem(ObservedProcess process)
        {
            var name = string.IsNullOrWhiteSpace(process.Name)
                ? process.Name
                : "none";

            _process = process;
            Header = CreateHeader();
            ProcessId = (int)process.ProcessId;
            CreateChild($"Executable Path: {process.FilePath}"); 
            CreateChild($"Process Name: {name}");
            CreateChild($"Process Id: {process.ProcessId}");
            CreateChild($"Priority: {process.Priority}");
        }

        protected TextBox CreateChild(string text)
        {
            var child = new TextBox
            {
                IsReadOnly = true,
                Foreground = _whiteBrush,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 10,
                Text = text
            };
            AddChild(child);
            return child;
        }

        private object CreateHeader()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Image {Source = _process.Icon.ToBitmapSource(), Height = 16, Width = 16},
                    new Label {Foreground = _whiteBrush, FontSize = 12, Content = _process.FileName}
                }
            };

            return panel;
        }
    }
}
