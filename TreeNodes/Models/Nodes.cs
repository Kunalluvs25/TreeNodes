using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TreeNodes.Models
{
    public class Nodes
    {
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public int? ParentNodeId { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public List<Nodes> Children { get; set; } = new List<Nodes>();

        public bool isNewUser { get; set; }
    }
}