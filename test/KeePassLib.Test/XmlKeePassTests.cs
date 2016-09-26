using KeePass;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace KeePassLib
{
    public class XmlKeePassTests
    {
        [Fact]
        public async Task XmlDatabase1Load()
        {
            var id = new KeePassId(Guid.NewGuid());
            const string name = "name";

            using (var stream = await TestAssets.GetFile("db1.xml").OpenReadAsync())
            {
                var doc = XDocument.Load(stream);
                var db = new XmlKeePassDatabase(doc, id, name);

                Assert.Equal(name, db.Name);
                Assert.Equal(id, db.Id);

                Assert.Equal(6, db.Root.Groups.Count);

                // Root
                //Assert.Equal("WNQ4nLKdyU+ArNxms2gknA==", db.Root.Id);
                Assert.Equal("password_master_key", db.Root.Name);
                Assert.Equal(string.Empty, db.Root.Notes);
                Assert.Equal(2, db.Root.Entries.Count);

                // Root -> Entry 0
                //Assert.Equal("XNTyGZeauEqnxgySqvBh6g==", db.Root.Entries[0].Id);
                Assert.Equal("Sample Entry", db.Root.Entries[0].Title);
                Assert.Equal("Password", db.Root.Entries[0].Password);
                Assert.Equal("User Name", db.Root.Entries[0].UserName);
                Assert.Equal("Notes", db.Root.Entries[0].Notes);
                Assert.Equal("http://keepass.info/", db.Root.Entries[0].Url);

                // Root -> Entry 0
                //Assert.Equal("W/TJ1xfdZ0+NG66Sam4ICw==", db.Root.Entries[1].Id);
                Assert.Equal("Sample Entry #2", db.Root.Entries[1].Title);
                Assert.Equal("12345", db.Root.Entries[1].Password);
                Assert.Equal("Bob", db.Root.Entries[1].UserName);
                Assert.Equal(string.Empty, db.Root.Entries[1].Notes);
                Assert.Equal("http://keepass.info/help/kb/testform.html", db.Root.Entries[1].Url);

                // Root -> Group 0
                //Assert.Equal("mgllRBpv4EW48hbhbKtU3Q==", db.Root.Groups[0].Id);
                Assert.Equal("General", db.Root.Groups[0].Name);
                Assert.Equal(string.Empty, db.Root.Groups[0].Notes);

                // Root -> Group 1
                //Assert.Equal("nhv56+Zmd02C8P7ERo67RA==", db.Root.Groups[1].Id);
                Assert.Equal("Windows", db.Root.Groups[1].Name);
                Assert.Equal(string.Empty, db.Root.Groups[1].Notes);

                // Root -> Group 2
                //Assert.Equal("7D1DywfVmkWjG1Hi1Rrr5g==", db.Root.Groups[2].Id);
                Assert.Equal("Network", db.Root.Groups[2].Name);
                Assert.Equal(string.Empty, db.Root.Groups[2].Notes);

                // Root -> Group 3
                //Assert.Equal("O5sKKHXUOEOw6NxxW3/kPQ==", db.Root.Groups[3].Id);
                Assert.Equal("Internet", db.Root.Groups[3].Name);
                Assert.Equal(string.Empty, db.Root.Groups[3].Notes);

                // Root -> Group 4
                //Assert.Equal("KIMQeonlPEGVCKX4CXD37Q==", db.Root.Groups[4].Id);
                Assert.Equal("eMail", db.Root.Groups[4].Name);
                Assert.Equal(string.Empty, db.Root.Groups[4].Notes);

                // Root -> Group 5
                //Assert.Equal("Jdhmwj8Syk6VGYDWDxhXHw==", db.Root.Groups[5].Id);
                Assert.Equal("Homebanking", db.Root.Groups[5].Name);
                Assert.Equal(string.Empty, db.Root.Groups[5].Notes);
            }
        }

        [Fact]
        public async Task XmLDatabase1Save()
        {
            var id = new KeePassId(Guid.NewGuid());
            const string name = "name";

            using (var stream = await TestAssets.GetFile("db1.xml").OpenReadAsync())
            using (var reader = new StreamReader(stream))
            {
                var contents = reader.ReadToEnd();

                var doc = XDocument.Parse(contents);
                var db = new XmlKeePassDatabase(doc, id, name);

                var ms = new MemoryStream();
                db.Save(ms);
                ms.Position = 0;

                using (var resultReader = new StreamReader(ms))
                {
                    var result = resultReader.ReadToEnd();

                    Assert.Equal(contents, result);
                }
            }
        }

    }
}
