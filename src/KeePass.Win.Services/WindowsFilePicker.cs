using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace KeePass.Win.Services
{
    public class WindowsFilePicker : IFilePicker
    {
        public Task<IFile> GetDatabaseAsync() => OpenFileAsync(".kdbx", ".kdb");

        public Task<IFile> GetKeyFileAsync() => OpenFileAsync("*");

        private async Task<IFile> OpenFileAsync(params string[] extensions)
        {
            var picker = new FileOpenPicker();

            foreach (var extension in extensions)
            {
                picker.FileTypeFilter.Add(extension);
            }

            return (await picker.PickSingleFileAsync()).AsFile();
        }
    }
}
