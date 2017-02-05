using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

#pragma warning disable SA1402 // File may only contain a single class

namespace KeePass.Win.Mvvm
{
    [ContentProperty(Name = nameof(Matches))]
    public class TypeTemplateSelector : DataTemplateSelector
    {
        public ObservableCollection<TemplateMatch> Matches { get; set; } = new ObservableCollection<TemplateMatch>();

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item == null)
            {
                return null;
            }

            return Matches.FirstOrDefault(m => m.Type.GetTypeInfo().IsAssignableFrom(item.GetType().GetTypeInfo()))?.TemplateContent;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }

    public abstract class TemplateMatch
    {
        public abstract Type Type { get; }

        public DataTemplate TemplateContent { get; set; }
    }

    public class GroupTemplateMatch : TemplateMatch
    {
        public override Type Type { get; } = typeof(IKeePassGroup);
    }

    public class EntryTemplateMatch : TemplateMatch
    {
        public override Type Type { get; } = typeof(IKeePassEntry);
    }
}
