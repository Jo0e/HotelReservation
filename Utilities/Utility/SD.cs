using Microsoft.AspNetCore.Http;

namespace Utilities.Utility
{
    public class SD
    {
        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";
        public const string CompanyRole = "Company";

        public static string GetLogoPath(HttpContext httpContext)
        {
            var logoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Logo");
            var files = Directory.GetFiles(logoDirectory);

            if (files.Length > 0)
            {
                var fileName = Path.GetFileName(files[0]);
                return $"/images/Logo/{fileName}";
            }

            return "/images/default-logo.png";
        }

    }
}
