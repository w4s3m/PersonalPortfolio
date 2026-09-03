using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalProtfolioBusniessTier;
using PersonalProtfolioDataTier;
using PersonelProtfolio.EventLogs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PersonelProtfolio.Controllers
{
    [Authorize]
    [Route("api/PersonelProtfolio")]
    [ApiController]
    public class PersonelProtfolio : ControllerBase
    {

        [Authorize(Roles = "admin")]
        [HttpPost("ADDnewProject",Name = "AddProject")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDataDTO>> AddNewProjectt([FromBody] ProjectDataDTO PDTO)
        {
            try
            {
                PersonalProtfolioBusniessTier.Project NewProject = new (new ProjectDataDTO(PDTO.ProjectID, PDTO.Title, PDTO.Description, PDTO.ImagePath,
                  PDTO.ProjectURL, PDTO.AddedDateTime, PDTO.IsActive));

                if (!await NewProject.AddNewProject() && !string.IsNullOrEmpty(NewProject.LastErrorMessage))
                     return BadRequest(NewProject.LastErrorMessage);

                if (NewProject.ProjectID != -1)
                {
                    PDTO.ProjectID = NewProject.ProjectID;
                    return CreatedAtRoute("GetProjectByID", new { ProjectID = PDTO.ProjectID }, PDTO);
                }
                else
                    return BadRequest("Failed to add new project.");
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Add New Project");
                return BadRequest("Exception error!");

            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("UpdateProjects/{ProjectID}", Name = "UpdateProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDataDTO>> UpdateProject( int ProjectID, [FromBody] ProjectDataDTO PDTO)
        {
            if (ProjectID <= 0)
                return BadRequest("Invalid Project ID.");
            try
            {
                Project? NewProjectData = await Project.GetProjectById(ProjectID);

                if (NewProjectData == null)
                    return NotFound($"Project with id {ProjectID} not found.");

                else
                {
                    NewProjectData.ProjectURL = PDTO.ProjectURL;
                    NewProjectData.ImagePath = PDTO.ImagePath;
                    NewProjectData.Title = PDTO.Title;
                    NewProjectData.Description = PDTO.Description;
                    NewProjectData.IsActive = PDTO.IsActive;

                    if (await NewProjectData.UpdateProject())
                        return Ok(NewProjectData.PDTO);
                    else
                        if (!string.IsNullOrEmpty(NewProjectData.LastErrorMessage))
                                 return BadRequest(NewProjectData.LastErrorMessage);
                        else
                             return BadRequest("Failed to update project.");

                }
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Update Project");
                return BadRequest("Exception error!");
            }
        }

        
        [Authorize(Roles = "admin")]
        [HttpDelete("deleteProject/{ProjectID}", Name = "DeleteProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteProjectByID(int ProjectID)
        {
            if (ProjectID <= 0)
                return BadRequest("Invalid Project ID.");

            try
            {
                Project? NewProjectData = await Project.GetProjectById(ProjectID);

                if (NewProjectData == null)
                    return NotFound($"Project with id {ProjectID} not found.");

                else
                {
                    if (await Project.DeleteProject(ProjectID))
                        return Ok(true);
                    else
                        return BadRequest("Failed to delete project.");
                }
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Delete Project");
                return BadRequest("Exception error!");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetAllProjects")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProjectDataDTO?>>> GetAllProjects()
        {
            try
            {
                IEnumerable<ProjectDataDTO?> Projects = await Project.GetAllProjects();

                // Best than Count == 0 because it doesn't need to count all items, just check if there's at least one item.
                if (!Projects.Any())
                    return NotFound("No projects found.");
                else
                    return Ok(Projects);
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get All Projects");
                return BadRequest("Exception error!");
            }
        }

        
        [Authorize(Roles = "admin")]
        [HttpGet("GetProjectByID/{ProjectID}", Name = "GetProjectByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDataDTO>> GetProjectByID(int ProjectID)
        {
            if (ProjectID <= 0)
                return BadRequest("Invalid Project ID.");
            try
            {
                Project? NewProjectData = await Project.GetProjectById(ProjectID);
                if (NewProjectData == null)
                    return NotFound($"Project with id {ProjectID} not found.");
                else
                    return Ok(NewProjectData.PDTO);
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get Project By ID");
                return BadRequest("Exception error!");
            }
        }


        [Authorize(Roles = "admin")]
        [HttpPost("AddSkill", Name = "AddSkill")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SkillsDataDTO>> AddNewSkill(SkillsDataDTO SDTO)
        {
            if (SDTO.SkillCategory < 0 || string.IsNullOrEmpty(SDTO.ImagePath)
                || string.IsNullOrEmpty(SDTO.SkillName) || !SDTO.IsActive || string.IsNullOrEmpty(SDTO.SkillDescreption))
                return BadRequest("Invalid Skill Data.");

            try
            {

                PersonalProtfolioBusniessTier.Skills NewSkill = new (new SkillsDataDTO(SDTO.SkillID, SDTO.SkillName, SDTO.ImagePath, SDTO.SkillCategory, SDTO.IsActive,SDTO.SkillDescreption));

                await NewSkill.AddNewSkill();
                SDTO.SkillID = NewSkill.SkillID;

                if (NewSkill.SkillID != -1)
                {
                    SDTO.SkillID = NewSkill.SkillID;
                    return CreatedAtRoute("GetSkillByid", new { SkillID = SDTO.SkillID }, SDTO);
                }
                else
                    return BadRequest("Failed to add new Skill.");
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Add New Skill");
                return BadRequest("Exception error!");

            }
        }

        //OLD DATA IS UPDATED
        [Authorize(Roles = "admin")]
        [HttpPut("UpdateSkills/{SkillID}", Name = "UpdateSkill")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SkillsDataDTO>> UpdateSkills(int SkillID, SkillsDataDTO SDTO)
        {
            if (SkillID <= 0)
                return BadRequest("Invalid Skill ID.");
            try
            {
                Skills? OldSkillData = await Skills.GetSkillById(SkillID);

                if (OldSkillData == null)
                    return NotFound($"Skill with id {SkillID} not found.");

                else
                {
                    SDTO.SkillID = SkillID;

                    OldSkillData.ImagePath = SDTO.ImagePath;
                    OldSkillData.IsActive = SDTO.IsActive;
                    OldSkillData.SkillCategory = SDTO.SkillCategory;
                    OldSkillData.SkillName = SDTO.SkillName;
                    OldSkillData.SkillDescreption = SDTO.SkillDescreption;

                    if (await OldSkillData.UpdateSkill())
                        return Ok(OldSkillData.SDTO);
                    else
                        if (!string.IsNullOrEmpty(OldSkillData.LastErrorMessage))
                             return BadRequest(OldSkillData.LastErrorMessage);
                        else
                             return BadRequest("Failed to update Skill.");
                }
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Update Skill");
                return BadRequest("Exception error!");

            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("deleteSkill/{SkillID}", Name = "DeleteSkills")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteSkillByID(int SkillID)
        {
            if (SkillID <= 0)
                return BadRequest("Invalid Skill ID.");

            try
            {
                Skills? NewSkillData = await Skills.GetSkillById(SkillID);

                if (NewSkillData == null)
                    return NotFound($"SkillID with id {SkillID} not found.");

                else
                {
                    if (await Skills.DeleteSkill(SkillID))
                        return Ok(true);
                    else
                        return BadRequest("Failed to delete Skill.");
                }
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Delete Skill");
                return BadRequest("Exception error!");
            }
        }
        [Authorize (Roles = "admin")]
        [HttpGet("GetSkillByid/{SkillID}", Name = "GetSkillByid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SkillsDataDTO>> GetSkillByid(int SkillID)
        {
            if (SkillID <= 0)
                return BadRequest("Invalid Skill ID.");
            try
            {
                Skills? NewSkillsData = await Skills.GetSkillById(SkillID);
                if (NewSkillsData == null)
                    return NotFound($"Skill with id {SkillID} not found.");
                else
                    return Ok(NewSkillsData.SDTO);
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get Skill By ID");
                return BadRequest("Exception error!");
            }
        }

       // [Authorize(Roles = "admin")]
        [HttpPost("addNewUsers", Name = "AddNewUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDataDTO>> AddNewUser(UserDataDTO UDTO)
        {
            try
            {

                PersonalProtfolioBusniessTier.Users NewUser = new (new UserDataDTO(UDTO.UserID, UDTO.UserName,
                    UDTO.PasswordHash, UDTO.FullName, UDTO.Email, UDTO.Role, UDTO.LastLogin, UDTO.IsActive));

               if(!await NewUser.AddNewUser() && !string.IsNullOrEmpty(NewUser.LastErrorMessage))
                    return BadRequest(NewUser.LastErrorMessage);

                if (NewUser.UserID != -1)
                {
                    UDTO.UserID = NewUser.UserID;
                    return CreatedAtRoute("GetUserByID", new { UserID = UDTO.UserID }, UDTO);
                }
                else
                    return BadRequest("Failed to add new User.");
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Add New User");
                return BadRequest("Exception error!");

            }
        }


       // [Authorize(Roles = "admin")]
        [HttpPut("UpdateUsers/{UserID}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDataDTO>> UpdateUser(int UserID, UserDataDTO UDTO)
        {
            if (UserID <= 0)
                return BadRequest("Invalid User ID.");
            try
            {
                Users? NewUserData = await Users.GetUserByID(UserID);

                if (NewUserData == null)
                    return NotFound($"User with id {UserID} not found.");

                else
                {
                    NewUserData.UserName = UDTO.UserName;
                    NewUserData.PasswordHash = UDTO.PasswordHash;
                    NewUserData.FullName = UDTO.FullName;
                    NewUserData.Email = UDTO.Email;
                    NewUserData.Role = UDTO.Role;
                    NewUserData.IsActive = UDTO.IsActive;

                    if (await NewUserData.UpdateUserData())
                        return Ok(NewUserData.UDTO);
                    else
                        if (!string.IsNullOrEmpty(NewUserData.LastErrorMessage))
                        return BadRequest(NewUserData.LastErrorMessage);
                    else
                        return BadRequest("Failed to update User.");

                }
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Update User");
                return BadRequest("Exception error!");
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("deleteUserByID/{UserID}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteUserByID(int UserID)
        {
            if (UserID <= 0)
                return BadRequest("Invalid User ID.");

            try
            {
                Users? NewUserData = await Users.GetUserByID(UserID);

                if (NewUserData == null)
                    return NotFound($"User with id {UserID} not found.");

                else
                {
                    if (await Users.DeleteUserByID(UserID))
                        return Ok(true);
                    else
                        return BadRequest("Failed to delete User.");
                }
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Delete User");
                return BadRequest("Exception error!");
            }
        }


        // This Owner of the data can access it, and Admin can access any user data.
        [HttpGet("GetUserByID/{UserID}", Name = "GetUserByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDataDTO>> GetUserByID(int UserID, [FromServices] IAuthorizationService authorizationService)
        {
            if (UserID <= 0)
                return BadRequest("Invalid UserID ID.");
            try
            {
                Users? NewUserData = await Users.GetUserByID(UserID);
                if (NewUserData == null)
                    return NotFound($"User with id {UserID} not found.");
                else
                {
                  
                   var authResult = await authorizationService.AuthorizeAsync(User, UserID, "UserOwnerOrAdmin");

                    if (!authResult.Succeeded)
                        return Forbid();
                    else
                        return Ok(NewUserData.UDTO);    
                }

            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get User By ID");
                return BadRequest("Exception error!");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetAllSkills")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SkillsDataDTO>>> GetAllSkills()
        {
            try
            {
                IEnumerable<SkillsDataDTO> skills = await Skills.GetAllSkills();

                if (!skills.Any())
                    return NotFound("No Skills found.");
                else  
                    return Ok(skills);
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get All Skills");
                return BadRequest("Exception error!");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetAdminContact")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetAdminContact()
        {
            try
            {
                var AdminEmail = await Users.GetAdminEmail();

                if (AdminEmail == null)
                    return NotFound("Admin Email Not Found!");

                return Ok(AdminEmail.ToString());
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get Admin Contact");
                return BadRequest("Exception error!");
            }
        }
        
        [AllowAnonymous]
        [HttpPost("SendEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> SendEmail(ContactsEmailDataDTO EDTO)
        {
            try
            {
                ContactsEmail NewEmail = new (new ContactsEmailDataDTO(EDTO.MessageID, EDTO.SenderName, 
                    EDTO.SenderEmail, EDTO.MessageContent, EDTO.SentDateTime, EDTO.IsRead));    

                bool isSent = await NewEmail.SendEmail(EDTO);
                
                if (!string.IsNullOrEmpty(NewEmail.LastErrorMessage))
                    return BadRequest(NewEmail.LastErrorMessage);
                else if (!isSent)
                    return BadRequest("Failed to send email.");
                else
                    return Ok(isSent);
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Send Email");
                return BadRequest("Exception error!");
            }
        }

        
        //[Authorize(Roles = "admin")]
        [HttpPut("MarkMessageAsRead")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> MarkMessageAsRead(int MessageID)
        {
            if (MessageID <= 0)
                return BadRequest("Invalid Message ID.");
            try
            {
                bool isMarked = await ContactsEmail.MarkMessageAsRead(MessageID);   
                if (isMarked)
                    return Ok(true);
                else
                    return BadRequest("Failed to mark message as read.");
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Mark Message As Read");
                return BadRequest("Exception error!");
            }
        }

      //  [Authorize(Roles = "admin")]
        [HttpDelete("DeleteMessage/{MessageID}", Name = "DeleteMessage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteMessage(int MessageID)
        {
            if (MessageID <= 0)
                return BadRequest("Invalid Message ID.");
            try
            {
                ContactsEmailDataDTO? Email = await ContactsEmail.GetMessageByID(MessageID);

                if (Email == null)
                    return NotFound($"Message with id {MessageID} not found.");

                bool isDeleted = await ContactsEmail.DeleteMessage(MessageID);
                if (isDeleted)
                    return Ok(true);
                else
                    return BadRequest("Failed to delete message.");
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Delete Message");
                return BadRequest("Exception error!");
            }
        }

      //  [Authorize(Roles = "admin")]
        [HttpGet("GetMessageByID/{MessageID}", Name = "GetMessageByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContactsEmailDataDTO>> GetMessageByID(int MessageID)
        {
            if (MessageID <= 0)
                return BadRequest("Invalid Message ID.");
            try
            {
                ContactsEmailDataDTO? MessageData = await ContactsEmail.GetMessageByID(MessageID);
                if (MessageData == null)
                    return NotFound($"Message with id {MessageID} not found.");
                else
                    return Ok(MessageData);
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get Message By ID");
                return BadRequest("Exception error!");
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("GetMessagesByOption", Name = "GetMessageOption")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ContactsEmailDataDTO>>> GetMessagesByOption(int Option)
        {
            if (Option < 0 || Option > 2)
                return BadRequest("Invalid Option. Option must be 0 for All Messages, 1 for Read Messages, or 2 for Unread Messages.");
            try
            {
                IEnumerable<ContactsEmailDataDTO> Messages = await ContactsEmail.GetMessagesByOption(Option);
                if (!Messages.Any())
                    return NotFound("No messages found for the specified option.");
                else
                    return Ok(Messages);
            }
            catch (Exception ex)
            {
                clsEventLog.EnterEventLog(ex, "Controller Get Messages By Option");
                return BadRequest("Exception error!");
            }
        }

        
    }
}