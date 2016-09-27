using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace KeePass
{
    public class XmlKeePassDatabase : IKeePassDatabase
    {
        private readonly XDocument _doc;

        public XmlKeePassDatabase(XDocument doc, KeePassId id, string name)
        {
            _doc = doc;

            Id = id;
            Name = name;
            Root = new XmlKeePassGroup(doc.Descendants("Group").First(), null);
        }

        public KeePassId Id { get; }

        public string Name { get; }

        public IKeePassGroup Root { get; }

        public void Save(Stream stream) => _doc.Save(stream);

        public class XmlKeePassId : IKeePassId
        {
            public XmlKeePassId(XElement entry)
            {
                Entry = entry;

                // The UUID won't be changed, so it may be cached
                Id = new KeePassId(Guid.Parse(entry.Element("UUID").Value));
            }

            protected XElement Entry { get; }

            public KeePassId Id { get; }
        }

        [DebuggerDisplay("Entry '{Title}'")]
        public class XmlKeePassEntry : XmlKeePassId, IKeePassEntry
        {
            public XmlKeePassEntry(XElement entry)
                : base(entry)
            { }

            public string UserName => GetString(nameof(UserName));

            public string Password => GetString(nameof(Password));

            public string Title => GetString(nameof(Title));

            public string Notes => GetString(nameof(Notes));

            public string Url => GetString("URL");

            public byte[] Icon { get; }

            public IList<IKeePassAttachment> Attachment { get; } = Array.Empty<IKeePassAttachment>();

            public int? IconId
            {
                get
                {
                    var element = Entry.Element(nameof(IconId));
                    if (element == null)
                    {
                        return null;
                    }

                    int icon;
                    if (int.TryParse((string)element, out icon))
                    {
                        return icon;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            private string GetString(string field)
            {
                var result = Entry.Elements("String")
                    .FirstOrDefault(e => string.Equals(field, e.Element("Key").Value, StringComparison.Ordinal));

                return result?.Element("Value").Value ?? string.Empty;
            }
        }

        [DebuggerDisplay("Group '{Name}'")]
        public class XmlKeePassGroup : XmlKeePassId, IKeePassGroup
        {
            public XmlKeePassGroup(XElement group, IKeePassGroup parent)
                : base(group)
            {
                Entries = group.Elements("Entry")
                    .Select(x => new XmlKeePassEntry(x))
                    .Cast<IKeePassEntry>()
                    .ToList()
                    .AsReadOnly();

                Groups = group.Elements("Group")
                    .Select(x => new XmlKeePassGroup(x, this))
                    .Where(x => !string.Equals(x.Name, "Recycle Bin", StringComparison.Ordinal))
                    .Cast<IKeePassGroup>()
                    .ToList()
                    .AsReadOnly();

                Parent = parent;
            }

            public IKeePassGroup Parent { get; }

            public IList<IKeePassEntry> Entries { get; }

            public IList<IKeePassGroup> Groups { get; }

            public string Name => Entry.Element("Name").Value;

            public string Notes => Entry.Element("Notes").Value;
        }
    }
}
