using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace KeePass
{
    internal sealed class KdbxDatabase : IKeePassDatabase
    {
        private readonly PwDatabase _db;
        private readonly IFile _file;

        public KdbxDatabase(IFile file, PwDatabase db, KeePassId id)
        {
            _file = file;
            _db = db;
            Id = id;
        }

        public KeePassId Id { get; }

        public string Name => _db.Name;

        public IKeePassGroup Root => new KbdxGroup(_db.RootGroup, null, _db);

        public async Task SaveAsync()
        {
            using (var fs = await _file.OpenWriteAsync())
            {
                Save(fs);
                _db.Modified = false;
            }
        }

        public void Save(Stream stream)
        {
            var kdbx = new KdbxFile(_db);
            kdbx.Save(stream, null, KdbxFormat.Default, null);
        }

        public bool Modified => _db.Modified;

        private sealed class KbdxGroup : KbdxId, IKeePassGroup
        {
            private readonly PwGroup _group;
            private readonly Lazy<IList<IKeePassGroup>> _groups;
            private readonly Lazy<IList<IKeePassEntry>> _entries;

            public KbdxGroup(PwGroup group, IKeePassGroup parent, PwDatabase db)
                : base(group.Uuid, db)
            {
                _group = group;

                Parent = parent;

                _entries = new Lazy<IList<IKeePassEntry>>(() => _group.Entries
                    .Select(e => new KbdxEntry(e, db, this))
                    .Cast<IKeePassEntry>()
                    .ToObservableCollection());
                _groups = new Lazy<IList<IKeePassGroup>>(() => _group.Groups
                    .Select(g => new KbdxGroup(g, this, db))
                    .Cast<IKeePassGroup>()
                    .ToObservableCollection());
            }

            public IList<IKeePassEntry> Entries => _entries.Value;

            public IList<IKeePassGroup> Groups => _groups.Value;

            public string Name
            {
                get { return _group.Name; }
                set
                {
                    if (!string.Equals(Name, value, StringComparison.CurrentCulture))
                    {
                        _group.Name = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            public string Notes => _group.Notes;

            public IKeePassGroup Parent { get; set; }

            public IKeePassEntry CreateEntry(string title)
            {
                var pwEntry = new PwEntry(true, true);
                _group.AddEntry(pwEntry, true);
                Database.Modified = true;

                var wrapped = new KbdxEntry(pwEntry, Database, this)
                {
                    Title = title
                };

                Entries.Add(wrapped);

                return wrapped;
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
                Database.Modified = true;

                var wrapped = new KbdxEntry(pwEntry, Database, this);

                Entries.Add(wrapped);

                return wrapped;
            }

            public IKeePassGroup CreateGroup(string name)
            {
                var pwGroup = new PwGroup(true, true)
                {
                    Name = name
                };

                _group.AddGroup(pwGroup, true, true);

                var wrapped = new KbdxGroup(pwGroup, Parent, Database);

                Groups.Add(wrapped);
                Database.Modified = true;

                return wrapped;
            }

            public void Remove()
            {
                _group.ParentGroup.Groups.Remove(_group);

                // TODO: Use recycle bin?
                Database.DeletedObjects.Add(new PwDeletedObject(_group.Uuid, DateTime.Now));

                // Remove entry from parent group
                Parent.Groups.Remove(this);
                Parent = null;

                Database.Modified = true;
            }
        }

        private abstract class KbdxId : IKeePassId, INotifyPropertyChanged
        {
            protected KbdxId(PwUuid id, PwDatabase db)
            {
                Database = db;
                Id = new KeePassId(new Guid(id.UuidBytes));
            }

            public KeePassId Id { get; set; }

            public PwDatabase Database { get; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void SetProperty<T>(ref T item, T value, IEqualityComparer<T> equalityComparer = null, [CallerMemberName]string name = null)
            {
                var comparer = equalityComparer ?? EqualityComparer<T>.Default;

                if (comparer.Equals(item, value))
                {
                    return;
                }

                item = value;

                Database.Modified = true;

                NotifyPropertyChanged(name);
            }

            protected void NotifyPropertyChanged([CallerMemberName]string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private sealed class KbdxEntry : KbdxId, IKeePassEntry
        {
            private readonly PwEntry _entry;

            public KbdxEntry(PwEntry entry, PwDatabase db, IKeePassGroup group)
                : base(entry.Uuid, db)
            {
                _entry = entry;
                Group = group;
            }

            public IList<IKeePassAttachment> Attachment { get; } = Array.Empty<IKeePassAttachment>();

            public byte[] Icon
            {
                get { return Database.GetCustomIcon(_entry.CustomIconUuid); }
                set { }
            }

            public string Notes
            {
                get { return Get(PwDefs.NotesField); }
                set { Add(PwDefs.NotesField, value); }
            }

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

            public void Remove()
            {
                _entry.ParentGroup.Entries.Remove(_entry);

                // TODO: Use recycle bin?
                Database.DeletedObjects.Add(new PwDeletedObject(_entry.Uuid, DateTime.Now));

                // Remove entry from parent group
                Group.Entries.Remove(this);
                Group = null;

                Database.Modified = true;
            }

            private string Get(string def)
            {
                return _entry.Strings.Get(def)?.ReadString() ?? string.Empty;
            }

            private void Add(string def, string value, [CallerMemberName]string name = null)
            {
                if (value == null)
                {
                    return;
                }

                if (string.Equals(Get(def), value, StringComparison.Ordinal))
                {
                    return;
                }

                _entry.Strings.Set(def, new ProtectedString(true, value));
                Database.Modified = true;

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
