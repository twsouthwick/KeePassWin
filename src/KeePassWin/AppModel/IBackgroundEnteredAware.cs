using System.Threading.Tasks;

namespace KeePass.Win.AppModel
{
    /// <summary>
    /// An abstraction that allows any service to hook into Application events via an Autofac extension method <see cref="AutofacExtensions.RegisterAppModel{TLimit, TActivatorData, TRegistrationStyle}(Autofac.Builder.IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle})"/> 
    /// </summary>
    internal interface IBackgroundEnteredAware
    {
        Task BackgroundEnteredAsync();
    }
}
