using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KeePassLib.Security;

namespace KeePass
{
    public static class KeePassLibWrapper
    {
        public static async Task<IKeePassDatabase> Load(IFile file, IFile keyFile, string password)
        {
            var compositeKey = new CompositeKey();

            if (password != null)
            {
                compositeKey.AddUserKey(new KcpPassword(password));
            }

            if (keyFile != null)
            {
                compositeKey.AddUserKey(new KcpKeyFile(await keyFile.ReadFileBytesAsync()));
            }

            var db = new PwDatabase
            {
                MasterKey = compositeKey
            };

            var kdbx = new KdbxFile(db);

            using (var fs = await file.OpenReadAsync())
            {
                await Task.Run(() =>
                {
                    kdbx.Load(fs, KdbxFormat.Default, new Logger());
                });

                return new WrapperDatabase(kdbx, db, file.IdFromPath());
            }
        }

        private sealed class WrapperDatabase : IKeePassDatabase
        {
            private readonly PwDatabase _db;
            private readonly KdbxFile _file;

            public WrapperDatabase(KdbxFile file, PwDatabase db, KeePassId id)
            {
                _file = file;
                _db = db;
                Id = id;
            }

            public IList<IKeePassIcon> Icons { get; } = Array.Empty<IKeePassIcon>();

            public KeePassId Id { get; }

            public string Name => _db.Name;

            public IKeePassGroup Root => new WrappedGroup(_db.RootGroup, null);

            public void Save(Stream stream)
            {
                _file.Save(stream, _db.RootGroup, KdbxFormat.Default, new Logger());
            }
        }

        private sealed class WrappedGroup : IKeePassGroup
        {
            private readonly PwGroup _group;

            public WrappedGroup(PwGroup group, IKeePassGroup parent)
            {
                _group = group;

                Id = group.Uuid;
                Parent = parent;
            }

            public IList<IKeePassEntry> Entries => _group.Entries
                .Select(e => new WrappedEntry(e, _group))
                .Cast<IKeePassEntry>()
                .ToList();

            public IList<IKeePassGroup> Groups => _group.Groups
                .Select(g => new WrappedGroup(g, this))
                .Cast<IKeePassGroup>()
                .ToList();

            public KeePassId Id { get; }

            public string Name => _group.Name;

            public string Notes => _group.Notes;

            public IKeePassGroup Parent { get; }
        }

        private sealed class WrappedEntry : IKeePassEntry
        {
            private readonly PwEntry _entry;
            private readonly PwGroup _group;

            public WrappedEntry(PwEntry entry, PwGroup group)
            {
                _entry = entry;
                _group = group;

                Id = entry.Uuid;
            }

            public IList<IKeePassAttachment> Attachment { get; } = Array.Empty<IKeePassAttachment>();

            public int? IconId { get; }

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
