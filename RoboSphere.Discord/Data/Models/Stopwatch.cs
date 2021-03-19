using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace RoboSphere.Discord.Data.Models
{
    [PublicAPI]
    [Index("Key")]
    public class Stopwatch
    {
        [Key]
        public int StopwatchId { get; set; }

        [MaxLength(128)]
        public string Key { get; set; } = "";

        [Required]
        public long Next { get; set; }

        public override string ToString() => $"Stopwatch {{ Key = \"{Key}\", Next = \"{Next}\" ({DateTimeOffset.FromUnixTimeMilliseconds(Next):G}) }}";
    }
}
