using System;
using System.Threading.Tasks;
using Autofac;
using IdentifyMe.MVVM.Abstractions;
using Xamarin.Forms;

namespace IdentifyMe.MVVM
{
    public abstract class BaseContentPage<T> : ContentPage, IViewFor<T> where T : BaseNavigationViewModel
    {
        protected BaseContentPage()
        {
            ViewModel = App.Container.Resolve<T>();
        }

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
                if (_viewModel == value) return;

                BindingContext = _viewModel = value;
                
                if (_viewModel is null) return;
                
                //Task.Run(Init).SafeAwait();;
                Task.Run(async () => await Init()).SafeAwait();

            }
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel?.OnAppearing();

        }

        private Task Init()
        {
            try
            {
                return ViewModel == null
                    ? Task.CompletedTask
                    : ViewModel.InitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Exception in {typeof(T).Name}.{nameof(ViewModel.InitAsync)}() {ex.Message}");
                throw;
            }
        }
    }
}
