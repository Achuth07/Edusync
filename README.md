# Edusync - School Management System
EduSync is a comprehensive web-based School Management System designed to modernize and streamline educational institution operations. It replaces traditional paper-based methods with a digital solution that enhances efficiency, improves communication,and provides real-time access to crucial information for administrators, teachers, and students.

# Build and Installation Guide

## Step 1: Install Required Software
Before downloading the project, make sure you have the following software installed:

1. **Git**: A distributed version control system for tracking changes in source code.  
   - [Download Git](https://git-scm.com/downloads)

2. **.NET SDK**: Includes everything needed to build and run .NET projects.  
   - [Download .NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet)

3. **Microsoft SQL Server**: A relational database management system required for your project.  
   - [Download SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or Developer edition will work)

4. **SQL Server Management Studio (SSMS)**: A tool to manage and connect to SQL Server databases.  
   - [Download SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

5. **Visual Studio Code or Visual Studio**: A code editor or IDE to work on the project.  
   - [Download Visual Studio Code](https://code.visualstudio.com/download)  
   - [Download Visual Studio](https://visualstudio.microsoft.com/downloads/)

---

## Step 2: Clone the GitLab Repository

1. Open Git Bash or Command Prompt on your machine.
2. Navigate to the directory where you want to clone the repository:
   ```sh
   cd path/to/your/folder
   ```
3. Clone the repository from GitLab using the provided HTTPS URL:
   ```sh
   git clone https://github.com/Achuth07/Edusync.git
   ```
4. Navigate to the project directory after cloning:
   ```sh
   cd Edusync
   ```

---

## Step 3: Set Up the SQL Server Database

### Install SQL Server and SSMS

1. Install Microsoft SQL Server if you haven’t already.  
2. Install SQL Server Management Studio (SSMS) for managing the database.

### Setting up Database – Method 1 (Preferred)

1. Obtain the `.bak` file (backup file) from the project repository (`SchoolManagementDb.bak`).
2. Copy it to a location accessible by SSMS (e.g., `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup`).
3. Open **SQL Server Management Studio (SSMS)** and connect to your local SQL Server instance.
4. Right-click on **Databases** in the **Object Explorer** and select **Restore Database**.
5. In the **Source** section, select **Device**, click on `...`, locate and select `SchoolManagementDb.bak`.
6. In the **Destination** section, set the database name to `SchoolManagementDb`.
7. Click **OK** to restore the database.

### Setting up Database – Method 2 (If Method 1 Doesn’t Work)

1. Locate the `SchoolManagementDb.sql` file in the project folder.
2. Open **SQL Server Management Studio (SSMS)** and connect to your local SQL Server instance.
3. Go to **File > Open > File**, then browse and select `SchoolManagementDb.sql`.
4. Click **Execute** to create the `SchoolManagementDb` database.
5. Refresh the database list in SSMS to see the new database.

---

## Step 4: Update the `appsettings.json` Connection String

1. Open the project in **Visual Studio Code**.
2. Locate and open the `appsettings.json` file in the project's root directory.
3. Update the connection string to match your local SQL Server instance.

### Example Connection Strings
#### Windows Authentication
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=SchoolManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;"
  }
}
```
- Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g., `(local)` or `localhost`).

#### SQL Server Authentication
```json
{
  "ConnectionStrings": {
    "SchoolManagementDbConnection": "Server=DESKTOP-BHPP4CG\\SQLEXPRESS,1433;Database=SchoolManagementDb;Trusted_Connection=False;MultipleActiveResultSets=True;Encrypt=False;User Id=your_user_id;Password=your_password;"
  }
}
```
- Replace `your_user_id` and `your_password` with your SQL Server credentials.

4. Save the changes.

---

## Step 5: Install .NET 8.0 SDK

1. [Download .NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet)
2. Verify installation:
   ```sh
   dotnet --version
   ```
   Ensure it shows `.NET 8.x.x`.

---

## Step 6: Restore Dependencies

1. Open a terminal in Visual Studio Code.
2. Run the following command to restore NuGet packages:
   ```sh
   dotnet restore
   ```
3. If `xUnit` package triggers errors, install it separately:
   ```sh
   dotnet add package xunit
   dotnet add package xunit.runner.visualstudio
   ```

---

## Step 7: Build and Run the Project

1. Build the project:
   ```sh
   dotnet build
   ```
2. Run the project:
   ```sh
   dotnet run
   ```
By default, the application runs on **HTTPS**.

### Default Credentials (For `.bak` Backup Users)
| Role    | Username | Password     |
|---------|---------|-------------|
| Admin   | Admin   | Edusync123!  |
| Teacher | Teacher | Edusync123!  |
| Student | Student | Edusync123!  |

### For `.sql` Script Users
- The program seeds an **Admin** account on the first run:
  - **Username**: `Admin`
  - **Password**: `Edusync123!`
- To create **Teacher** or **Student** accounts, use the **Register** option on the homepage.
- New accounts are registered as **Students** by default.
- Log in as **Admin**, navigate to **Manage Roles**, and assign **Teacher** roles as needed.

---

## Step 8: Running Unit Tests

1. Navigate to the test project folder:
   ```sh
   cd Edusync/Edusync.Tests
   ```
2. Run unit tests:
   ```sh
   dotnet test
   ```
This will execute all unit tests in the project.

---

## License
This project is licensed under the [MIT License](LICENSE).

