using Microsoft.EntityFrameworkCore;
using Edusync.Data; 

public class TestDbContext
{
    // This method sets up an in-memory database and returns the context
    public static SchoolManagementDbContext GetInMemoryDbContext()
    {
        // Define options for using the InMemory database provider
        var options = new DbContextOptionsBuilder<SchoolManagementDbContext>()
                      .UseInMemoryDatabase(databaseName: "TestDatabase") // Define the database name for testing
                      .Options;

        // Create a new instance of the context using those options
        var context = new SchoolManagementDbContext(options);
        
        return context;
    }
}
