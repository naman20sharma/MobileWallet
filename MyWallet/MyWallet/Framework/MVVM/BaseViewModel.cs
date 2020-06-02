using System;
using System.Threading.Tasks;

namespace IdentifyMe.MVVM
{
    public abstract class BaseViewModel : BaseNotifyPropertyChanged, IDisposable
    {
        bool _isBusy;

        /// <summary>
        /// Gets or sets whether the ViewModel is busy. 
        /// </summary>
        /// <value><c>true</c> if is busy; otherwise, <c>false</c>.</value>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (RaiseAndUpdate(ref _isBusy, value))
                    Raise(nameof(IsNotBusy));
            }
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set => RaiseAndUpdate(ref title, value);
        }

        /// <summary>
        /// Gets a value indicating whether the ViewModel is not busy.
        /// </summary>
        /// <value><c>true</c> if is not busy; otherwise, <c>false</c>.</value>
        public bool IsNotBusy => !IsBusy;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <returns>Task with result.</returns>
        public virtual Task InitAsync() => Task.FromResult(true);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}
