namespace KeePass.Win
{
    public interface INavigator
    {
        bool GoToMain();

        void GoBack();

        bool GoToSettings();

        bool GoToDatabaseView(IKeePassDatabase db, IKeePassGroup keePassGroup);
    }
}
