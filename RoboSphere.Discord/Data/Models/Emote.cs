namespace RoboSphere.Discord.Data.Models
{
    public record Emote(string Name, ulong Id)
    {
        public override string ToString() => $"<:{Name}:{Id}>";
        public static implicit operator string(Emote e) => e.ToString();
    }
}
