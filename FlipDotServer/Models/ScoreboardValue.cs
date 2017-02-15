
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlipDotServer.Models
{
    public class ScoreboardValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Key { get; set; }
        public float Value { get; set; }
    }
}
