using Microsoft.Data.SqlClient;
using PersonelProtfolio;
using PersonelProtfolio.EventLogs;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;


namespace PersonalProtfolioDataTier
{

    public class ProjectDataDTO
    {
        public ProjectDataDTO(int ProjectID, string Title, string Description, string ImagePath, string ProjectURL, DateTime AddedDateTime, bool IsActive)
        {
            this.ProjectID = ProjectID;
            this.Title = Title;
            this.Description = Description;
            this.ImagePath = ImagePath;
            this.ProjectURL = ProjectURL;
            this.AddedDateTime = AddedDateTime;
            this.IsActive = IsActive;
        }

        public int ProjectID { get; set; }

        [DefaultValue("")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [DefaultValue("")]
        [Required]
        public string Description { get; set; } = string.Empty;

        [DefaultValue("")]
        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [DefaultValue("")]
        [Required]
        public string ProjectURL { get; set; } = string.Empty;

        public DateTime AddedDateTime { get; set; }

        public bool IsActive { get; set; }
    }

    public class ProjectData
    {
         
        private static string?_connectionString = clsConnectionString.connectionString;


        
       

        public static async Task<int> AddNewProject(ProjectDataDTO PDTO)
        {
            int newProjectId = -1;

            string query = @"INSERT INTO Projects (Title, Description, ImagePath, ProjectURL, AddedDateTime, IsActive)
                    VALUES (@Title, @Description, @ImagePath, @ProjectURL, @AddedDateTime, @IsActive);
                    SELECT SCOPE_IDENTITY();";


            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new (query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@Title", PDTO.Title);
                    cmd.Parameters.AddWithValue("@Description", PDTO.Description);
                    cmd.Parameters.AddWithValue("@ImagePath", PDTO.ImagePath);
                    cmd.Parameters.AddWithValue("@ProjectURL", PDTO.ProjectURL);
                    cmd.Parameters.AddWithValue("@AddedDateTime", PDTO.AddedDateTime);
                    cmd.Parameters.AddWithValue("@IsActive", PDTO.IsActive);

                    await connection.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        newProjectId = insertedID;

                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Add New Project");
                    return -1;
                }
            }
            return newProjectId;
        }

        public static async Task<ProjectDataDTO?> GetProjectById(int Project)
        {
            string query = @"SELECT ProjectID, Title, Description, ImagePath, ProjectURL, AddedDateTime, IsActive
                            FROM Projects WHERE ProjectID = @ProjectID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@ProjectID", Project);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ProjectDataDTO(
                               reader.GetInt32(reader.GetOrdinal("ProjectID")),
                               reader.GetString(reader.GetOrdinal("Title")),
                               reader.GetString(reader.GetOrdinal("Description")),
                               reader.GetString(reader.GetOrdinal("ImagePath")),
                               reader.GetString(reader.GetOrdinal("ProjectURL")),
                               reader.GetDateTime(reader.GetOrdinal("AddedDateTime")),
                               reader.GetBoolean(reader.GetOrdinal("IsActive"))
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Get Project By ID");
                    return null;
                }
            }
            return null;
        }

        public static async Task<bool> UpdateProject(ProjectDataDTO PDTO)
        {
            string query = @"UPDATE Projects SET Title = @Title, Description = @Description, ImagePath = @ImagePath, 
                            ProjectURL = @ProjectURL, AddedDateTime = @AddedDateTime, IsActive = @IsActive
                            WHERE ProjectID = @ProjectID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@ProjectID", PDTO.ProjectID);
                    cmd.Parameters.AddWithValue("@Title", PDTO.Title);
                    cmd.Parameters.AddWithValue("@Description", PDTO.Description);
                    cmd.Parameters.AddWithValue("@ImagePath", PDTO.ImagePath);
                    cmd.Parameters.AddWithValue("@ProjectURL", PDTO.ProjectURL);
                    cmd.Parameters.AddWithValue("@AddedDateTime", PDTO.AddedDateTime);
                    cmd.Parameters.AddWithValue("@IsActive", PDTO.IsActive);

                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                        return true;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Update Project");
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> DeleteProject(int ProjectID)
        {
            string query = @"DELETE FROM Projects WHERE ProjectID = @ProjectID";
            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Delete Project");
                    return false;
                }
            }
        }
        public static async Task<List<ProjectDataDTO?>> GetAllProjects()
        {
            List<ProjectDataDTO?> projects = new();

            string query = @"SELECT ProjectID, Title, Description, ImagePath, ProjectURL, AddedDateTime, IsActive FROM Projects";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            projects.Add(new ProjectDataDTO(
                                reader.GetInt32(reader.GetOrdinal("ProjectID")),
                                reader.GetString(reader.GetOrdinal("Title")),
                                reader.GetString(reader.GetOrdinal("Description")),
                                reader.GetString(reader.GetOrdinal("ImagePath")),
                                reader.GetString(reader.GetOrdinal("ProjectURL")),
                                reader.GetDateTime(reader.GetOrdinal("AddedDateTime")),
                                reader.GetBoolean(reader.GetOrdinal("IsActive"))
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Get All Projects");
                }
            }
            return projects;
        }
    }
}
