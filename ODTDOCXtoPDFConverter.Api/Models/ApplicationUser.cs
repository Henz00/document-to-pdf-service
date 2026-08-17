using Microsoft.AspNetCore.Identity;

namespace ODTDOCXtoPDFConverter.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; private set; } = string.Empty;
    }
}
