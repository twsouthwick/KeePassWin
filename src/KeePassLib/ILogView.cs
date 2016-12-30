using System.ComponentModel;

namespace KeePass
{
    public interface ILogView : INotifyPropertyChanged
    {
        string Id { get; }

        string Log { get; }

        bool ShouldLogEvents { get; set; }
    }
}
