// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.Views
{
    [Register ("StartTimeEntryProjectsViewCell")]
    partial class ProjectSuggestionViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AmountOfTasksLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ClientNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView ProjectDotView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ProjectNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView ToggleTaskImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ToggleTasksButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView TopSeparatorView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AmountOfTasksLabel != null) {
                AmountOfTasksLabel.Dispose ();
                AmountOfTasksLabel = null;
            }

            if (ClientNameLabel != null) {
                ClientNameLabel.Dispose ();
                ClientNameLabel = null;
            }

            if (ProjectDotView != null) {
                ProjectDotView.Dispose ();
                ProjectDotView = null;
            }

            if (ProjectNameLabel != null) {
                ProjectNameLabel.Dispose ();
                ProjectNameLabel = null;
            }

            if (ToggleTaskImage != null) {
                ToggleTaskImage.Dispose ();
                ToggleTaskImage = null;
            }

            if (ToggleTasksButton != null) {
                ToggleTasksButton.Dispose ();
                ToggleTasksButton = null;
            }

            if (TopSeparatorView != null) {
                TopSeparatorView.Dispose ();
                TopSeparatorView = null;
            }
        }
    }
}