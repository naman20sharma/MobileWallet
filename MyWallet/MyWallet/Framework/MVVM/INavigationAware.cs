namespace IdentifyMe.MVVM
{
    public interface INavigationAware
    {
        /// <summary>
        /// When navigating out of the screen
        /// </summary>
        void OnNavigatingFrom();
        /// <summary>
        /// Before loading the screen
        /// </summary>
        void OnNavigatingTo();
        /// <summary>
        /// When screen is loaded
        /// </summary>
        void OnNavigatedTo();
    }
}
