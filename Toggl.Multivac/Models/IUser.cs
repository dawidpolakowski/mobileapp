using System;

namespace Toggl.Multivac.Models
{
    public interface IUser : IBaseModel
    {
        string ApiToken { get; }

        long DefaultWorkspaceId { get; }

        string Email { get; }

        string Fullname { get; }

        string TimeOfDayFormat { get; }

        string DateFormat { get; }

        bool StoreStartAndStopTime { get; }

        BeginningOfWeek BeginningOfWeek { get; }

        string Language { get; }

        string ImageUrl { get; }

        DateTimeOffset At { get; }
    }
}
