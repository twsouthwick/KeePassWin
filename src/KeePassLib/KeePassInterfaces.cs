using System.Collections.Generic;
using System.IO;

namespace KeePass
{
    public interface IKeePassDatabase
    {
        KeePassId Id { get; }
        string Name { get; }
        IKeePassGroup Root { get; }
        void Save();
        bool Modified { get; }
    }

    public interface IKeePassId
    {
        KeePassId Id { get; set; }
    }

    public interface IKeePassGroup : IKeePassId
    {
        string Name { get; }
        string Notes { get; }
        IKeePassGroup Parent { get; }
        IList<IKeePassEntry> Entries { get; }
        IList<IKeePassGroup> Groups { get; }
        IKeePassEntry AddEntry(IKeePassEntry entry);
        IKeePassGroup AddGroup(IKeePassGroup group);
    }

    public interface IKeePassEntry : IKeePassId
    {
        string UserName { get; set; }
        string Password { get; set; }
        string Title { get; set; }
        string Notes { get; set; }
        //IList<IKeePassAttachment> Attachment { get; }
        string Url { get; set; }
        byte[] Icon { get; set; }
    }

    public interface IKeePassAttachment : IKeePassId
    {
    }
}
