﻿using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Color;
using MvvmCross.Plugins.Visibility;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Converters;
using UIKit;

namespace Toggl.Daneel.Views
{
    public partial class ProjectSuggestionViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ProjectSuggestionViewCell));
        public static readonly UINib Nib;

        public bool TopSeparatorHidden
        {
            get => TopSeparatorView.Hidden;
            set => TopSeparatorView.Hidden = value;
        }

        static ProjectSuggestionViewCell()
        {
            Nib = UINib.FromName(nameof(ProjectSuggestionViewCell), NSBundle.MainBundle);
        }

        protected ProjectSuggestionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand { get; set; }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            ClientNameLabel.LineBreakMode = UILineBreakMode.TailTruncation;
            ProjectNameLabel.LineBreakMode = UILineBreakMode.TailTruncation;
            ToggleTasksButton.TouchUpInside += togglTasksButton;

            this.DelayBind(() =>
            {
                var colorConverter = new MvxRGBValueConverter();
                var taskCountConverter = new TaskCountConverter();
                var visibilityConverter = new MvxVisibilityValueConverter();

                var bindingSet = this.CreateBindingSet<ProjectSuggestionViewCell, ProjectSuggestion>();

                //Text
                bindingSet.Bind(ProjectNameLabel).To(vm => vm.ProjectName);
                bindingSet.Bind(ClientNameLabel).To(vm => vm.ClientName);
                bindingSet.Bind(AmountOfTasksLabel)
                          .To(vm => vm.NumberOfTasks)
                          .WithConversion(taskCountConverter);

                //Color
                bindingSet.Bind(ProjectNameLabel)
                          .For(v => v.TextColor)
                          .To(vm => vm.ProjectColor)
                          .WithConversion(colorConverter);

                bindingSet.Bind(ProjectDotView)
                          .For(v => v.BackgroundColor)
                          .To(vm => vm.ProjectColor)
                          .WithConversion(colorConverter);

                //Visibility
                bindingSet.Bind(ToggleTaskImage)
                          .For(v => v.BindVisibility())
                          .To(vm => vm.NumberOfTasks)
                          .WithConversion(visibilityConverter);
                
                bindingSet.Bind(ToggleTasksButton)
                          .For(v => v.BindVisibility())
                          .To(vm => vm.NumberOfTasks)
                          .WithConversion(visibilityConverter);
                
                bindingSet.Apply();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            ToggleTasksButton.TouchUpInside -= togglTasksButton;
        }

        private void togglTasksButton(object sender, EventArgs e)
            => ToggleTasksCommand?.Execute((ProjectSuggestion)DataContext);
    }
}
