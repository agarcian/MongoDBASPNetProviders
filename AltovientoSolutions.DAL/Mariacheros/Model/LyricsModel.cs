using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.DAL.Mariacheros.Model
{
    public class LyricsModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "SongTitle", ResourceType = typeof(Resources.Lyrics))]
        public string SongTitle { get; set; }
        [Display(Name = "Author", ResourceType = typeof(Resources.Lyrics))]
        public string Author { get; set; }
        [Required]
        [Display(Name = "SongLyrics", ResourceType = typeof(Resources.Lyrics))]
        public string Lyrics { get; set; }

    }
}
