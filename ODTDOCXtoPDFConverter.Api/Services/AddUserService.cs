using Microsoft.AspNetCore.Identity;
using ODTDOCXtoPDFConverter.Api.Models;

namespace ODTDOCXtoPDFConverter.Api.Services
{
    public class AddUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AddUserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task AddUserAsync(string username, string password)
        {
            if (await _userManager.FindByNameAsync(username) is not null)
            {
                Console.WriteLine($"User '{username}' already exists.");
                return;
            }

            var user = new ApplicationUser
            {
                UserName = username
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    Console.WriteLine($"{error.Code}: {error.Description}");

                return;
            }

            Console.WriteLine($"User '{username}' created successfully.");
        }
    }
}
