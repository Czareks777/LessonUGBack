using LessonUG.Models;
using Microsoft.AspNetCore.Identity;

namespace LessonUG.Data
{
    public class SeedTeacherAccount
    {
        public static async Task SeedTeacher(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            if (!await roleManager.RoleExistsAsync(Roles.Teacher))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Teacher));
            }


            var adminUsers = await userManager.GetUsersInRoleAsync(Roles.Teacher);
            if (adminUsers.Any())
            {
                return;
            }


            var adminUser = new User
            {
                UserName = "Teacher@teacher.com",
                Email = "Teacher@teacher.com",
                NormalizedEmail = "Teacher@teacher.com",
                SchoolIndex = "TeacherIndex"
            };

            var result = await userManager.CreateAsync(adminUser, "Teacher123");


            //Teacher@teacher.com
            //Teacher123

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Teacher);
            }
            else
            {
                throw new Exception("Failed to create the Teacher");
            }


        }
    }
}
