using NSubstitute;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace KeePass.Tests
{
    public class FileExtensionsTests
    {
        [Fact]
        public async Task ReadFileBytes()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 6, 5 };
            var file = Substitute.For<IFile>();

            file.OpenReadAsync().Returns(new MemoryStream(bytes));

            var result = await file.ReadFileBytesAsync();

            Assert.Equal(bytes, result);
        }

        [Theory]
        [InlineData("path1", "4263104c-d777-51ac-1eab-dd7f35c7e827")]
        [InlineData("PaTh1", "4263104c-d777-51ac-1eab-dd7f35c7e827")]
        [InlineData("o", "863f35a8-09a0-2270-f765-133ff440ef61")]
        [InlineData("O", "863f35a8-09a0-2270-f765-133ff440ef61")]
        public void CheckIdFromPath(string path, string id)
        {
            var file = Substitute.For<IFile>();

            file.Path.Returns(path);

            var result = file.IdFromPath();

            Assert.Equal(result.Id, Guid.Parse(id));
        }
    }
}
