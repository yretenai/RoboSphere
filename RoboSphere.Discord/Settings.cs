namespace RoboSphere.Discord
{
    public record Settings(long ClientId, string Sqlite)
    {
        public static Settings Default { get; } = new(0, "Data Source=Spherical.db");
    }
}
