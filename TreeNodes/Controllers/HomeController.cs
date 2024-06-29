using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using TreeNodes.Models;

namespace TreeNodes.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Main page (Tree and CURD ref)
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var nodes = GetAllNodes();
            return View(nodes);
        }

        /// <summary>
        /// Node info page
        /// </summary>
        /// <returns></returns>
        public ActionResult Tree()
        {
            var nodes = GetAllNodes();
            return View(nodes);

        }

        /// <summary>
        /// CURD page
        /// </summary>
        /// <returns></returns>
        public ActionResult CURD()
        {

            return View();

        }

        /// <summary>
        /// Connection string
        /// </summary>
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnectDB"].ConnectionString;

        /// <summary>
        /// Get all data from DB
        /// </summary>
        /// <returns>
        /// Data list
        /// </returns>
        public List<Nodes> GetTreeData1()
        {
            try
            {
                // Stored procedure
                string storedProcedure = "GetNodeInfo";
                List<Nodes> nodes = new List<Nodes>();

                // Fetch connection string
                using (var connection = new SqlConnection(connectionString))
                {
                    // establish connection and SP
                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();

                        // Fetch data
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Nodes node = new Nodes
                                {
                                    NodeId = Convert.ToInt32(reader["NodeID"]),
                                    NodeName = reader["NodeName"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["isActive"]),
                                    StartDate = Convert.ToDateTime(reader["StartDate"])
                                };

                                if (reader["ParentNodeId"] != DBNull.Value)
                                {
                                    node.ParentNodeId = Convert.ToInt32(reader["ParentNodeId"]);
                                }
                                else
                                {
                                    node.ParentNodeId = null;
                                }

                                nodes.Add(node);
                            }
                        }
                    }

                    return nodes;
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Sync parent nodes with child
        /// </summary>
        /// <returns>
        /// Synced list
        /// </returns>
        public List<Nodes> GetAllNodes()
        {
            try { 
            return BuildNodeTree(null);
            }
            catch { throw; }
        }

        /// <summary>
        /// Syncing method
        /// </summary>
        /// <param name="parentNodeId"></param>
        /// <returns>Synced list</returns>
        private List<Nodes> BuildNodeTree(int? parentNodeId)
        {
            try { 
            var nodes = GetTreeData1().Where(n => n.ParentNodeId == parentNodeId).ToList();

            foreach (var node in nodes)
            {
                node.Children = BuildNodeTree(node.NodeId);
            }

            return nodes;
            }
            catch { throw; }
        }

        /// <summary>
        /// Curd operation 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>
        /// Results
        /// </returns>
        public string CURDOperations(Nodes Data)
        {
            try
            {
                // Check data is null
                if (Data == null)
                {
                    return "101";// Null data
                }
                // Check is user new
                if (Data.isNewUser == false)
                {
                    using (var context = new kunsaa_Entities())
                    {

                        List<Nodes> InfoList = GetTreeData1();

                        // User exist checking
                        bool hasData = false;
                        foreach (var node in InfoList)
                        {
                            if (node.NodeId == Data.NodeId)
                            {
                                hasData = true;
                            }

                        }

                        if (!hasData)
                        {
                            return "103"; // No users
                        }

                        // Update user
                        if (UpdateNode(Data.NodeId, Data) == true)
                        {
                            return "1";
                        }
                        // is valid model
                        if (ModelState.IsValid)
                        {
                            Node insertValue = new Node();
                            insertValue.nodeName = Data.NodeName;
                            insertValue.ParentNodeId = Data.ParentNodeId;
                            insertValue.isActive = Data.IsActive;

                            context.Entry(insertValue).State = EntityState.Modified;

                            // Save changes
                            context.SaveChanges();
                        }

                    }

                }
                else
                {
                    // Insert node
                    Node insertValue = new Node();
                    insertValue.nodeName = Data.NodeName;
                    insertValue.ParentNodeId = Data.ParentNodeId;
                    insertValue.isActive = Data.IsActive;
                    if (insertData(Data) == true)
                    {
                        return "1";
                    }

                    // Use entity framework
                    using (var context = new kunsaa_Entities())
                    {

                        if (ModelState.IsValid)
                        {

                            context.Entry(insertValue).State = EntityState.Added;

                            // Save changes
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch { return "404"; }

            return "1";
        }

        /// <summary>
        /// Delete node
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// Results
        /// </returns>
        public int DeleteNode(Nodes data)
        {
            try
            {
                if (data == null)
                {
                    return 0;// Null data
                }

                List<Nodes> InfoList = GetTreeData1();

                bool hasData = false;
                foreach (var node in InfoList)
                {
                    if (node.NodeId == data.NodeId)
                    {
                        hasData = true;
                    }

                }
                if (!hasData)
                {
                    return 103; // No users
                }
                if (DeleteCurNode(data) == true)
                {
                    return 1;
                }
                using (var context = new kunsaa_Entities())
                {

                    if (ModelState.IsValid)
                    {

                        context.Entry(data).State = EntityState.Deleted;

                        // Save changes
                        context.SaveChanges();
                    }

                }

            }
            catch
            {
                return 404;
            }

            return 1;
        }

        /// <summary>
        /// Update node using query
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedModel"></param>
        /// <returns>
        /// Results
        /// </returns>
        public static bool UpdateNode(int id, Nodes updatedModel)
        {
            try { 
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectDB"].ConnectionString))
            {
                string query = "UPDATE Nodes SET isActive = @isActive, NodeName = @NodeName, ParentNodeId = @ParentNodeId WHERE NodeID = @NodeID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@isActive", SqlDbType.Bit) { Value = updatedModel.IsActive });
                    cmd.Parameters.Add(new SqlParameter("@NodeName", SqlDbType.NVarChar) { Value = updatedModel.NodeName });
                    cmd.Parameters.Add(new SqlParameter("@NodeID", SqlDbType.Int) { Value = id });
                    cmd.Parameters.Add(new SqlParameter("@ParentNodeId", SqlDbType.Int) { Value = updatedModel.ParentNodeId });

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();

                    return rowsAffected > 0;
                }
            }
            }
            catch { throw; }
        }

        /// <summary>
        /// Insert new node using query
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// results
        /// </returns>
        public bool insertData(Nodes model)
        {
            try { 
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectDB"].ConnectionString))
            {
                string query = "INSERT INTO Nodes (NodeName, IsActive, ParentNodeId) VALUES (@NodeName, @IsActive, @ParentNodeId)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {

                    cmd.Parameters.Add(new SqlParameter("@NodeName", SqlDbType.NVarChar) { Value = model.NodeName });
                    cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = model.IsActive });
                    cmd.Parameters.Add(new SqlParameter("@ParentNodeId", SqlDbType.Int) { Value = model.ParentNodeId ?? (object)DBNull.Value });


                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();

                    return rowsAffected > 0;
                }
            }
            }
            catch { throw; }

        }

        /// <summary>
        /// Delete current node
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns>
        /// Results
        /// </returns>
        public bool DeleteCurNode(Nodes nodes)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectDB"].ConnectionString))
                {
                    string query = "DELETE FROM Nodes WHERE NodeID = @NodeID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@NodeID", SqlDbType.Int) { Value = nodes.NodeId });

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        conn.Close();

                        return rowsAffected > 0;
                    }
                }
            }
            catch { throw; }
        }
    }



}
