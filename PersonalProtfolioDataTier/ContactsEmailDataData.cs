using PersonelProtfolio;
using Microsoft.Data.SqlClient;
using PersonelProtfolio.EventLogs;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PersonalProtfolioDataTier
{

    public class ContactsEmailDataDTO
    {

        public ContactsEmailDataDTO(int MessageID, string SenderName, string SenderEmail, string MessageContent, DateTime SentDateTime, bool IsRead)
        {
            this.MessageID = MessageID;
            this.SenderName = SenderName;
            this.SenderEmail = SenderEmail;
            this.SentDateTime = SentDateTime;
            this.IsRead = IsRead;
            this.MessageContent = MessageContent;
        }

        public int MessageID { get; private set; }

        [DefaultValue("")]
        [Required]
        public string SenderEmail { get; set; } = null!;

        [DefaultValue("")]
        [Required]
        public string SenderName { get; set; } = null!;

        [DefaultValue("")]
        [Required]
        public string MessageContent { get; set; } = null!;

        [Required]
        public DateTime SentDateTime { get;  set; } = DateTime.Now;

        [DefaultValue(false)]
        public bool IsRead { get; set; }
    }
    public class ContactsEmailDataData
    {

        static string? _connectionString = clsConnectionString.connectionString;

        public static async Task<bool> SendEmail(ContactsEmailDataDTO EDTO)
        {
            string query = @"INSERT INTO ContactMessages (SenderName, SenderEmail, MessageContent, SentDateTime, IsRead)
                VALUES (@SenderName, @SenderEmail, @MessageContent, @SentDateTime, @IsRead)";

            using (SqlConnection connection = new (_connectionString))
            using (SqlCommand cmd = new (query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@SenderName", EDTO.SenderName);
                    cmd.Parameters.AddWithValue("@SenderEmail", EDTO.SenderEmail);
                    cmd.Parameters.AddWithValue("@MessageContent", EDTO.MessageContent);
                    cmd.Parameters.AddWithValue("@SentDateTime", EDTO.SentDateTime);
                    cmd.Parameters.AddWithValue("@IsRead", EDTO.IsRead);

                    await connection.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return rowsAffected > 0;

                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in SendEmail!");
                    return false;
                }
            }
        }


        public static async Task<List<ContactsEmailDataDTO>> GetAllMessages()
        {
            string query = "SELECT MessageID, SenderName, SenderEmail, MessageContent, SentDateTime, IsRead FROM ContactMessages ORDER BY SentDateTime DESC";

            List<ContactsEmailDataDTO> messages = new ();

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
                            messages.Add(new ContactsEmailDataDTO(
                                reader.GetInt32(reader.GetOrdinal("MessageID")),
                                reader.GetString(reader.GetOrdinal("SenderName")),
                                reader.GetString(reader.GetOrdinal("SenderEmail")),
                                reader.GetString(reader.GetOrdinal("MessageContent")),
                                reader.GetDateTime(reader.GetOrdinal("SentDateTime")),
                                reader.GetBoolean(reader.GetOrdinal("IsRead"))
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in GetAllMessages!");
                }
            }
            return messages;
        }

        public static async Task<bool> MarkMessageAsRead(int messageId)
        {
            string query = "UPDATE ContactMessages SET IsRead = 1 WHERE MessageID = @MessageID";
            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@MessageID", messageId);

                    await connection.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in MarkMessageAsRead!");
                    return false;
                }
            }
        }

        public static async Task<bool> DeleteMessage(int messageId)
        {
            string query = "DELETE FROM ContactMessages WHERE MessageID = @MessageID";
            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@MessageID", messageId);

                    await connection.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in DeleteMessage!");
                    return false;
                }
            }
        }

        public static async Task<ContactsEmailDataDTO?> GetMessageById(int messageId)
        {
            string query = "SELECT MessageID, SenderName, SenderEmail, MessageContent, SentDateTime, IsRead FROM ContactMessages WHERE MessageID = @MessageID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@MessageID", messageId);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ContactsEmailDataDTO(
                                reader.GetInt32(reader.GetOrdinal("MessageID")),
                                reader.GetString(reader.GetOrdinal("SenderName")),
                                reader.GetString(reader.GetOrdinal("SenderEmail")),
                                reader.GetString(reader.GetOrdinal("MessageContent")),
                                reader.GetDateTime(reader.GetOrdinal("SentDateTime")),
                                reader.GetBoolean(reader.GetOrdinal("IsRead"))
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in GetMessageById!");
                }
            }
            return null;
        }

        public static async Task<IEnumerable<ContactsEmailDataDTO>> GetMessagesByOption(int Option)
        {
            string Query = "Setele * from ContactMessages where IsRead = @Option order by SentDateTime desc";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(Query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@Option", Option);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        List<ContactsEmailDataDTO> messages = new ();
                        while (await reader.ReadAsync())
                        {
                            messages.Add(new ContactsEmailDataDTO(
                                reader.GetInt32(reader.GetOrdinal("MessageID")),
                                reader.GetString(reader.GetOrdinal("SenderName")),
                                reader.GetString(reader.GetOrdinal("SenderEmail")),
                                reader.GetString(reader.GetOrdinal("MessageContent")),
                                reader.GetDateTime(reader.GetOrdinal("SentDateTime")),
                                reader.GetBoolean(reader.GetOrdinal("IsRead")   )
                            ));
                        }
                        return messages;
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in GetMessagesByOption!");
                    return Enumerable.Empty<ContactsEmailDataDTO>();
                }
            }
        }
    }
}

