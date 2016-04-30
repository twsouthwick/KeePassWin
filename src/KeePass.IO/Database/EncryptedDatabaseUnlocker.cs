using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace KeePass
{
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

                    var decrypted = await FileFormat.Decrypt(fs, masterKey.ToArray(), headers);

                    // Wrap in a stream so position is tracked
                    var stream = new MemoryStream(decrypted);

                    // TODO: Verify start bytes. This must be called even without validation so that
                    // the stream is correctly advanced
                    var startBytesCorrect = await FileFormat.VerifyStartBytes(stream, headers);

                    // Parse content
                    var doc = await FileFormat.ParseContent(stream, headers.UseGZip, headers);

                    // TODO: verify headers integrity

                    return new XmlKeePassDatabase(doc, file.Path, Path.GetFileNameWithoutExtension(file.Name));
                }
            }
            catch (Exception e) when ((uint)e.HResult == 0x80070017)
            {
                throw new DatabaseUnlockException("Invalid password or key file", e);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                throw new DatabaseUnlockException($"Unknown error: {e.Message}",e);
            }
        }
    }
}
