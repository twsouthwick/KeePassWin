using KeePass.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace KeePass.IO.Database
{
    public class DatabaseUnlockException : Exception
    {
        public DatabaseUnlockException(string message, Exception inner, bool known)
            : base(message, inner)
        {
            Known = known;
        }

        public bool Known { get; }
    }

    public class EncryptedDatabaseUnlocker : IDatabaseUnlocker
    {
        public virtual Task<IKeePassDatabase> UnlockAsync(IStorageFile file)
        {
            return UnlockAsync(file, null, null);
        }

        protected async Task<IKeePassDatabase> UnlockAsync(IStorageFile file, IStorageFile keyfile, string password)
        {
            try
            {
                using (var fs = await file.OpenReadAsync())
                {
                    var _password = new PasswordData { Password = password ?? string.Empty };

                    await _password.AddKeyFileAsync(keyfile);

                    // TODO: handle errors & display transformation progress
                    var result = await FileFormat.Headers(fs);
                    var headers = result.Headers;

                    var masterKey = await _password.GetMasterKey(headers);

                    using (var decrypted = await FileFormat.Decrypt(fs, masterKey, headers))
                    {
                        // TODO: verify start bytes
                        await FileFormat.VerifyStartBytes(decrypted, headers);

                        // Parse content
                        var doc = await FileFormat.ParseContent(decrypted, headers.UseGZip, headers);

                        // TODO: verify headers integrity

                        return new XmlKeePassDatabase(doc, "Id", Path.GetFileNameWithoutExtension(file.Name));
                    }
                }
            }
            catch (Exception e) when ((uint)e.HResult == 0x80070017)
            {
                throw new DatabaseUnlockException("Invalid password or key file", e, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                throw new DatabaseUnlockException("Unknown exception", e, false);
            }
        }
    }
}
