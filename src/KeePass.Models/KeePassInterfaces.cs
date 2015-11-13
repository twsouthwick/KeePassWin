using System.Collections.Generic;

namespace KeePass.Models
{
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
