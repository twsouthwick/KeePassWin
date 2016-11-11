using KeePass;

namespace KeePassWin.Mvvm
{
    public interface INavigator
    {
        bool GoToDatabaseView(KeePassId database, KeePassId group);
        bool GoToMain();
        void GoBack();
        bool GoToSearch(KeePassId id, string text);
        bool GoToSettings();
    }
}
