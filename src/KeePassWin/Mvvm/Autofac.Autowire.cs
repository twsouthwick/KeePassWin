using Autofac;
using Autofac.Core;
using System;
using System.Diagnostics;
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

        public static void SetAutowire(UserControl attached, bool value)
        {
            attached.SetValue(AutowireProperty, value);
        }

        public static bool GetAutowire(UserControl attached)
        {
            return (bool)attached.GetValue(AutowireProperty);
        }

        private static void AutowirePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var container = ((App)Application.Current).Container;

            container.InjectProperties(d, new MvvmPropertySelector(container));
        }

        private sealed class MvvmPropertySelector : IPropertySelector
        {
            private readonly IContainer _container;

            public MvvmPropertySelector(IContainer container)
            {
                _container = container;
            }

            public bool InjectProperty(PropertyInfo propertyInfo, object instance)
            {
                var requiresInjection = propertyInfo.GetCustomAttribute<InjectAttribute>() != null;

                if (requiresInjection)
                {
                    if (!_container.IsRegistered(propertyInfo.PropertyType))
                    {
                        throw new InvalidOperationException($"Requested type {propertyInfo.PropertyType} on {propertyInfo.DeclaringType} is not registered");
                    }

                    Debug.WriteLine($"Requested type {propertyInfo.PropertyType} on {propertyInfo.DeclaringType} will be injected");
                }


                return requiresInjection;
            }
        }
    }
}
