using System.Collections.Generic;

namespace microsoft_graph_extensions.Models
{
    public class GroupModel
    {
        public GroupModel()
        {
            Members = new List<Member>();
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ICollection<Member> Members { get; set; }
    }
}
