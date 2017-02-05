using System;
using System.Threading.Tasks;

namespace KeePass
{
    public interface IMessageDialogFactory
    {
        Task DatabaseAlreadyExistsAsync();

        Task InvalidCredentialsAsync();

        Task UnlockErrorAsync(string message);
        Task DatabaseSavedAsync();
        Task ErrorSavingDatabaseAsync(Exception e);
        Task<bool> CheckToDeleteAsync(string v, string name);
    }
}