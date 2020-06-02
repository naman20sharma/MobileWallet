using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using IdentifyMe.MVVM.Abstractions;

namespace IdentifyMe.MVVM
{
    public class BasePopupPage<T> : PopupPage, IViewFor<T> where T : BaseNavigationViewModel
    {
        private TaskCompletionSource<T> CompletionSource = new TaskCompletionSource<T>();

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set
            {
                HasSystemPadding = false;
                BindingContext = ViewModel = value as T;
            }
        }

        public T ViewModel { get; set; }

        public Task<T> Task => CompletionSource?.Task;

        protected override void OnDisappearing() => CompletionSource.SetResult(ViewModel);
    }
}