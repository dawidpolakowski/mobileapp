using System.Collections.Generic;
using System.Linq;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Autocomplete.Suggestions
{
    public sealed class TagSuggestion : AutocompleteSuggestion
    {
        public static IEnumerable<TagSuggestion> FromTags(IEnumerable<IDatabaseTag> tags)
            => tags.Select(t => new TagSuggestion(t));

        public long TagId { get; }

        public string Name { get; }

        public TagSuggestion(IDatabaseTag tag)
        {
            TagId = tag.Id;
            Name = tag.Name;
            WorkspaceId = tag.WorkspaceId;
            WorkspaceName = tag.Workspace.Name;
            WorkspaceId = tag.WorkspaceId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TagId.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }
    }
}
