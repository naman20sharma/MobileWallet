using System;
using System.Threading.Tasks;
using IdentifyMe.MVVM;
using IdentifyMe.MVVM.Abstractions;
using Xamarin.Forms;

namespace IdentifyMe.Framework.MVVM
{
    public class BaseMasterDetailsPage<T> : MasterDetailPage, IViewFor<T> where T : BaseNavigationViewModel
    {
        private T _viewModel;

        public T ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;

                BindingContext = _viewModel;

                Task.Run(Init).SafeAwait();
            }
        }
        
        object IViewFor.ViewModel
        {
            get => _viewModel;
            set => ViewModel = (T)value;
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
