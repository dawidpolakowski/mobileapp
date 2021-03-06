﻿using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public sealed partial class SelectProjectViewController : MvxViewController<SelectProjectViewModel>
    {
        public SelectProjectViewController() : base(nameof(SelectProjectViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            var source = new SelectProjectTableViewSource(ProjectsTableView);
            ProjectsTableView.Source = source;
            ProjectsTableView.TableFooterView = new UIView();
            
            var bindingSet = this.CreateBindingSet<SelectProjectViewController, SelectProjectViewModel>();

            //Table view
            bindingSet.Bind(source).To(vm => vm.Suggestions);
            bindingSet.Bind(source)
                      .For(v => v.ToggleTasksCommand)
                      .To(vm => vm.ToggleTaskSuggestionsCommand);
            
            //Text
            bindingSet.Bind(TextField).To(vm => vm.Text);
            
            //Commands
            bindingSet.Bind(CloseButton).To(vm => vm.CloseCommand);
            bindingSet.Bind(source)
                      .For(s => s.SelectionChangedCommand)
                      .To(vm => vm.SelectProjectCommand);
            
            bindingSet.Apply();
        }
    }
}

