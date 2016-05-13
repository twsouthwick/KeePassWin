using System.Threading.Tasks;

namespace KeePassWin
{
    public interface IEntryView
    {
        Task ShowAsync();
    }
}
