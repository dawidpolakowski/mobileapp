using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS.Views;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Daneel.Views.Interfaces;
using UIKit;

namespace Toggl.Daneel.Views
{
    public sealed partial class WorkspaceHeaderViewCell : MvxTableViewHeaderFooterView, IHeaderViewCellWithHideableTopSeparator
    {
        public static readonly NSString Key = new NSString(nameof(WorkspaceHeaderViewCell));
        public static readonly UINib Nib;

        public bool TopSeparatorHidden
        {
            get => TopSeparator.Hidden;
            set => TopSeparator.Hidden = value;
        }

        static WorkspaceHeaderViewCell()
        {
            Nib = UINib.FromName(nameof(WorkspaceHeaderViewCell), NSBundle.MainBundle);
        }

        public WorkspaceHeaderViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<WorkspaceHeaderViewCell, WorkspaceGroupedCollection<object>>();

                bindingSet.Bind(WorkspaceLabel).To(vm => vm.WorkspaceName);

                bindingSet.Apply();
            });
        }
    }
}
