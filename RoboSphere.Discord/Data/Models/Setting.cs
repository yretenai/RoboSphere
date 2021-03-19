using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RoboSphere.Discord.Data.Models
{
    [PublicAPI]
    [Index("Key")]
    public class Setting
    {
        [Key]
        public int SettingId { get; set; }

        [MaxLength(128)]
        public string Key { get; set; } = "";

        [Required]
        public string Value { get; set; } = "";

        public override string ToString() => $"Setting {{ Key = \"{Key}\", Value = \"{Value}\" }}";
    }
}
