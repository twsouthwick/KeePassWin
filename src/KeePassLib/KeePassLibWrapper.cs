using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;

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

        public IKeePassGroup Root => new WrappedGroup(_db.RootGroup, null, _db);

        public void Save(Stream stream)
        {
            _file.Save(stream, _db.RootGroup, KdbxFormat.Default, new Logger());
        }

        private sealed class WrappedGroup : IKeePassGroup
        {
            private readonly PwDatabase _db;
            private readonly PwGroup _group;
            private readonly Lazy<IList<IKeePassGroup>> _groups;
            private readonly Lazy<IList<IKeePassEntry>> _entries;

            public WrappedGroup(PwGroup group, IKeePassGroup parent, PwDatabase db)
            {
                _group = group;
                _db = db;

                Id = new KeePassId(new Guid(group.Uuid.UuidBytes));
                Parent = parent;

                _entries = new Lazy<IList<IKeePassEntry>>(() => _group.Entries
                    .Select(e => new WrappedEntry(e, _db))
                    .Cast<IKeePassEntry>()
                    .ToObservableCollection());
                _groups = new Lazy<IList<IKeePassGroup>>(() => _group.Groups
                    .Select(g => new WrappedGroup(g, this, _db))
                    .Cast<IKeePassGroup>()
                    .ToObservableCollection());
            }

            public IList<IKeePassEntry> Entries => _entries.Value;

            public IList<IKeePassGroup> Groups => _groups.Value;

            public KeePassId Id { get; }

            public string Name => _group.Name;

            public string Notes => _group.Notes;

            public IKeePassGroup Parent { get; }

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

                var wrapped = new WrappedEntry(pwEntry, _db);

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

                var wrapped = new WrappedGroup(pwGroup, Parent, _db);

                Groups.Add(wrapped);

                return wrapped;
            }
        }

        private sealed class WrappedEntry : IKeePassEntry
        {
            private readonly PwDatabase _db;
            private readonly PwEntry _entry;

            public WrappedEntry(PwEntry entry, PwDatabase db)
            {
                _entry = entry;
                _db = db;

                Id = new KeePassId(new Guid(entry.Uuid.UuidBytes));
            }

            public IList<IKeePassAttachment> Attachment { get; } = Array.Empty<IKeePassAttachment>();

            public byte[] Icon => _db.GetCustomIcon(_entry.CustomIconUuid);

            public KeePassId Id { get; }

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

            private string Get(string def)
            {
                return _entry.Strings.Get(def)?.ReadString();
            }

            private void Add(string def, string value)
            {
                if (value == null)
                {
                    return;
                }

                _entry.Strings.Set(def, new ProtectedString(true, value));
            }
        }

        private sealed class Logger : IStatusLogger
        {
            public bool ContinueWork()
            {
                return true;
            }

            public void EndLogging()
            {
            }

            public bool SetProgress(uint uPercent)
            {
                return true;
            }

            public bool SetText(string strNewText, LogStatusType lsType)
            {
                return true;
            }

            public void StartLogging(string strOperation, bool bWriteOperationToLog)
            {
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
