using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI4.Areas.Mariachi.Models
{
    public class SongDetailsModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Song Title")]
        public string SongTitle
        {
            get;
            set;
        }

        [Display(Name = "Author")]
        public string Author
        {
            get;
            set;
        }

        [Required]
        [Display(Name = "Lyrics")]
        public string Lyrics
        {
            get;
            set;
        }
    }
}