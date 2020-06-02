using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using IdentifyMe.MVVM.Abstractions;
using IdentifyMe.Resources;

namespace IdentifyMe.MVVM
{
    public class BaseNavigationViewModel : BaseViewModel
    {
        protected static T MakeVm<T>() => App.Container.Resolve<T>();

        protected static T MakeVm<T>(params object[] parameters) =>
            App.Container.Resolve<T>(parameters.Select((x, index) => new PositionalParameter(index, x)));

        private static INavigationService _registeredService;

        /// <summary>
        /// Gets or sets whether the ViewModel is modal.
        /// </summary>
        /// <value><c>true</c> if is modal; otherwise, <c>false</c>.</value>
        public bool IsModal { get; set; }

        protected INavigationService Navigation =>
            _registeredService ??= App.Container.Resolve<INavigationService>();


        /// <summary>
        /// Dismisses the ViewModel.
        /// </summary>
        /// <returns>The Task.</returns>
        public virtual Task Dismiss()
        {
            return IsModal
                ? Navigation.PopModalAsync()
                : Navigation.PopAsync();
        }

        public virtual void OnAppearing()
        {
        }

        protected static string GetString(string str, CultureInfo cultureInfo = null)
            => Localization.ResourceManager.GetString(str, cultureInfo ?? CultureInfo.InvariantCulture);
    }
}