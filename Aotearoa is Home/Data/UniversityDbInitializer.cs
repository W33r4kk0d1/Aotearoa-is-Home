using Aotearoa_is_Home.Models;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Data
{
    public static class UniversityDbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var context =
                services.GetRequiredService<UniversityDbContext>();

            await context.Database.MigrateAsync();

            // If students already exist, don't add them again
            if (await context.UniversityStudents.AnyAsync())
            {
                return;
            }

            var students = new List<UniversityStudent>
            {
                // Current student
                new UniversityStudent
                {
                    StudentId = "STU001",
                    FirstName = "John",
                    LastName = "Smith",
                    StudentEmail = "john.smith@student.ac.nz",
                    ApplicationEmail = "john.smith@gmail.com",
                    IsCurrentStudent = true
                },

                // Current student
                new UniversityStudent
                {
                    StudentId = "STU002",
                    FirstName = "Sarah",
                    LastName = "Brown",
                    StudentEmail = "sarah.brown@student.ac.nz",
                    ApplicationEmail = null,
                    IsCurrentStudent = true
                },

                // Previous applicant / not currently enrolled
                new UniversityStudent
                {
                    StudentId = "STU003",
                    FirstName = "David",
                    LastName = "Wilson",
                    StudentEmail = "david.wilson@student.ac.nz",
                    ApplicationEmail = "david.wilson@gmail.com",
                    IsCurrentStudent = false
                }
            };

            await context.UniversityStudents.AddRangeAsync(students);

            await context.SaveChangesAsync();
        }
    }
}