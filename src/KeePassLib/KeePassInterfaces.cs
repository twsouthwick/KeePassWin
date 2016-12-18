using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass
{
    public interface IKeePassDatabase
    {
        KeePassId Id { get; }
        string Name { get; }
        IKeePassGroup Root { get; }
        Task SaveAsync();
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
        IKeePassEntry CreateEntry(string title);
        IKeePassGroup CreateGroup(string name);
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

        IKeePassGroup Group { get; set; }

        void Remove();
    }

    public interface IKeePassAttachment : IKeePassId
    {
    }
}
