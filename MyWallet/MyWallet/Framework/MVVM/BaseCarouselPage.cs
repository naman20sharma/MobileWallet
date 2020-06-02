using System;
using System.Threading.Tasks;
using Autofac;
using IdentifyMe.MVVM.Abstractions;
using Xamarin.Forms;

namespace IdentifyMe.MVVM
{
    public class BaseCarouselPage<T> : CarouselPage, IViewFor<T> where T : BaseNavigationViewModel
    {
        private T _viewModel;
        object IViewFor.ViewModel
        {
            get => _viewModel;
            set => ViewModel = (T)value;
        }
        
        public T ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                BindingContext = _viewModel;
                Task.Run(_viewModel.InitAsync).SafeAwait();
            }
        }

        protected BaseCarouselPage()
        {
            ViewModel = App.Container.Resolve<T>();
        }

        protected override void OnAppearing()
        {
            (BindingContext as T).OnAppearing();
            base.OnAppearing();
        }
    }
}
