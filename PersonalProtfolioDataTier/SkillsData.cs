using Microsoft.Data.SqlClient;
using PersonelProtfolio;
using PersonelProtfolio.EventLogs;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;


namespace PersonalProtfolioDataTier
{

    public class SkillsDataDTO
    {

        public SkillsDataDTO(int skillID, string skillName, string imagePath, int skillCategory, bool isActive , string SkillDescreption )
        {
            this.SkillID = skillID;
            this.SkillName = skillName;
            this.ImagePath = imagePath;
            this.SkillCategory = skillCategory;
            this.IsActive = isActive;
            this.SkillDescreption = SkillDescreption;
        }

        public int SkillID { get; set; }
        
        [DefaultValue("")]
        [Required]
        public string SkillName { get; set; }
        
        [DefaultValue("")]
        [Required]
        public string ImagePath { get; set; }
        
        [DefaultValue(0)]
        [Required]
        public int SkillCategory { get; set; }
        public bool IsActive { get; set; }

        [DefaultValue("")]
        [Required]
        public string SkillDescreption { get; set; }
    }

    public class SkillsData
    {


        private static string? _connectionString = clsConnectionString.connectionString;

        //public static void Initialize()
        //{
        //    _connectionString = clsConnectionString.connectionString;
        //}
        public static async Task<int> AddNewSkill(SkillsDataDTO SDTO)
        {
            int newSkillId = -1;

            string query = @"INSERT INTO Skills (SkillName, ImagePath, SkillCategory, IsActive, SkillDescreption)
                    VALUES (@SkillName, @ImagePath, @SkillCategory, @IsActive, @SkillDescreption);
                    SELECT SCOPE_IDENTITY();";


            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@SkillName", SDTO.SkillName);
                    cmd.Parameters.AddWithValue("@ImagePath", SDTO.ImagePath);
                    cmd.Parameters.AddWithValue("@SkillCategory", SDTO.SkillCategory);
                    cmd.Parameters.AddWithValue("@IsActive", SDTO.IsActive);
                   
                    cmd.Parameters.AddWithValue("@SkillDescreption", SDTO.SkillDescreption);


                    await connection.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        newSkillId = insertedID;

                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Add New Skill");
                    return -1;
                }
            }
            return newSkillId;
        }

        public static async Task<SkillsDataDTO?> GetSkillById(int SkillID)
        {
            string query = @"SELECT SkillID, SkillName, ImagePath, SkillCategory, IsActive, SkillDescreption
                            FROM Skills WHERE SkillID = @SkillID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@SkillID", SkillID);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new SkillsDataDTO(

                                reader.GetInt32(reader.GetOrdinal("SkillID")),
                                reader.GetString(reader.GetOrdinal("SkillName")),
                                reader.GetString(reader.GetOrdinal("ImagePath")),
                                reader.GetInt32(reader.GetOrdinal("SkillCategory")),
                                reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                reader.GetString(reader.GetOrdinal("SkillDescreption"))
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Get Skill By ID");
                    return null;
                }
            }
            return null;
        }

        public static async Task<bool> UpdateSkill(SkillsDataDTO SDTO)
        {
            string query = @"UPDATE Skills SET SkillName = @SkillName, ImagePath = @ImagePath, SkillCategory = @SkillCategory, SkillDescreption = @SkillDescreption,
                            IsActive = @IsActive
                            WHERE SkillID = @SkillID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@SkillID", SDTO.SkillID);
                    cmd.Parameters.AddWithValue("@SkillName", SDTO.SkillName);
                    cmd.Parameters.AddWithValue("@ImagePath", SDTO.ImagePath);
                    cmd.Parameters.AddWithValue("@SkillCategory", SDTO.SkillCategory);
                    cmd.Parameters.AddWithValue("@IsActive", SDTO.IsActive);
                    cmd.Parameters.AddWithValue("@SkillDescreption", SDTO.SkillDescreption);

                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                        return true;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Update Skill");
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> DeleteSkill(int SkillID)
        {
            string query = @"DELETE FROM Skills WHERE SkillID = @SkillID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@SkillID", SkillID);
                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Delete Skills");
                    return false;
                }
            }
        }

        public static async Task<List<SkillsDataDTO>> GetAllSkills()
        {
            List<SkillsDataDTO> skillsList = new List<SkillsDataDTO>();
            string query = @"SELECT SkillID, SkillName, ImagePath, SkillCategory, IsActive, SkillDescreption FROM Skills";
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
                            skillsList.Add(new SkillsDataDTO(
                                reader.GetInt32(reader.GetOrdinal("SkillID")),
                                reader.GetString(reader.GetOrdinal("SkillName")),
                                reader.GetString(reader.GetOrdinal("ImagePath")),
                                reader.GetInt32(reader.GetOrdinal("SkillCategory")),
                                reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                reader.GetString(reader.GetOrdinal("SkillDescreption"))
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Data Tier Get All Skills");
                    return skillsList;
                }
                return skillsList;
            }
        }

    }        
}
