using System;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Abstractions;
using PersonalProtfolioDataTier;


namespace PersonalProtfolioBusniessTier
{
    public class Project
    {
        public enum _enMode { AddNew = 0, Update = 1 }
        public _enMode _Mode;
        public string LastErrorMessage { get; private set; } = string.Empty;
        public ProjectDataDTO PDTO
        {
            get
            {
                return new ProjectDataDTO(this.ProjectID, this.Title, this.Description,
                           this.ImagePath, this.ProjectURL, this.AddedDateTime, this.IsActive);
            }
        }

        public int ProjectID { get; set; } = -1;
        public string Title { get; set; } = string.Empty;
        public string ProjectURL { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public DateTime AddedDateTime { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public Project(ProjectDataDTO PDTO, _enMode Mode = _enMode.AddNew)
        {
            this.ProjectID = PDTO.ProjectID;
            this.Title = PDTO.Title;
            this.Description = PDTO.Description;
            this.ImagePath = PDTO.ImagePath;
            this.ProjectURL = PDTO.ProjectURL;
            this.AddedDateTime = PDTO.AddedDateTime;
            this.IsActive = PDTO.IsActive;
            this._Mode = Mode;
            this.LastErrorMessage = string.Empty;
        }

        public static async Task<Project?> GetProjectById(int ProjectID)
        {
            ProjectDataDTO? PDTO = await ProjectData.GetProjectById(ProjectID);

            if (PDTO == null)
                return null;

            return new Project(PDTO, _enMode.Update);
        }

        public  async Task<bool> UpdateProject()
        {
            if (!_Validate())
                return false;

            bool Result = await ProjectData.UpdateProject(this.PDTO);
            return Result;
        }   

        public async Task<bool> AddNewProject()
        {
            if (!_Validate())
            {
                return false;
            }

            this.ProjectID = await ProjectData.AddNewProject(this.PDTO);
                return (this.ProjectID != -1);
        }

        public static async Task<bool> DeleteProject(int ProjectID)
        {
            return await ProjectData.DeleteProject(ProjectID);
        }

        public static async Task<List<ProjectDataDTO>> GetAllProjects()
        {
            List<ProjectDataDTO> projectsData = await ProjectData.GetAllProjects();

            return projectsData;
        }

        private bool _Validate()
        {
           this.LastErrorMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.ProjectURL) && !this.ProjectURL.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                this.LastErrorMessage = "رابط المشروع يجب أن يبدأ بـ https لضمان الأمان.";
                return false;
            }

            //if (this.AddedDateTime > DateTime.)
            //{
            //    this.LastErrorMessage = "تاريخ الإضافة لا يمكن أن يكون في المستقبل.";
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(this.Description))
            {
                this.LastErrorMessage = "وصف المشروع قصير جداً او فارغ.";
                return false;
            }
          
            if (!string.IsNullOrWhiteSpace(this.Title))
            {
                this.LastErrorMessage = "عنوان المشروع قصير جداً او فارغ.";
                this.Title = char.ToUpper(this.Title[0]) + this.Title.Substring(1).ToLower();
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
