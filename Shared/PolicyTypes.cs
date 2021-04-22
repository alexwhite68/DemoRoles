using Microsoft.AspNetCore.Authorization;

namespace DemoRoles.Shared {
    public static class PolicyTypes {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireUser = "RequireUser";

        public static AuthorizationOptions AddAppPolicies(this AuthorizationOptions options) {
            options.AddPolicy(RequireAdmin, policy => policy.RequireRole(RoleTypes.Admin));
            options.AddPolicy(RequireUser, policy => policy.RequireRole(RoleTypes.User));
            return options;
        }
    }
}
