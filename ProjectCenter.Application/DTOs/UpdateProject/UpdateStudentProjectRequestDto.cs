using Microsoft.AspNetCore.Http;

namespace ProjectCenter.Application.DTOs.UpdateProject
{
    public class UpdateStudentProjectRequestDto
    {
        public IFormFile? NewProjectFile { get; set; }      
        public IFormFile? NewDocumentationFile { get; set; }    
        public bool? IsPublic { get; set; }                
        public bool? RemoveProjectFile { get; set; }       
        public bool? RemoveDocumentationFile { get; set; }   
    }
}