using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FolderSize.Annotations;

namespace FolderSize;

public class FolderInfo : IFsInfo
{


    private double _percentage;
    private long _size;

    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            OnPropertyChanged(nameof(Percentage));
        }
    }

    public List<IFsInfo> FsObjects => SubFolders.Cast<IFsInfo>().Concat(Files).ToList();
    
    public bool DisplayTick { get; private set; }

    public List<FileWithPercentage> Files { get; set; }

    public FolderInfo(string baseFolder, ConcurrentDictionary<string, FolderInfo> dictionary, TokenBox token)
    {
        if (token.Token.IsCancellationRequested)
        {
            Task = Task.CompletedTask;
            return;
        }
        DisplayTick = false;
        OnPropertyChanged(nameof(DisplayTick));
        Name = baseFolder;
        DisplayName = new DirectoryInfo(baseFolder).Name;
        dictionary.TryAdd(Name, this);
        try
        {
            SubFolders = new ();
            Files = new();
            Task = Task.Run(async () =>
            {
                if (token.Token.IsCancellationRequested) return;
                try
                {
                    Files = Directory.GetFiles(baseFolder).OrderBy(f => f).Select(f => new FileWithPercentage(f)).ToList();
                    Size = Files.Select(f => f.Size).Sum();
                    Files.ForEach(f => f.CalculatePercentage(Size));
                    SizeString = FileSizeFormatter.FormatSize(Size);
                    OnPropertyChanged(nameof(SizeString));
                    SubFolders = Directory.GetDirectories(baseFolder).OrderBy(d => d).Select(d => new FolderInfo(d, dictionary, token)).ToList();

                    OnPropertyChanged(nameof(FsObjects));
                    
                    await Task.WhenAll(SubFolders.Select(async f =>
                    {
                        if (token.Token.IsCancellationRequested) return;
                        await f.Task.ConfigureAwait(false);
                        if (token.Token.IsCancellationRequested) return;
                        Interlocked.Add(ref _size, f.Size);

                        SizeString = FileSizeFormatter.FormatSize(Size);
                        OnPropertyChanged(nameof(SizeString));
                        UpdatePercentages();
                    })).ConfigureAwait(false);
                }
                catch
                {
                    SubFolders = new();
                    Files = new();
                }

                DisplayTick = true;
                OnPropertyChanged(nameof(DisplayTick));
            }, token.Token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Task = Task.CompletedTask;
        }
    }

    private void UpdatePercentages()
    {
        SubFolders.ForEach(a => a.CalculatePercentage(Size));
        Files.ForEach(a => a.CalculatePercentage(Size));
    }

    public List<FolderInfo> SubFolders { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }

    public long Size
    {
        get => _size;
        private set => _size = value;
    }

    public string SizeString { get; set; }

    public Task Task { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TokenBox
{
    public TokenBox(CancellationToken token)
    {
        Token = token;
    }
    public CancellationToken Token { get; private set; }
}