using Autofac;
using Autofac.Core;
using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Mvvm
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
#pragma warning disable SA1402 // File may only contain a single class
    public sealed class InjectAttribute : Attribute
#pragma warning restore SA1402 // File may only contain a single class
    {
    }

    public static class Autofac
    {
        public static readonly DependencyProperty AutowireProperty = DependencyProperty.RegisterAttached("Autowire", typeof(bool), typeof(Autofac), new PropertyMetadata(false, AutowirePropertyChanged));

        private static readonly IPropertySelector s_propertySelector = new MvvmPropertySelector();

        public static void SetAutowire(Page attached, bool value)
        {
            attached.SetValue(AutowireProperty, value);
        }

        public static bool GetAutowire(Page attached)
        {
            return (bool)attached.GetValue(AutowireProperty);
        }

        private static void AutowirePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as Page;

            if (page == null)
            {
                return;
            }

            var container = ((App)Application.Current).Container;

            container.InjectProperties(d, s_propertySelector);
        }

        private sealed class MvvmPropertySelector : IPropertySelector
        {
            public bool InjectProperty(PropertyInfo propertyInfo, object instance)
            {
                return propertyInfo.GetCustomAttribute<InjectAttribute>() != null;
            }
        }
    }
}
