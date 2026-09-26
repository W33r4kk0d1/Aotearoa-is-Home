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


            // Seed Organizations
            if (!await context.Organizations.AnyAsync())
            {
                var organizations = new List<Organization>
                {
                    new Organization
                    {
                        Name = "WelTec"
                    },

                    new Organization
                    {
                        Name = "Whitireia"
                    }
                };

                await context.Organizations.AddRangeAsync(organizations);
                await context.SaveChangesAsync();
            }


            // Seed Employees
            if (!await context.Employees.AnyAsync())
            {
                var welTec = await context.Organizations
                    .FirstAsync(o => o.Name == "WelTec");

                var whitireia = await context.Organizations
                    .FirstAsync(o => o.Name == "Whitireia");

                var employees = new List<Employee>
                {
                    // WelTec employee
                    new Employee
                    {
                        EmployeeId = "EMP001",
                        FirstName = "Michael",
                        LastName = "Taylor",
                        Email = "michael.taylor@weltec.ac.nz",
                        OrganizationId = welTec.Id,
                        IsActive = true
                    },

                    new Employee
                    {
                        EmployeeId = "EMP002",
                        FirstName = "Emma",
                        LastName = "Johnson",
                        Email = "emma.johnson@weltec.ac.nz",
                        OrganizationId = welTec.Id,
                        IsActive = true
                    },

                    // Whitireia employee
                    new Employee
                    {
                        EmployeeId = "EMP003",
                        FirstName = "Daniel",
                        LastName = "Brown",
                        Email = "daniel.brown@whitireia.ac.nz",
                        OrganizationId = whitireia.Id,
                        IsActive = true
                    },

                    // Inactive employee - useful for testing
                    new Employee
                    {
                        EmployeeId = "EMP005",
                        FirstName = "James",
                        LastName = "Anderson",
                        Email = "james.anderson@weltec.ac.nz",
                        OrganizationId = welTec.Id,
                        IsActive = false
                    }
                };

                await context.Employees.AddRangeAsync(employees);

                await context.SaveChangesAsync();
            }

            // Seed Students
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