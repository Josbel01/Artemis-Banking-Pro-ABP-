using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.User
{
    public class DeleteUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;
    }
}
