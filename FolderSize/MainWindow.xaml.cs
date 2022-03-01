using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using FolderSize.Annotations;

namespace FolderSize
{

  public class MainWindowDetails : INotifyPropertyChanged
  {

    private string _overallSizeText = string.Empty;
    private string _currentFolder;

    public string OverallSizeText
    {
      get => _overallSizeText;
      set
      {
        _overallSizeText = value;
        OnPropertyChanged(nameof(OverallSizeText));
      }
    }

    public string CurrentFolder
    {
      get => _currentFolder;
      set
      {
        _currentFolder = value;
        OnPropertyChanged(nameof(CurrentFolder));
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  
  public partial class MainWindow : Window, IDisposable
  {
    private FolderInfo _folderInfo;
    private readonly ConcurrentDictionary<string, FolderInfo> _dictionary = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly MainWindowDetails _windowDetails = new ();
    
    private Task? OutstandingTasks { get; set; }
    
    public MainWindow()
    {
      InitializeComponent();
      GatherData();
    }

    private void GatherData()
    {
      _cancellationTokenSource = new CancellationTokenSource();
      var path = @"C:\";

      var rootFolderInfo = new FolderInfo(path, _dictionary, new TokenBox(_cancellationTokenSource.Token));
      
      UpdateBinding(rootFolderInfo);
    }

    private void UpdateBinding(FolderInfo folderInfo)
    {
      _folderInfo = folderInfo;
      MyView.DataContext = _folderInfo;
      OverallSize.DataContext = _folderInfo;
      CurrentFolderText.DataContext = _folderInfo;
    }
    
    private readonly List<FolderInfo> _list = new();
    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (!_dictionary.TryGetValue((sender as Grid).Tag.ToString(), out var folderInfo)) return;
     
      _list.Add(_folderInfo);
      
      UpdateBinding(folderInfo);
      
      MyView.DataContext = _folderInfo;
    }

    private void FolderUp_OnClick(object sender, RoutedEventArgs e)
    {
      if (!_list.Any()) return;
      var folderInfo = _list.Last();
      _list.Remove(folderInfo);

      UpdateBinding(folderInfo);
    }

    private void Refresh_OnClick(object sender, RoutedEventArgs e)
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource?.Dispose();
      
      OutstandingTasks = null;
      
      UpdateBinding(null);
      
      _list.Clear();
      _dictionary.Clear();

      GatherData();
    }

    public void Dispose()
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource?.Dispose();
    }

    private void Explore_OnClick(object sender, RoutedEventArgs e)
    {
      Process.Start("explorer.exe", _folderInfo?.Name ?? @"C:\");
    }
  }
}