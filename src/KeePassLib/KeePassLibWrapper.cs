using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            public WrappedGroup(PwGroup group, IKeePassGroup parent, PwDatabase db)
            {
                _group = group;
                _db = db;

                Id = new KeePassId(new Guid(group.Uuid.UuidBytes));
                Parent = parent;
            }

            public IList<IKeePassEntry> Entries => _group.Entries
                .Select(e => new WrappedEntry(e, _db))
                .Cast<IKeePassEntry>()
                .ToList();

            public IList<IKeePassGroup> Groups => _group.Groups
                .Select(g => new WrappedGroup(g, this, _db))
                .Cast<IKeePassGroup>()
                .ToList();

            public KeePassId Id { get; }

            public string Name => _group.Name;

            public string Notes => _group.Notes;

            public IKeePassGroup Parent { get; }
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

            public string Notes => string.Empty;

            public string Password => _entry.Strings.Get(PwDefs.PasswordField).ReadString();

            public string Title => _entry.Strings.Get(PwDefs.TitleField).ReadString();

            public string Url => _entry.Strings.Get(PwDefs.UrlField).ReadString();

            public string UserName => _entry.Strings.Get(PwDefs.UserNameField).ReadString();
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
}
