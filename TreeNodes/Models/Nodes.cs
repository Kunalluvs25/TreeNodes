using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TreeNodes.Models
{
    /// <summary>
    /// Main model
    /// </summary>
    public class Nodes
    {
        /// <summary>
        /// Node ID
        /// </summary>
        public int NodeId { get; set; }

        /// <summary>
        /// Node name
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Parent node ID
        /// </summary>
        public int? ParentNodeId { get; set; }

        /// <summary>
        /// Is node active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Node issue date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Child node list
        /// </summary>
        public List<Nodes> Children { get; set; } = new List<Nodes>();


        /// <summary>
        /// Check is node new
        /// </summary>
        public bool isNewUser { get; set; }
    }
}