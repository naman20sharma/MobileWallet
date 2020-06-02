namespace IdentifyMe.MVVM.Abstractions
{
    public interface IViewFor
    {
        object ViewModel { get; set; }
    }

    public interface IViewFor<T> : IViewFor where T : BaseNavigationViewModel
    {
        new T ViewModel { get; set; }
    }
}
