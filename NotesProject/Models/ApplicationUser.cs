using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace NotesProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Notes = new List<Note>();
        }
        public List<Note> Notes { get; set; }
    }
}
