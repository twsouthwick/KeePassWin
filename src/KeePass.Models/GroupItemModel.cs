using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace KeePass.Models
{
    public sealed class GroupItemModel
    {
        private readonly IKeePassGroup _group;
        private readonly IKeePassDatabase _db;

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string Name { get; set; }

        public KeePassId Id { get; set; }

        /// <summary>
        /// Gets or sets the group notes.
        /// </summary>
        public string Notes { get; set; }

        public GroupItemModel(IKeePassDatabase db, IKeePassGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            Name = group.Name;
            Id = group.Id;
            _group = group;
            _db = db;
        }

        //public GroupItemModel() { }

        /// <summary>
        /// Lists the entries of this group.
        /// </summary>
        /// <returns>The entries.</returns>
        public List<EntryItemModel> ListEntries()
        {
            return _group.Entries
                .Select(x => new EntryItemModel(_db, x))
                .ToList();
        }

        /// <summary>
        /// Lists the child groups of this group.
        /// </summary>
        /// <returns>The child groups.</returns>
        public List<GroupItemModel> ListGroups()
        {
            return _group.Groups
                .Select(x => new GroupItemModel(_db, x))
                .ToList();
        }
    }
}