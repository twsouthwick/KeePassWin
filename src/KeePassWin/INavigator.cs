namespace KeePass.Win
{
    public interface INavigator
    {
        bool GoToDatabaseView(KeePassId database, KeePassId group);

        bool GoToMain();

        void GoBack();

        bool GoToSettings();
    }
}
