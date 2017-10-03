using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass
{
    public enum DatabaseAction
    {
        Add,
        Open,
        Remove
    }

    public interface IDatabaseCache
    {
        Task<IKeePassDatabase> UnlockAsync(KeePassId id, ICredentialProvider credentialProvider);

        Task<IFile> AddDatabaseAsync(IFilePicker filePicker, bool autoOpen);

        Task<IFile> AddKeyFileAsync(IFile db, IFilePicker filePicker);

        Task<IEnumerable<IFile>> GetDatabaseFilesAsync();

        Task RemoveDatabaseAsync(IFile dbFile);

        IObservable<(DatabaseAction action, IFile file)> Databases { get; }
    }
}
