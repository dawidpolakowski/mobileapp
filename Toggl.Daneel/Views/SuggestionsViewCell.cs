using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Color;
using MvvmCross.Plugins.Color.iOS;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Combiners;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.Suggestions;
using UIKit;

namespace Toggl.Daneel.Views
{
    public partial class SuggestionsViewCell : MvxTableViewCell
    {
        private const float NoProjectDistance = 24;
        private const float HasProjectDistance = 14;

        public static readonly NSString Key = new NSString(nameof(SuggestionsViewCell));
        public static readonly UINib Nib;

        public IMvxAsyncCommand<Suggestion> StartTimeEntryCommand { get; set; }

        static SuggestionsViewCell()
        {
            Nib = UINib.FromName(nameof(SuggestionsViewCell), NSBundle.MainBundle);
        }

        protected SuggestionsViewCell(IntPtr handle) 
            : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            ContentView.BackgroundColor = UIColor.White;
            FadeView.Layer.AddSublayer(new CAGradientLayer
            {
                Colors = new[] { UIColor.White.ColorWithAlpha(0.0f).CGColor, UIColor.White.CGColor },
                Locations = new[] { new NSNumber(0.0f), new NSNumber(0.2f) },
                StartPoint = new CGPoint(0.0, 0.5),
                EndPoint = new CGPoint(1.0, 0.5),
                Frame = FadeView.Bounds
            });

            StartButton.TouchUpInside += onStartButtonTap;

            this.DelayBind(() =>
            {
                var colorConverter = new MvxRGBValueConverter();
                var visibilityConverter = new MvxVisibilityValueConverter();
                var timeSpanConverter = new TimeSpanToDurationValueConverter();
                var descriptionTopDistanceValueConverter = new BoolToConstantValueConverter<nfloat>(HasProjectDistance, NoProjectDistance);
                var projectTaskClientCombiner = new ProjectTaskClientValueCombiner(
                    ProjectTaskClientLabel.Font.CapHeight,
                    Color.Suggestions.ClientColor.ToNativeColor(),
                    true
                );

                var bindingSet = this.CreateBindingSet<SuggestionsViewCell, Suggestion>();

                //Text
                bindingSet.Bind(DescriptionLabel).To(vm => vm.Description);
                bindingSet.Bind(ProjectTaskClientLabel)
                          .For(v => v.AttributedText)
                          .ByCombining(projectTaskClientCombiner,
                                       v => v.ProjectName,
                                       v => v.TaskName,
                                       v => v.ClientName,
                                       v => v.ProjectColor);

                //Visibility
                bindingSet.Bind(DescriptionTopDistanceConstraint)
                          .For(v => v.Constant)
                          .To(vm => vm.HasProject)
                          .WithConversion(descriptionTopDistanceValueConverter);

                bindingSet.Bind(ProjectTaskClientLabel)
                          .For(v => v.BindVisibility())
                          .To(vm => vm.HasProject)
                          .WithConversion(visibilityConverter);

                bindingSet.Apply();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            StartButton.TouchUpInside -= onStartButtonTap;
        }

        private async void onStartButtonTap(object sender, EventArgs e)
            => await StartTimeEntryCommand?.ExecuteAsync((Suggestion)DataContext);
    }
}
