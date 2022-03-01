using System.ComponentModel;

namespace FolderSize;

public interface IFsInfo:INotifyPropertyChanged
{
    double Percentage { get; set; }
    bool DisplayTick { get; }
    string Name { get; }
    string SizeString { get; }
    
    string DisplayName { get; }
    
    long Size { get; }
}

public static class IFsInfoExtensions
{
    public static void CalculatePercentage(this IFsInfo f, long parentSize)
    {
        f.Percentage = f.Size / (double) parentSize * 100;
    }
}