using System.ComponentModel.DataAnnotations;

namespace NotesProject.ViewModels
{
    public class CreateNoteViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
