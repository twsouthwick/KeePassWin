using System.Collections.Generic;

namespace KeePass
{
    public class ReadWriteKeePassEntry : IKeePassEntry
    {
        public IList<IKeePassAttachment> Attachment { get; } = new List<IKeePassAttachment>();

        public byte[] Icon { get; set; }

        public KeePassId Id { get; set; }

        public string Notes { get; set; }

        public string Password { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string UserName { get; set; }
    }
}
