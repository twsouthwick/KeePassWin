using ImageProcessorCore;
using System.Collections.Generic;
using System.IO;

namespace KeePass
{
    public interface IKeePassDatabase
    {
        KeePassId Id { get; }
        string Name { get; }
        IKeePassGroup Root { get; }
        void Save(Stream stream);
    }

    public interface IKeePassId
    {
        KeePassId Id { get; }
    }

    public interface IKeePassGroup : IKeePassId
    {
        string Name { get; }
        string Notes { get; }
        IKeePassGroup Parent { get; }
        IList<IKeePassEntry> Entries { get; }
        IList<IKeePassGroup> Groups { get; }
    }

    public interface IKeePassEntry : IKeePassId
    {
        string UserName { get; }
        string Password { get; }
        string Title { get; }
        string Notes { get; }
        IList<IKeePassAttachment> Attachment { get; }
        string Url { get; }
        Image Icon { get; }
    }

    public interface IKeePassAttachment : IKeePassId
    {
    }
}
