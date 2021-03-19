namespace RoboSphere.Discord.Data.Models
{
    public record AnimatedEmote(string Name, ulong Id)
    {
        public override string ToString() => $"<a:{Name}:{Id}>";
        public static implicit operator string(AnimatedEmote e) => e.ToString();
    }
}
