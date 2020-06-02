using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Rg.Plugins.Popup.Services;
using IdentifyMe.MVVM.Abstractions;
using Xamarin.Forms;

namespace IdentifyMe.MVVM
{
    public class NavigationService : INavigationService
    {
        INavigation FormsNavigation => Application.Current.MainPage.Navigation;

        // View model to view lookup - making the assumption that view model to view will always be 1:1
        readonly Dictionary<Type, Type> _viewModelViewDictionary = new Dictionary<Type, Type>();

        #region Replace

        // Because we're going to do a hard switch of the page, either return
        // the detail page, or if that's null, then the current main page       
        Page DetailPage
        {
            get
            {
                var masterController = Application.Current.MainPage as MasterDetailPage;
                return masterController?.Detail ?? Application.Current.MainPage;
            }
            set
            {
                if (Application.Current.MainPage is MasterDetailPage masterController)
                {
                    masterController.Detail = value;
                    masterController.IsPresented = false;
                }
                else
                {
                    Application.Current.MainPage = value;
                }
            }
        }

        /// <inheritdoc />
        public void SwitchDetailPage(BaseNavigationViewModel viewModel)
        {
            var view = InstantiateView(viewModel);

            Page newDetailPage;

            // Tab pages shouldn't go into navigation pages
            if (view is TabbedPage)
                newDetailPage = (Page) view;
            else
                newDetailPage = new NavigationPage((Page) view);

            DetailPage = newDetailPage;
        }

        /// <inheritdoc />
        public void SwitchDetailPage<T>(Action<T> initialize = null) where T : BaseNavigationViewModel
        {
            T viewModel;

            // First instantiate the view model
            viewModel = App.Container.Resolve<T>();
            initialize?.Invoke(viewModel);

            // Actually switch the page
            SwitchDetailPage(viewModel);
        }

        #endregion

        #region Registration

        /// <inheritdoc />
        public void RegisterViewModels(System.Reflection.Assembly asm)
        {
            // Loop through everything in the assembley that implements IViewFor<T>
            foreach (var type in asm.DefinedTypes.Where(dt => !dt.IsAbstract &&
                dt.ImplementedInterfaces.Any(ii => ii == typeof(IViewFor))))
            {

                // Get the IViewFor<T> portion of the type that implements it
                var viewForType = type.ImplementedInterfaces.FirstOrDefault(
                    ii => ii.IsConstructedGenericType &&
                    ii.GetGenericTypeDefinition() == typeof(IViewFor<>));

                // Register it, using the T as the key and the view as the value
                Register(viewForType.GenericTypeArguments[0], type.AsType());
            }
        }

        /// <inheritdoc />
        public void Register(Type viewModelType, Type viewType)
        {
            if (!_viewModelViewDictionary.ContainsKey(viewModelType))
                _viewModelViewDictionary.Add(viewModelType, viewType);
        }

        #endregion

        #region Pop

        /// <inheritdoc />
        public async Task PopAsync()
        {
            var currentPage = FormsNavigation.NavigationStack.LastOrDefault();
            var pageWeGoBackTo = FormsNavigation.NavigationStack.Reverse().Skip(1).FirstOrDefault();
            if (pageWeGoBackTo != null)
            {
                if (pageWeGoBackTo.BindingContext is INavigationAware pageWeGoBackNav)
                {
                    pageWeGoBackNav.OnNavigatingTo();
                }

                if (pageWeGoBackTo is TabbedPage tbPage)
                {
                    if(tbPage.CurrentPage.InternalChildren[0].BindingContext is INavigationAware tabb)
                        tabb.OnNavigatingTo();
                }
            }

            if (currentPage.BindingContext is INavigationAware nav)
            {
                nav.OnNavigatingFrom();
            }

            await FormsNavigation.PopAsync(true);

            if (pageWeGoBackTo != null)
            {
                if (pageWeGoBackTo.BindingContext is INavigationAware pageWeGoBackNav)
                {
                    pageWeGoBackNav.OnNavigatedTo();
                }
                if (pageWeGoBackTo is TabbedPage tbPage)
                {
                    if(tbPage.CurrentPage.InternalChildren[0].BindingContext is INavigationAware tabb)
                        tabb.OnNavigatedTo();
                }
                
            }
        }

        /// <inheritdoc />
        public async Task PopModalAsync()
        {
            await FormsNavigation.PopModalAsync(true);
        }

        /// <inheritdoc />
        public async Task PopToRootAsync(bool animate)
        {
            await FormsNavigation.PopToRootAsync(animate);
        }

        /// <inheritdoc />
        public async Task PopPopupAsync()
        {
            if (PopupNavigation.Instance.PopupStack.Count>=1)
                await PopupNavigation.Instance.PopAsync();
        }

        #endregion

        #region Push

        /// <inheritdoc />
        public async Task PushAsync(BaseNavigationViewModel viewModel, bool discardCurrent = false)
        {
            var currentPage = FormsNavigation.NavigationStack.LastOrDefault();
            var view = InstantiateView(viewModel);

            if (currentPage.BindingContext is INavigationAware navPrevious)
            {
                navPrevious.OnNavigatingFrom();
            }

            if(viewModel is INavigationAware navForward)
            {
                navForward.OnNavigatingTo();
            }

            await FormsNavigation.PushAsync((Page) view);

            if (viewModel is INavigationAware navigated)
            {
                navigated.OnNavigatedTo();
            }

            if (discardCurrent)
            {
                FormsNavigation.RemovePage(currentPage);
            }
        }

        /// <inheritdoc />
        public async Task PushModalAsync(BaseNavigationViewModel viewModel)
        {
            viewModel.IsModal = true;

            var currentPage = FormsNavigation.NavigationStack.LastOrDefault();
            if (currentPage.BindingContext is INavigationAware navPrevious)
            {
                navPrevious.OnNavigatingFrom();
            }

            if(viewModel is INavigationAware navForward)
            {
                navForward.OnNavigatingTo();
            }

            var view = InstantiateView(viewModel);
            var nv = new NavigationPage((Page) view);

            await FormsNavigation.PushModalAsync(nv);
        }

        /// <inheritdoc />
        public async Task<T> PushPopupAsync<T>(T viewModel) where T : BaseNavigationViewModel, IPopupViewModel
        {
            var view = InstantiateView(viewModel);
            var nv = (Page) view;

            var popupPage = (BasePopupPage<T>) nv;
            await PopupNavigation.Instance.PushAsync(popupPage);
            return await popupPage.Task;
        }

        /// <inheritdoc />
        public async Task PushAsync<T>(Action<T> initialize = null, bool discardCurrent = false)
            where T : BaseNavigationViewModel
        {
            T viewModel;

            var currentPage = FormsNavigation.NavigationStack.LastOrDefault();
            // Instantiate the view model & invoke the initialize method, if any
            viewModel = App.Container.Resolve<T>();
            initialize?.Invoke(viewModel);

            await PushAsync(viewModel);

            if (discardCurrent && currentPage != null)
            {
                FormsNavigation.RemovePage(currentPage);
            }
        }

        /// <inheritdoc />
        public async Task PushModalAsync<T>(Action<T> initialize = null) where T : BaseNavigationViewModel
        {
            // Instantiate the view model & invoke the initialize method, if any
            var viewModel = App.Container.Resolve<T>();
            viewModel.IsModal = true;
            initialize?.Invoke(viewModel);

            await PushModalAsync(viewModel);
        }

        #endregion

        IViewFor InstantiateView(BaseNavigationViewModel viewModel)
        {
            // Figure out what type the view model is
            var viewModelType = viewModel.GetType();

            // look up what type of view it corresponds to
            var viewType = _viewModelViewDictionary[viewModelType];

            // instantiate it
            var view = (IViewFor) App.Container.Resolve(viewType);

            view.ViewModel = viewModel;

            return view;
        }

        public void SwitchToChildPage(int pageIndex)
        {
            if (!(Application.Current.MainPage is MultiPage<ContentPage> mp))
                throw new ArgumentException("MainPage is not a Multi Page<ContentPage>");

            mp.CurrentPage = mp.Children[pageIndex];
        }

        /// <inheritdoc />
        public async Task PopToRootAsync(BaseNavigationViewModel vm,bool animate)
        {
            var navPage=InstantiateView(vm) as Page;
            
            var root = FormsNavigation.NavigationStack.First();
            FormsNavigation.InsertPageBefore(navPage,root);
            await FormsNavigation.PopToRootAsync(animate);
        }
    }
}
