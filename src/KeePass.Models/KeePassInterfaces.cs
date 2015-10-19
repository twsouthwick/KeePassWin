using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePass.Models
{
    [DebuggerDisplay("KeePass ID: {Id}")]
    public class KeePassId
    {
        private readonly string _id;

        public KeePassId(string id)
        {
            _id = id;
        }

        public string Id { get { return _id; } }

        public override bool Equals(object obj)
        {
            return string.Equals(obj, _id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _id;
        }

        public static implicit operator KeePassId(string id)
        {
            return new KeePassId(id);
        }

        public static explicit operator string(KeePassId id)
        {
            return id.Id;
        }
    }

    public interface IKeePassDatabase
    {
        KeePassId Id { get; }

        string Name { get; }

        IEnumerable<IKeePassIcon> Icons { get; }

        IKeePassIcon GetIcon(int idx);

        IList<IKeePassGroup> Groups { get; }
    }

    public interface IKeePassId
    {
        KeePassId Id { get; }
    }

    public interface IKeePassIcon : IKeePassId
    {
        byte[] Data { get; }
    }

    public interface IKeePassGroup : IKeePassId
    {
        string Name { get; }
        string Notes { get; }
        IList<IKeePassEntry> Entries { get; }
        IList<IKeePassGroup> Groups { get; }
    }

    public interface IKeePassEntry : IKeePassId
    {
        string UserName { get; }
        string Password { get; }
        int? IconId { get; }
        string Title { get; }
        string Notes { get; }
        IList<KeePassField> Fields { get; }
        IList<IKeePassAttachment> Attachment { get; }
        string Url { get; }
    }

    public interface IKeePassAttachment : IKeePassId
    {
    }
}
