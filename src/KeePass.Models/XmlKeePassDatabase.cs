using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace KeePass.Models
{
    public class KeePassField
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsProtected { get; set; }
    }

    public class XmlKeePassDatabase : IKeePassDatabase
    {
        private readonly IKeePassGroup _root;
        private readonly IList<IKeePassIcon> _icons;
        private readonly KeePassId _id;
        private readonly string _name;

        public XmlKeePassDatabase(XDocument doc, KeePassId id, string name)
        {
            _id = id;
            _name = name;
            _root = doc.Descendants("Group")
                .Select(x => new XmlKeePassGroup(x))
                .Cast<IKeePassGroup>()
                .First();
            _icons = doc.Descendants("Icon")
                .Select(x => new XmlKeePassIcon(x))
                .Cast<IKeePassIcon>()
                .ToList();
        }

        public KeePassId Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<IKeePassIcon> Icons
        {
            get { return _icons; }
        }

        public IKeePassIcon GetIcon(int idx)
        {
            if (idx >= 0 && idx < _icons.Count)
            {
                return _icons[idx];
            }
            else
            {
                return null;
            }
        }

        public IKeePassGroup Root
        {
            get { return _root; }
        }

        public class XmlKeePassId : IKeePassId
        {
            private readonly KeePassId _id;

            public XmlKeePassId(XElement entry)
            {
                _id = entry.Element("UUID").Value;
            }

            public KeePassId Id { get { return _id; } }
        }

        public class XmlKeePassIcon : XmlKeePassId, IKeePassIcon
        {
            private readonly byte[] _data;

            public XmlKeePassIcon(XElement element)
                : base(element)
            {
                _data = Convert.FromBase64String((string)element.Element("Data"));
            }

            public byte[] Data { get { return _data; } }
        }

        [DebuggerDisplay("Entry '{Title}'")]
        public class XmlKeePassEntry : IKeePassEntry
        {
            private readonly KeePassId _id;
            private readonly string _title;
            private readonly string _username;
            private readonly string _password;
            private readonly string _notes;
            private readonly IList<KeePassField> _fields;
            private readonly string _url;
            private readonly int? _iconId = 0;

            private static int? GetIconId(XElement element)
            {
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

            public XmlKeePassEntry(XElement entry)
            {
                var strings = entry.Elements("String")
                    .ToLookup(x => (string)x.Element("Key"), x => (string)x.Element("Value"));

                _id = entry.Element("UUID").Value;
                _iconId = GetIconId(entry.Element("IconID"));
                _title = strings["Title"].FirstOrDefault();
                _username = strings["UserName"].FirstOrDefault();
                _password = strings["Password"].FirstOrDefault();
                _notes = strings["Notes"].FirstOrDefault();

                var url = new StringBuilder(strings["URL"].FirstOrDefault() ?? string.Empty);

                if (url.Length > 0)
                {
                    foreach (var item in strings)
                    {
                        var key = "{S:" + item.Key + "}";
                        url.Replace(key, item.First());
                    }
                }

                _url = url.ToString();

                _fields = entry.Elements("String")
                    .Select(x => new
                    {
                        Key = (string)x.Element("Key"),
                        Value = x.Element("Value"),
                    })
                    .Where(x => !IsStandardField(x.Key))
                    .Select(x => new KeePassField
                    {
                        Name = (string)x.Key,
                        Value = (string)x.Value,
                        IsProtected = IsProtected(x.Value),
                    })
                    .ToList();

                //      var attachments = entry
                //    .Elements("Binary")
                //    {
                //        Key = (string)x.Element("Key"),
                //        Value = x.Element("Value"),
                //    })
                //    .ToList();

                //var references = GetReferences(element);
                //foreach (var attachment in attachments.ToList())
                //{
                //    var value = attachment.Value;
                //    var reference = value.Attribute("Ref");
                //    if (reference == null)
                //        continue;

                //    // Referenced binary, update the reference
                //    var map = references.Value;
                //    value = map != null
                //        ? map[(string)reference].FirstOrDefault()
                //        : null;

                //    if (value != null)
                //    {
                //        attachment.Value = value;
                //        continue;
                //    }

                //    // Broken reference, do not display
                //    attachments.Remove(attachment);
                //}
            }

            private Lazy<ILookup<string, XElement>> GetReferences(XElement element)
            {
                return new Lazy<ILookup<string, XElement>>(() =>
                {
                    var root = element.Parent;
                    while (true)
                    {
                        if (root == null)
                            return null;

                        if (root.Name == "KeePassFile")
                            break;

                        root = root.Parent;
                    }

                    return root
                        .Element("Meta")
                        .Element("Binaries")
                        .Elements("Binary")
                        .Select(x => new
                        {
                            Element = x,
                            Id = x.Attribute("ID"),
                        })
                        .Where(x => x.Id != null)
                        .ToLookup(x => (string)x.Id, x => x.Element);
                });
            }

            private static bool IsProtected(XElement element)
            {
                var attr = element.Attribute("Protected");
                return attr != null && (bool)attr;
            }

            private static bool IsStandardField(string key)
            {
                switch (key)
                {
                    case "UserName":
                    case "Password":
                    case "URL":
                    case "Notes":
                    case "Title":
                        return true;
                }

                return false;
            }

            public KeePassId Id
            {
                get { return _id; }
            }

            public string UserName
            {
                get { return _username; }
            }

            public string Password
            {
                get { return _password; }
            }

            public string Title
            {
                get { return _title; }
            }

            public string Notes
            {
                get { return _notes; }
            }

            public IList<KeePassField> Fields
            {
                get { return _fields; }
            }


            public string Url
            {
                get { return _url; }
            }

            public IList<IKeePassAttachment> Attachment
            {
                get { return new List<IKeePassAttachment>(); }
            }

            public int? IconId
            {
                get { return _iconId; }
            }
        }

        [DebuggerDisplay("Group '{Name}'")]
        public class XmlKeePassGroup : IKeePassGroup
        {
            private readonly KeePassId _id;
            private readonly List<IKeePassEntry> _entries;
            private readonly List<IKeePassGroup> _groups;
            private readonly string _name;
            private readonly string _notes;

            public XmlKeePassGroup(XElement group)
            {
                _id = group.Element("UUID").Value;

                _entries = group.Elements("Entry")
                    .Select(x => new XmlKeePassEntry(x))
                    .Cast<IKeePassEntry>()
                    .ToList();

                _groups = group.Elements("Group")
                    .Select(x => new XmlKeePassGroup(x))
                    .Cast<IKeePassGroup>()
                    .ToList();

                _name = (string)group.Element("Name");
                _notes = (string)group.Element("Notes");
            }

            public KeePassId Id
            {
                get { return _id; }
            }

            public IList<IKeePassEntry> Entries
            {
                get { return _entries; }
            }

            public IList<IKeePassGroup> Groups
            {
                get { return _groups; }
            }


            public string Name
            {
                get { return _name; }
            }

            public string Notes
            {
                get { return _notes; }
            }
        }
    }
}
