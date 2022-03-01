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
  
  public partial class MainWindow : Window, IDisposable
  {
    private FolderInfo _folderInfo;
    private readonly ConcurrentDictionary<string, FolderInfo> _dictionary = new();
    private CancellationTokenSource? _cancellationTokenSource;
    
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
      FolderView.DataContext = _folderInfo;
      OverallSize.DataContext = _folderInfo;
      CurrentFolderText.DataContext = _folderInfo;
    }
    
    private readonly List<FolderInfo> _list = new();
    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (!_dictionary.TryGetValue((sender as Grid)?.Tag?.ToString()??string.Empty, out var folderInfo)) return;
     
      _list.Add(_folderInfo);
      
      UpdateBinding(folderInfo);
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