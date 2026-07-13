using System;
using System.Numerics;
using PersonalProtfolioDataTier;


namespace PersonalProtfolioBusniessTier
{
    public class Skills
    {
        public enum _enMode { AddNew = 0, Update = 1 }
        public _enMode _Mode;
        public string LastErrorMessage { get; private set; } = string.Empty;

        public SkillsDataDTO SDTO
        {
            get
            {
                return new SkillsDataDTO(this.SkillID, this.SkillName, this.ImagePath, this.SkillCategory, this.IsActive, this.SkillDescreption);
            }
        }
        public int SkillID { get; set; }
        public string SkillName { get; set; } = null!;
        public string ImagePath { get; set; } = null!;
        public int SkillCategory { get; set; } = 0;
        public bool IsActive { get; set; } = true!;
        public string SkillDescreption { get; set; } = null!;


        public Skills(SkillsDataDTO SDTO, _enMode Mode = _enMode.AddNew)
        {
            this.SkillID = SDTO.SkillID;
            this.SkillName = SDTO.SkillName;
            this.ImagePath = SDTO.ImagePath;
            this.SkillCategory = SDTO.SkillCategory;
            this.IsActive = SDTO.IsActive;
            this._Mode = Mode;
            this.LastErrorMessage = string.Empty;
            this.SkillDescreption = SDTO.SkillDescreption;

        }


        public async Task<bool> AddNewSkill()
        {
            if (!_Validate())
                return false;

            this.SkillID = await SkillsData.AddNewSkill(SDTO);
            return (this.SkillID != -1);

        }

        public static async Task<Skills?> GetSkillById(int SkillID)
        {
            SkillsDataDTO? SDTO = await SkillsData.GetSkillById(SkillID);

            if (SDTO != null)
                return new Skills(SDTO, _enMode.Update);

            return null;
        }

        public async Task<bool> UpdateSkill()
        {
            if (!_Validate())
                return false;
            bool result = await SkillsData.UpdateSkill(this.SDTO);
            return result;
        }

        public static async Task<bool> DeleteSkill(int SkillID)
        {
            return await SkillsData.DeleteSkill(SkillID);
        }
        public static async Task<List<SkillsDataDTO>> GetAllSkills()
        {
            return await SkillsData.GetAllSkills();
        }
    
        private bool _Validate()
        {
            this.LastErrorMessage = string.Empty;

            if (this.SkillCategory > 4)
            {
                this.LastErrorMessage = "يجب أن لا يكون فئة المهاره تتجاوز 3";
            }
            if (string.IsNullOrWhiteSpace(this.SkillName))
            {
                this.LastErrorMessage = "اسم المهارة غير صالح.";
                return false;
            }
            else
            {
                this.SkillName = this.SkillName.ToUpper();
            }

            if (this.SkillCategory < 0)
            {
                this.LastErrorMessage = "يرجى اختيار تصنيف صالح للمهارة.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.ImagePath))
            {
                this.LastErrorMessage = "مسار أيقونة المهارة مطلوب.";
                return false;
            }

            if (string.IsNullOrEmpty(this.SkillDescreption))
            {
                this.LastErrorMessage = "وصف المهارة مطلوب.";
                return false;
            }
            //string extension = System.IO.Path.GetExtension(this.ImagePath).ToLower();
            //string[] allowedExtensions = { ".png", ".jpg", ".svg", ".webp" };

            //if (!allowedExtensions.Contains(extension))
            //{
            //    this.LastErrorMessage = "امتداد الصورة غير مدعوم، يرجى استخدام (PNG, SVG, JPG).";
            //    return false;
            //}
            LastErrorMessage = string.Empty;

            return true;
        }
    }
}
