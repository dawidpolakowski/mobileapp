using MvvmCross.Core.ViewModels;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public abstract class BaseViewModelTests<TViewModel> : BaseMvvmCrossTests
        where TViewModel : MvxViewModel
    {
        protected ITogglApi Api { get; } = Substitute.For<ITogglApi>();
        protected IDialogService DialogService { get; } = Substitute.For<IDialogService>();
        protected ITimeService TimeService { get; } = Substitute.For<ITimeService>();
        protected ITogglDatabase Database { get; } = Substitute.For<ITogglDatabase>();
        protected ITogglDataSource DataSource { get; } = Substitute.For<ITogglDataSource>();

        protected TViewModel ViewModel { get; private set; }

        protected BaseViewModelTests()
        {
            Setup();
        }

        protected abstract TViewModel CreateViewModel();

        private void Setup()
        {
            AdditionalSetup();

            ViewModel = CreateViewModel();
        }

        protected virtual void AdditionalSetup()
        {
        }
    }
}