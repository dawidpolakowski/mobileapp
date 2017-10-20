using System;
namespace Toggl.Foundation.MvvmCross.Services
{
    public interface IDialogService
    {
        void ShowMessage(
            string title,
            string message,
            string dismissButtonTitle,
            Action dismissAction);

        void Confirm(
            string title,
            string message,
            string confirmButtonTitle,
            string dismissButtonTitle,
            Action confirmAction,
            Action dismissAction);
    }
}
