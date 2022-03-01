using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using FolderSize.Annotations;

namespace FolderSize;

public sealed class FileWithPercentage:IFsInfo
{
    public FileWithPercentage(string path)
    {
        var fi = new FileInfo(path);
        SizeString = FileSizeFormatter.FormatSize(fi.Length);
        Size = fi.Length;
        Name = fi.Name;
    }
    
    public long Size { get;  }
    public string Name { get; }
    public string DisplayName => Name;
    public string SizeString { get; }
    private double _percentage;
    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            OnPropertyChanged(nameof(Percentage));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool DisplayTick => false;
}