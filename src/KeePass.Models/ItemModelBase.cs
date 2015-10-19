using System;
using System.Xml.Linq;

namespace KeePass.Models
{
    public class ItemModelBase
    {
        private readonly XElement _element;

        /// <summary>
        /// Gets the element containing the item's details.
        /// </summary>
        public XElement Element
        {
            get { return _element; }
        }

        /// <summary>
        /// Gets or sets element UUID.
        /// </summary>
        public string Id { get; set; }

        public ItemModelBase(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            _element = element;
            Id = (string)element.Element("UUID");
        }

        public ItemModelBase() {}
    }
}