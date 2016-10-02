using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KeePass
{
    internal sealed class KdbxDatabase : IKeePassDatabase
    {
        private readonly PwDatabase _db;
        private readonly KdbxFile _file;

        public KdbxDatabase(KdbxFile file, PwDatabase db, KeePassId id)
        {
            _file = file;
            _db = db;
            Id = id;
        }

        public KeePassId Id { get; }

        public string Name => _db.Name;

        public IKeePassGroup Root => new KbdxGroup(_db.RootGroup, null, _db);

        public void Save()
        {

        }

        public void Save(Stream stream)
        {
            _file.Save(stream, _db.RootGroup, KdbxFormat.Default, null);
        }

        public bool Modified => _db.Modified;

        private sealed class KbdxGroup : KbdxId, IKeePassGroup
        {
            private readonly PwDatabase _db;
            private readonly PwGroup _group;
            private readonly Lazy<IList<IKeePassGroup>> _groups;
            private readonly Lazy<IList<IKeePassEntry>> _entries;

            public KbdxGroup(PwGroup group, IKeePassGroup parent, PwDatabase db)
                : base(group.Uuid)
            {
                _group = group;
                _db = db;

                Parent = parent;

                _entries = new Lazy<IList<IKeePassEntry>>(() => _group.Entries
                    .Select(e => new KbdxEntry(e, _db, this))
                    .Cast<IKeePassEntry>()
                    .ToObservableCollection());
                _groups = new Lazy<IList<IKeePassGroup>>(() => _group.Groups
                    .Select(g => new KbdxGroup(g, this, _db))
                    .Cast<IKeePassGroup>()
                    .ToObservableCollection());
            }

            public IList<IKeePassEntry> Entries => _entries.Value;

            public IList<IKeePassGroup> Groups => _groups.Value;

            public string Name => _group.Name;

            public string Notes => _group.Notes;

            public IKeePassGroup Parent { get; }

            public void RemoveEntry(IKeePassEntry entry)
            {
                var kbdxEntry = entry as KbdxEntry;

                Debug.Assert(kbdxEntry != null);
                if (kbdxEntry == null)
                {
                    return;
                }

                _group.Entries.Remove(kbdxEntry.Entry);
                _entries.Value.Remove(entry);
            }

            public IKeePassEntry AddEntry(IKeePassEntry entry)
            {
                var pwEntry = new PwEntry(true, true);

                if (!string.IsNullOrEmpty(entry.Title))
                {
                    pwEntry.Strings.Set(PwDefs.TitleField, new ProtectedString(true, entry.Title));
                }

                if (!string.IsNullOrEmpty(entry.UserName))
                {
                    pwEntry.Strings.Set(PwDefs.UserNameField, new ProtectedString(true, entry.UserName));
                }

                if (!string.IsNullOrEmpty(entry.Password))
                {
                    pwEntry.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, entry.Password));
                }

                if (!string.IsNullOrEmpty(entry.Notes))
                {
                    pwEntry.Strings.Set(PwDefs.NotesField, new ProtectedString(true, entry.Notes));
                }

                if (!string.IsNullOrEmpty(entry.Url))
                {
                    pwEntry.Strings.Set(PwDefs.UrlField, new ProtectedString(true, entry.Url));
                }

                _group.AddEntry(pwEntry, true);

                var wrapped = new KbdxEntry(pwEntry, _db, this);

                Entries.Add(wrapped);

                return wrapped;
            }

            public IKeePassGroup AddGroup(IKeePassGroup group)
            {
                var pwGroup = new PwGroup(true, true)
                {
                    Name = group.Name
                };

                _group.AddGroup(pwGroup, true, true);

                var wrapped = new KbdxGroup(pwGroup, Parent, _db);

                Groups.Add(wrapped);

                return wrapped;
            }
        }

        private abstract class KbdxId : IKeePassId, INotifyPropertyChanged
        {
            protected KbdxId(PwUuid id)
            {
                Id = new KeePassId(new Guid(id.UuidBytes));
            }

            public KeePassId Id { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void SetProperty<T>(ref T item, T value, IEqualityComparer<T> equalityComparer = null, [CallerMemberName]string name = null)
            {
                var comparer = equalityComparer ?? EqualityComparer<T>.Default;

                if (comparer.Equals(item, value))
                {
                    return;
                }

                item = value;

                NotifyPropertyChanged(name);
            }

            protected void NotifyPropertyChanged([CallerMemberName]string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private sealed class KbdxEntry : KbdxId, IKeePassEntry
        {
            private readonly PwDatabase _db;
            private readonly PwEntry _entry;

            public KbdxEntry(PwEntry entry, PwDatabase db, IKeePassGroup group)
                : base(entry.Uuid)
            {
                _entry = entry;
                _db = db;
                Group = group;
            }

            public PwEntry Entry => _entry;

            public IList<IKeePassAttachment> Attachment { get; } = Array.Empty<IKeePassAttachment>();

            public byte[] Icon
            {
                get { return _db.GetCustomIcon(_entry.CustomIconUuid); }
                set { }
            }

            public string Notes { get; set; } = string.Empty;

            public string Password
            {
                get { return Get(PwDefs.PasswordField); }
                set { Add(PwDefs.PasswordField, value); }
            }

            public string Title
            {
                get { return Get(PwDefs.TitleField); }
                set { Add(PwDefs.TitleField, value); }
            }

            public string Url
            {
                get { return Get(PwDefs.UrlField); }
                set { Add(PwDefs.UrlField, value); }
            }

            public string UserName
            {
                get { return Get(PwDefs.UserNameField); }
                set { Add(PwDefs.UserNameField, value); }
            }

            public IKeePassGroup Group { get; set; }

            private string Get(string def)
            {
                return _entry.Strings.Get(def)?.ReadString();
            }

            private void Add(string def, string value, [CallerMemberName]string name = null)
            {
                if (value == null)
                {
                    return;
                }

                _entry.Strings.Set(def, new ProtectedString(true, value));

                NotifyPropertyChanged(name);
            }
        }
    }

    internal static class ListExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }
    }
}
