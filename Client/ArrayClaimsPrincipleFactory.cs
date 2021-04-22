using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

// taken from the following location
// https://github.com/cradle77/BlazorSecurityDemo

// all the code is these projects come from a number of different sources, my goals were 
// must be the latest framework code, so everything is .Net 5, I needed roles and polcies to work
// with web assemblies, I wanted to use SQLite as the data store, I needed users to be able to have
// multiple roles, policies attached to them.

namespace DemoRoles.Client {
    public class ArrayClaimsPrincipalFactory<TAccount> : AccountClaimsPrincipalFactory<TAccount> where TAccount : RemoteUserAccount {
        public ArrayClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
        : base(accessor) { }

        // when a user belongs to multiple roles, IS4 returns a single claim with a serialised array of values
        // this class improves the original factory by deserializing the claims in the correct way
        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(TAccount account, RemoteAuthenticationUserOptions options) {
            var user = await base.CreateUserAsync(account, options);

            var claimsIdentity = (ClaimsIdentity)user.Identity;

            if (account != null) {
                foreach (var kvp in account.AdditionalProperties) {
                    var name = kvp.Key;
                    var value = kvp.Value;
                    if (value != null &&
                        (value is JsonElement element && element.ValueKind == JsonValueKind.Array)) {
                        claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(kvp.Key));

                        var claims = element.EnumerateArray()
                            .Select(x => new Claim(kvp.Key, x.ToString()));

                        claimsIdentity.AddClaims(claims);
                    }
                }
            }
            return user;
        }
    }
}