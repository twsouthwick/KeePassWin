using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace KeePass.Win.Services
{
    public class WindowsFilePicker : IFilePicker
    {
        public Task<IFile> GetDatabaseAsync() => OpenFileAsync(".kdbx");

        public Task<IFile> GetKeyFileAsync() => OpenFileAsync("*");

        private async Task<IFile> OpenFileAsync(string extension)
        {
            var picker = new FileOpenPicker();

            picker.FileTypeFilter.Add(extension);

            return (await picker.PickSingleFileAsync()).AsFile();
        }
    }
}
