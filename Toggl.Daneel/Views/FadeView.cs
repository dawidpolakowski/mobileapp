using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(FadeView))]
    public class FadeView : UIView
    {
        public nfloat FadeWidth { get; set; } = 8;

        public bool FadeLeft { get; set; }

        public bool FadeRight { get; set; }

        public FadeView(IntPtr handle) : base(handle)
        {
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            addFading();
        }

        private void addFading()
        {
            var gradient = new CAGradientLayer();
            var relativeFadingStart
                = FadeWidth / Bounds.Width;

            var transparentColor = new UIColor(0, 0).CGColor;
            var opaqueColor = new UIColor(0, 1).CGColor;

            gradient.Frame = Bounds;

            gradient.StartPoint = new CGPoint(0, 0);
            gradient.EndPoint = new CGPoint(1, 0);

            gradient.Colors = new CGColor[]
            {
                FadeLeft ? transparentColor : opaqueColor,
                opaqueColor,
                opaqueColor,
                FadeRight ? transparentColor : opaqueColor
            };

            gradient.Locations = new NSNumber[]
            {
                0,
                new NSNumber(relativeFadingStart),
                new NSNumber(1 - relativeFadingStart),
                1
            };

            Layer.Mask = gradient;
        }
    }
}
