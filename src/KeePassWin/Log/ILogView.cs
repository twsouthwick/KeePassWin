using System.ComponentModel;

namespace KeePass.Win.Log
{
    public interface ILogView : INotifyPropertyChanged
    {
        string Id { get; }

        string Log { get; }

        bool ShouldLogEvents { get; set; }
    }
}
