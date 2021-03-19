using Remora.Commands.Conditions;
using Remora.Discord.Core;
using System.Linq;

namespace RoboSphere.Discord.Conditions
{
    public class SecureRoleConditionAttribute : ConditionAttribute
    {
        public SecureRoleConditionAttribute(params ulong[] roleIds) => RoleIds = roleIds.Select(x => new Snowflake(x)).ToArray();

        public Snowflake[] RoleIds { get; }
    }
}
