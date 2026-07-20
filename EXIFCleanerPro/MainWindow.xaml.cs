using EXIFCleanerPro.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EXIFCleanerPro;

public partial class MainWindow : Window
{
    private const double InspectorBreakpoint = 1050;
    private readonly MainViewModel viewModel;
    private bool isCompact;

    internal MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateResponsiveLayout(Width);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = !viewModel.IsBusy && e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (!viewModel.IsBusy && e.Data.GetData(DataFormats.FileDrop) is string[] paths)
        {
            await viewModel.AddDroppedPathsAsync(paths);
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) =>
        UpdateResponsiveLayout(e.NewSize.Width);

    private void UpdateResponsiveLayout(double width)
    {
        isCompact = width < InspectorBreakpoint;
        InspectorColumn.Width = isCompact ? new GridLength(0) : new GridLength(360);
        DesktopInspector.Visibility = !isCompact && viewModel.HasSelectedItem
            ? Visibility.Visible
            : Visibility.Collapsed;
        CompactDetailsButton.Visibility = isCompact && viewModel.HasSelectedItem
            ? Visibility.Visible
            : Visibility.Collapsed;
        if (!isCompact)
        {
            CompactInspectorPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.HasSelectedItem))
        {
            UpdateResponsiveLayout(ActualWidth);
        }
    }

    private void OnOpenCompactInspector(object sender, RoutedEventArgs e)
    {
        if (isCompact && viewModel.HasSelectedItem)
        {
            CompactInspectorPanel.Visibility = Visibility.Visible;
            SetCompactCloseButtonVisibility(Visibility.Visible);
        }
    }

    private void OnCloseCompactInspector(object sender, RoutedEventArgs e)
    {
        CompactInspectorPanel.Visibility = Visibility.Collapsed;
    }

    private void SetCompactCloseButtonVisibility(Visibility visibility)
    {
        CompactInspectorPanel.ApplyTemplate();
        if (FindVisualChild<Button>(CompactInspectorPanel, "CloseCompactInspectorButton") is Button closeButton)
        {
            closeButton.Visibility = visibility;
        }
    }

    private void OnQueueDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (isCompact && viewModel.HasSelectedItem)
        {
            OnOpenCompactInspector(sender, e);
        }
    }

    private void OnInspectorMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer || scrollViewer.ScrollableHeight <= 0)
        {
            return;
        }

        double targetOffset = Math.Clamp(
            scrollViewer.VerticalOffset - (e.Delta / 3.0),
            0,
            scrollViewer.ScrollableHeight);
        scrollViewer.ScrollToVerticalOffset(targetOffset);
        e.Handled = true;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
        {
            viewModel.AddFilesCommand.Execute(null);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.O)
        {
            viewModel.AddFolderCommand.Execute(null);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
        {
            viewModel.CleanCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && !viewModel.IsBusy)
        {
            viewModel.RemoveSelectedCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            if (CompactInspectorPanel.Visibility == Visibility.Visible)
            {
                CompactInspectorPanel.Visibility = Visibility.Collapsed;
            }
            else if (viewModel.IsBusy)
            {
                viewModel.CancelCleaningCommand.Execute(null);
            }

            e.Handled = true;
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!viewModel.IsBusy)
        {
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            "A cleaning run is still active. Stop after the current image and close?",
            "Cleaning in progress",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (result == MessageBoxResult.No)
        {
            e.Cancel = true;
        }
        else
        {
            viewModel.CancelCleaningCommand.Execute(null);
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild && typedChild.Name == name)
            {
                return typedChild;
            }

            T? nested = FindVisualChild<T>(child, name);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }
}
