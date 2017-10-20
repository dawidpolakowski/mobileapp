using System;
using Toggl.Foundation.MvvmCross.Services;
using UIKit;

namespace Toggl.Daneel.Services
{
    public sealed class DialogService : IDialogService
    {
        public void Confirm(
            string title,
            string message,
            string confirmButtonTitle,
            string dismissButtonTitle,
            Action confirmAction,
            Action dismissAction)
        {
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(confirmButtonTitle, UIAlertActionStyle.Default, _ => confirmAction?.Invoke()));
            alert.AddAction(UIAlertAction.Create(dismissButtonTitle, UIAlertActionStyle.Cancel, _ => dismissAction?.Invoke()));
            getPresentationController().PresentViewController(alert, true, null);
        }

        public void ShowMessage(string title, string message, string dismissButtonTitle, Action dismissAction)
        {
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(dismissButtonTitle, UIAlertActionStyle.Cancel, _ => dismissAction?.Invoke()));
            getPresentationController().PresentViewController(alert, true, null);
        }

        private UIViewController getPresentationController()
        {
            var current = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (current.PresentedViewController != null)
                current = current.PresentedViewController;
            return current;
        }
    }
}
