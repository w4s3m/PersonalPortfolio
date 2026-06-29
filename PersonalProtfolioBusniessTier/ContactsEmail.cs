using System;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Abstractions;
using PersonalProtfolioDataTier;

namespace PersonalProtfolioBusniessTier
{
    public class ContactsEmail
    {
        public ContactsEmailDataDTO EDTO
        {
            get
            {
                return new ContactsEmailDataDTO(this.MessageID, this.SenderName, this.SenderEmail, this.MessageContent, this.SentDateTime, this.IsRead);
            }
        }

        public ContactsEmail(ContactsEmailDataDTO EDTO) 
        {
            this.MessageID = EDTO.MessageID;
            this.SenderName = EDTO.SenderName;
            this.SenderEmail = EDTO.SenderEmail;
            this.MessageContent = EDTO.MessageContent;
            this.SentDateTime = EDTO.SentDateTime;
            this.IsRead = EDTO.IsRead;
        }

        public int MessageID { get; private set; }
        public string SenderName { get; set; } = null!;
        public string SenderEmail { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public DateTime SentDateTime { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }

        public string LastErrorMessage { get; private set; } = string.Empty;

        private static bool IsValidEmail(string email)
        {
            try
            {
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(SenderName))
            {
                LastErrorMessage = "يرجى إدخال اسمك الكريم.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(SenderEmail))
            {
                LastErrorMessage = "البريد الإلكتروني مطلوب لنتمكن من الرد عليك.";
                return false;
            }
            if (!IsValidEmail(SenderEmail))
            {
                LastErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(MessageContent))
            {
                LastErrorMessage = "لا يمكنك إرسال رسالة فارغة، يرجى كتابة استفسارك.";
                return false;
            }
            LastErrorMessage = string.Empty;
            return true;
        }
        public async Task<bool> SendEmail(ContactsEmailDataDTO EDTO)
        {
            EDTO.SentDateTime = DateTime.Now;   
            EDTO.IsRead = false;
           
            if (!IsValid())
                return false;

            return await ContactsEmailDataData.SendEmail(EDTO); 
        }
        public static async Task <bool> MarkMessageAsRead(int MessageID)
        {   
            return await ContactsEmailDataData.MarkMessageAsRead(MessageID);
        }   

        public static async Task<bool> DeleteMessage(int MessageID)
        {
            return await ContactsEmailDataData.DeleteMessage(MessageID);
        }   

        public static async Task <ContactsEmailDataDTO?> GetMessageByID(int MessageID)
        {
            return await ContactsEmailDataData.GetMessageById(MessageID);   
        }
        public static async Task<IEnumerable<ContactsEmailDataDTO>> GetMessagesByOption(int Option)
        {
            return await ContactsEmailDataData.GetMessagesByOption(Option);
        }
    }

}
