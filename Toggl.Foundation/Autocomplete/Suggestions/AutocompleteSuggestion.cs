namespace Toggl.Foundation.Autocomplete.Suggestions
{
    public abstract class AutocompleteSuggestion
    {
        public string WorkspaceName { get; protected set; } = "";

        public long WorkspaceId { get; protected set; }

        public abstract override int GetHashCode();
    }
}
