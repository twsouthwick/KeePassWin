using KeePass.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace KeePass.IO.Database
{
    public class UnknownDatabaseUnlockException : DatabaseUnlockException
    {
        public UnknownDatabaseUnlockException(Exception inner)
            : base($"Unknown error: {inner.Message}", inner)
        { }
    }

    public class DatabaseUnlockException : Exception
    {
        public DatabaseUnlockException(string message)
            : base(message)
        { }

        public DatabaseUnlockException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    public class EncryptedDatabaseUnlocker : IDatabaseUnlocker
    {
        public virtual Task<IKeePassDatabase> UnlockAsync(IFile file)
        {
            return UnlockAsync(file, null, null);
        }

        protected async Task<IKeePassDatabase> UnlockAsync(IFile file, IFile keyfile, string password)
        {
            try
            {
                using (var fs = await file.OpenReadAsync())
                {
                    var _password = new PasswordData { Password = password ?? string.Empty };

                    await _password.AddKeyFileAsync(keyfile);

                    // TODO: handle errors & display transformation progress
                    var result = await FileFormat.Headers(fs.AsInputStream());
                    var headers = result.Headers;

                    var masterKey = await _password.GetMasterKey(headers);

                    using (var decrypted = await FileFormat.Decrypt(fs.AsRandomAccessStream(), masterKey.ToArray() , headers))
                    {
                        // TODO: verify start bytes
                        await FileFormat.VerifyStartBytes(decrypted, headers);

                        // Parse content
                        var doc = await FileFormat.ParseContent(decrypted, headers.UseGZip, headers);

                        // TODO: verify headers integrity

                        return new XmlKeePassDatabase(doc, file.Path, Path.GetFileNameWithoutExtension(file.Name));
                    }
                }
            }
            catch (Exception e) when ((uint)e.HResult == 0x80070017)
            {
                throw new DatabaseUnlockException("Invalid password or key file", e);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                throw new UnknownDatabaseUnlockException(e);
            }
        }
    }
}
