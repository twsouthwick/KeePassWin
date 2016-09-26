using KeePass;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KeePassLib
{
    public static class TestAssets
    {
        public static IFile GetFile(string name)
        {
            var assembly = typeof(TestAssets).GetTypeInfo().Assembly;
            var resource = assembly.GetManifestResourceNames()
                .Single(n => n.EndsWith($".{name}", StringComparison.Ordinal));

            return new EmbeddedResourceFile(name, resource, assembly);
        }

        public static IFile TryGetFile(string name)
        {
            try
            {
                return GetFile(name);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private class EmbeddedResourceFile : IFile
        {
            private readonly Assembly _assembly;

            public EmbeddedResourceFile(string name, string resource, Assembly assembly)
            {
                _assembly = assembly;

                Name = name;
                Path = resource;
            }

            public string Name { get; }

            public string Path { get; }

            public Task<Stream> OpenReadAsync()
            {
                return Task.FromResult(_assembly.GetManifestResourceStream(Path));
            }
        }
    }
}
