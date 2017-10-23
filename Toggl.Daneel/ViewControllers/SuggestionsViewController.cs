using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Daneel.ViewControllers
{
    [NestedPresentation]
    public partial class SuggestionsViewController : MvxViewController<SuggestionsViewModel>
    {
        private SuggestionsTableViewSource tableViewSource;

        public SuggestionsViewController() 
            : base(nameof(SuggestionsViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            WelcomeTextLabel.Text = Resources.SuggestionsEmptyStateText;
            WelcomeTitleLabel.Text = Resources.SuggestionsEmptyStateTitle;

            //TableView setup
            tableViewSource = new SuggestionsTableViewSource(SuggestionsTableView);
            SuggestionsTableView.Source = tableViewSource;

            var bindingSet = this.CreateBindingSet<SuggestionsViewController, SuggestionsViewModel>();

            //Suggestions table view
            bindingSet.Bind(tableViewSource).To(vm => vm.Suggestions);
            bindingSet.Bind(WelcomeView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsEmpty)
                      .WithConversion(new MvxVisibilityValueConverter());

            bindingSet.Bind(tableViewSource)
                      .For(v => v.StartTimeEntryCommand)
                      .To(vm => vm.StartTimeEntryCommand);

            bindingSet.Apply();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            tableViewSource.ReloadTableData();
        }
    }
}
