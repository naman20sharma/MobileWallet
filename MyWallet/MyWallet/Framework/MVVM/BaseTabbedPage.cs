using System;
using System.Threading.Tasks;
using IdentifyMe.MVVM.Abstractions;
using Xamarin.Forms;

namespace IdentifyMe.MVVM
{
    public class BaseTabbedPage<T> : TabbedPage, IViewFor<T> where T : BaseNavigationViewModel
    {
        object IViewFor.ViewModel
        {
            get => _viewModel;
            set => ViewModel = (T)value;
        }

        private T _viewModel;
        public T ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;

                BindingContext = _viewModel;

                Task.Run(async () => await Init()).SafeAwait();
            }
        }
        
        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            if (CurrentPage is IViewFor viewForPage
                && viewForPage.ViewModel is BaseNavigationViewModel viewModelForPage)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await viewModelForPage.InitAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Exception in {typeof(T).Name}.{nameof(OnCurrentPageChanged)}() {ex.Message}");
                        throw;
                    }
                });
            }
        }


        private async Task Init()
        {
            try
            {
                await ViewModel.InitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Exception in {typeof(T).Name}.{nameof(ViewModel.InitAsync)}() {ex.Message}");
                throw;
            }
        }
    }
}
