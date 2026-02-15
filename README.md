# TaskMan - Task Management Application

A full-stack web application for managing user tasks with a .NET Core backend, React frontend, and SQL Server database.

## 📋 Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Quick Start](#quick-start)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Database Setup](#database-setup)
- [RabbitMQ Setup](#rabbitmq-setup-optional---required-for-task-reminder-service)
- [Running the Application](#running-the-application)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [SQL Query - Tasks with Multiple Tags](#sql-query---tasks-with-multiple-tags)
- [Key Implementations](#key-implementations)
- [Validation Rules](#validation-rules)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

TaskMan is a comprehensive task management system that allows users to create, view, update, and delete tasks with detailed information including user assignments, priorities, due dates, and multiple tags. The application features a modern, responsive UI with real-time state management and robust validation on both frontend and backend.

## ✨ Features

### Task Management
- ✅ **Create Tasks** - Add new tasks with complete details
- ✅ **View Tasks** - Display all tasks in a sortable, filterable data grid
- ✅ **Update Tasks** - Edit existing task information
- ✅ **Delete Tasks** - Remove tasks with confirmation

### Task Details
Each task contains:
- **Title** - Task name (3-200 characters)
- **Description** - Detailed task description (10-2000 characters)
- **Due Date** - Task deadline (validated to prevent past dates)
- **Priority** - Low, Medium, or High priority levels
- **User Assignment** - Assigned user with full details
- **Tags** - Multiple tags per task (N:N relationship, 1-10 tags)

### User Management
- ✅ **Create Users** - Add new users with validation
- ✅ **View Users** - List all users
- ✅ **Update Users** - Edit user information
- ✅ **Delete Users** - Remove users (with task cascade handling)
- **User Details** include:
  - Full Name
  - Email (with format validation)
  - Telephone (with format validation)

### Tag Management
- ✅ **Create Tags** - Add new tags for task categorization
- ✅ **View Tags** - List all available tags
- ✅ **Update Tags** - Edit tag names
- ✅ **Delete Tags** - Remove unused tags

### UI Features
- ✅ **Responsive Design** - Works on desktop, tablet, and mobile devices
- ✅ **Material-UI Components** - Modern, accessible UI components
- ✅ **Data Grid** - Sortable and filterable task table
- ✅ **Form Validation** - Real-time validation with helpful error messages
- ✅ **Character Counters** - Visual feedback on input length requirements
- ✅ **Navigation** - React Router for seamless page transitions
- ✅ **State Management** - Redux Toolkit with RTK Query for efficient data handling
- ✅ **Error Handling** - Comprehensive error messages and handling

## 🛠️ Tech Stack

### Backend
- **.NET Core 10.0** - Web API framework
- **Entity Framework Core 10.0.3** - ORM for database operations
- **SQL Server** - Relational database

### Frontend
- **React 19.2.0** - UI framework
- **TypeScript** - Type-safe JavaScript
- **Redux Toolkit 2.11.2** - State management
- **RTK Query** - Data fetching and caching
- **React Router DOM** - Client-side routing
- **Material-UI (MUI) 7.3.8** - Component library
- **MUI DataGrid 8.27.1** - Advanced data table
- **Vite** - Build tool and dev server

### Background Service
- **.NET Core Worker Service** - Long-running background service
- **RabbitMQ.Client 6.7.0** - Message queue integration

## 🚀 Quick Start

For those who want to get started quickly (assuming prerequisites are installed):

```bash
# 1. Clone and navigate to project
cd TaskMan

# 2. Setup and run backend
cd API
dotnet restore
dotnet run
# API will be at http://localhost:5000

# 3. In a new terminal, setup and run frontend
cd client
npm install
npm run dev
# App will be at http://localhost:5173
```

**Note:** Database will be automatically created and seeded on first API run. For RabbitMQ Task Reminder Service setup, see the [RabbitMQ Setup](#rabbitmq-setup-optional---required-for-task-reminder-service) section.

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 10.0 SDK or later** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18.x or later** - [Download](https://nodejs.org/)
- **npm 9.x or later** (comes with Node.js)
- **SQL Server 2019 or later** - [Download SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
  - Or SQL Server LocalDB (included with Visual Studio)
- **Git** - [Download](https://git-scm.com/)
- **RabbitMQ 3.12 or later** (Optional, required for Task Reminder Service) - [Download](https://www.rabbitmq.com/download.html)
  - Can be installed via **Chocolatey** on Windows (see [RabbitMQ Setup](#rabbitmq-setup) section)

### Verify Installation
```bash
# Check .NET version
dotnet --version

# Check Node.js version
node --version

# Check npm version
npm --version
```

## 🚀 Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd TaskMan
```

### 2. Backend Setup

#### Install Dependencies
```bash
cd API
dotnet restore
```

#### Configure Database Connection
Edit `appsettings.json` and update the connection string if needed:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskManDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Connection String Options:**
- **Windows Authentication:** `Server=localhost;Database=TaskManDb;Trusted_Connection=True;TrustServerCertificate=True`
- **SQL Server Authentication:** `Server=localhost;Database=TaskManDb;User Id=your_username;Password=your_password;TrustServerCertificate=True`
- **LocalDB:** `Server=(localdb)\\mssqllocaldb;Database=TaskManDb;Trusted_Connection=True;TrustServerCertificate=True`

### 3. Frontend Setup

```bash
cd ../client
npm install
```

## 🗄️ Database Setup

### Apply Migrations & Seed Data

The application will automatically create and seed the database on first run. However, you can manually apply migrations:

```bash
cd API

# Apply migrations
dotnet ef database update

# Or create a new migration (if needed)
dotnet ef migrations add MigrationName
```

### Seed Data
The application automatically seeds the database with:
- **5 Users** - Sample users with complete information
- **10 Tags** - Common task tags (Urgent, Work, Personal, Shopping, Health, Finance, Education, Home, Travel, Project)
- **10 Sample Tasks** - Pre-populated tasks with various priorities and tags

### Manual Database Creation (if needed)
If automatic migration fails, create the database manually:

```sql
-- In SQL Server Management Studio or Azure Data Studio
CREATE DATABASE TaskManDb;
GO
```

Then run migrations:
```bash
cd API
dotnet ef database update
```

## 🐰 RabbitMQ Setup (Optional - Required for Task Reminder Service)

### Install RabbitMQ via Chocolatey

If you have **Chocolatey** installed, run this command in an **administrative command prompt**:

```bash
choco install rabbitmq
```

**Note:** If you don't have Chocolatey installed, download it from [https://chocolatey.org/install](https://chocolatey.org/install)

### Start RabbitMQ Service

After installation, RabbitMQ should start automatically. To verify:

```bash
# Check if RabbitMQ service is running
Get-Service RabbitMQ
```

To start the service manually:

```bash
net start RabbitMQ
```

### Create Admin User

By default, RabbitMQ only has a `guest` user with limited permissions. To create an admin user for the Task Reminder Service:

1. **Open Administrative Command Prompt** and navigate to the RabbitMQ installation directory:

```bash
cd "C:\Program Files\RabbitMQ Server\rabbitmq_server-xxx\sbin"
```

2. **Create the admin user** with username `admin` and password `admin123`:

```bash
rabbitmqctl add_user admin admin123
```

3. **Set admin user permissions**:

```bash
rabbitmqctl set_permissions -p "/" admin ".*" ".*" ".*"
```

4. **Make admin user an administrator**:

```bash
rabbitmqctl set_user_tags admin administrator
```

### Verify Setup

Access the RabbitMQ Management UI at:
- **URL:** http://localhost:15672/
- **Username:** admin
- **Password:** admin123

You should see the admin dashboard with queues and connections.

### Configuration File

The Task Reminder Service uses these RabbitMQ settings (in `TaskReminderService/appsettings.json`):

```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "UserName": "admin",
    "Password": "admin123",
    "Port": 5672,
    "VirtualHost": "/"
  }
}
```

## ▶️ Running the Application

### Start Backend API
```bash
cd API
dotnet run
```

The API will be available at:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:7000

### Start Frontend Development Server
```bash
cd client
npm run dev
```

The React app will be available at:
- **Local:** http://localhost:5173

### Access the Application

1. **Open your browser** and navigate to `http://localhost:5173`
2. You should see the TaskMan application homepage
3. The backend API is running at `http://localhost:5000`
4. Test the API directly at `http://localhost:5000/api/tasks`

**What you should see:**
- Tasks table with pre-seeded data (10 sample tasks)
- Navbar with options to create Tasks, Users, and Tags
- Fully functional CRUD operations

**Note:** The database is automatically created, migrated, and seeded with sample data when you first run the API.

### Start Task Reminder Service (Optional)

The Task Reminder Service automatically polls for overdue tasks and publishes reminders to RabbitMQ. This requires RabbitMQ and the `admin` user to be set up.

**Prerequisites:**
- RabbitMQ running locally on port 5672
- Admin user created (`admin` / `admin123`)
- Backend API running on `http://localhost:5000`

**To start the service:**

```bash
cd TaskReminderService
dotnet run
```

**Expected Output:**
```
info: TaskReminderService.Services.TaskRemainderService[0]
      RabbitMQ connection established in service
info: TaskReminderService.Services.TaskRemainderService[0]
      ✓ Subscribed to queue 'Remainder'
info: TaskReminderService.Services.TaskRemainderService[0]
      Task Reminder Service started
info: TaskReminderService.Services.TaskRemainderService[0]
      Checking for overdue tasks...
warn: TaskReminderService.Services.TaskRemainderService[0]
      ✓ Hi John Doe your Task is due Fix critical bug in production (Task ID: 1)
```

The service will:
- Poll the API every 10 seconds for overdue tasks
- Publish task reminders to RabbitMQ
- Log warnings with the user's full name and task details

## 📁 Project Structure

```
TaskMan/
├── API/                                 # Backend .NET Core API
│   ├── Controllers/                    # API Controllers
│   │   ├── TasksController.cs         # Task CRUD operations
│   │   ├── UsersController.cs         # User CRUD operations
│   │   └── TagsController.cs          # Tag CRUD operations
│   ├── Data/                          # Database context and seeding
│   │   ├── AppDbContext.cs            # EF Core DbContext
│   │   ├── Seed.cs                    # Database seed logic
│   │   └── SeedFile.json              # Seed data
│   ├── DTOs/                          # Data Transfer Objects
│   │   ├── CreateTaskDto.cs
│   │   ├── UpdateTaskDto.cs
│   │   ├── TaskDto.cs
│   │   ├── CreateUserDto.cs
│   │   ├── UpdateUserDto.cs
│   │   ├── UserDto.cs
│   │   └── TagDto.cs
│   ├── Entities/                      # Database Models
│   │   ├── Task.cs                    # Task entity
│   │   ├── User.cs                    # User entity
│   │   ├── Tag.cs                     # Tag entity
│   │   ├── TaskTag.cs                 # Junction table for N:N
│   │   └── TaskPriority.cs            # Priority enum
│   ├── Migrations/                    # EF Core migrations
│   ├── Program.cs                     # Application entry point
│   ├── appsettings.json              # Configuration
│   └── API.csproj                     # Project file
│
├── client/                            # Frontend React Application
│   ├── src/
│   │   ├── components/               # Reusable components
│   │   │   └── Navbar.tsx            # Navigation bar component
│   │   ├── pages/                    # Page components
│   │   │   ├── Layout.tsx            # Main layout with routing
│   │   │   ├── NewTaskForm.tsx       # Create task page
│   │   │   ├── EditTaskForm.tsx      # Edit task dialog
│   │   │   ├── TasksTable.tsx        # Task list data grid
│   │   │   ├── NewUserForm.tsx       # Create user page
│   │   │   └── NewTagForm.tsx        # Create tag page
│   │   ├── services/                 # API service layer (RTK Query)
│   │   │   ├── tasksApi.ts           # Task API endpoints
│   │   │   ├── usersApi.ts           # User API endpoints
│   │   │   └── tagsApi.ts            # Tag API endpoints
│   │   ├── store/                    # Redux store configuration
│   │   │   ├── store.ts              # Store setup
│   │   │   └── hooks.ts              # Typed Redux hooks
│   │   ├── types/                    # TypeScript type definitions
│   │   │   └── api.ts                # API types
│   │   ├── validation/               # Form validation logic
│   │   │   ├── taskValidation.ts
│   │   │   ├── userValidation.ts
│   │   │   └── tagValidation.ts
│   │   ├── App.tsx                   # Root component
│   │   ├── main.tsx                  # Application entry
│   │   └── index.css                 # Global styles
│   ├── package.json                  # Dependencies
│   ├── tsconfig.json                 # TypeScript config
│   └── vite.config.ts                # Vite configuration
│
├── TaskReminderService/              # Background Service for Task Reminders
│   ├── Services/                     # Service implementations
│   │   └── TaskRemainderService.cs   # RabbitMQ integration
│   ├── Program.cs                    # Service entry point
│   ├── Worker.cs                     # Background worker
│   ├── appsettings.json             # Service configuration
│   └── TaskReminderService.csproj   # Project file
│
└── README.md                          # This file
```

## 🔌 API Endpoints

### Tasks
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks` | Get all tasks |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create new task |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Delete task |

### Users
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/users` | Create new user |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Delete user |

### Tags
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tags` | Get all tags |
| GET | `/api/tags/{id}` | Get tag by ID |
| POST | `/api/tags` | Create new tag |
| PUT | `/api/tags/{id}` | Update tag |
| DELETE | `/api/tags/{id}` | Delete tag |

## 📊 SQL Query - Tasks with Multiple Tags

This query returns all tasks that have **at least two tags**, including tag names, sorted by the number of tags in descending order:

```sql
SELECT 
    t.Id AS TaskId,
    t.Title,
    t.Description,
    t.DueDate,
    t.Priority,
    u.FullName AS AssignedUser,
    u.Email AS UserEmail,
    COUNT(tt.TagId) AS TagCount,
    STRING_AGG(tg.Name, ', ') WITHIN GROUP (ORDER BY tg.Name) AS TagNames
FROM 
    Tasks t
    INNER JOIN Users u ON t.UserId = u.Id
    INNER JOIN TaskTags tt ON t.Id = tt.TaskId
    INNER JOIN Tags tg ON tt.TagId = tg.Id
GROUP BY 
    t.Id, t.Title, t.Description, t.DueDate, t.Priority, u.FullName, u.Email
HAVING 
    COUNT(tt.TagId) >= 2
ORDER BY 
    TagCount DESC,
    t.Title ASC;
```

### Query Explanation
- **Joins all necessary tables**: Tasks, Users, TaskTags (junction), and Tags
- **Groups by task**: To count and aggregate tags per task
- **HAVING clause**: Filters to only include tasks with 2 or more tags
- **STRING_AGG**: Concatenates tag names into a comma-separated list
- **ORDER BY**: Sorts by tag count (descending), then by title (ascending)

### Alternative Query (With Priority Name)
```sql
SELECT 
    t.Id AS TaskId,
    t.Title,
    t.Description,
    t.DueDate,
    CASE t.Priority
        WHEN 1 THEN 'Low'
        WHEN 2 THEN 'Medium'
        WHEN 3 THEN 'High'
    END AS PriorityLevel,
    u.FullName AS AssignedUser,
    u.Email AS UserEmail,
    u.Telephone AS UserPhone,
    COUNT(tt.TagId) AS TagCount,
    STRING_AGG(tg.Name, ', ') WITHIN GROUP (ORDER BY tg.Name) AS TagNames
FROM 
    Tasks t
    INNER JOIN Users u ON t.UserId = u.Id
    INNER JOIN TaskTags tt ON t.Id = tt.TaskId
    INNER JOIN Tags tg ON tt.TagId = tg.Id
GROUP BY 
    t.Id, t.Title, t.Description, t.DueDate, t.Priority, 
    u.FullName, u.Email, u.Telephone
HAVING 
    COUNT(tt.TagId) >= 2
ORDER BY 
    TagCount DESC,
    t.DueDate ASC;
```

### Sample Output
```
TaskId | Title              | TagCount | TagNames
-------|--------------------|----------|-------------------
5      | Project Planning   | 4        | Home, Urgent, Work, Personal
8      | Code Review        | 3        | Urgent, Work, Personal
2      | Design Meeting     | 2        | Work, Urgent
```

## 🔑 Key Implementations

### 1. Entity Framework Configuration
- **Database Context**: Centralized DbContext with DbSet properties for all entities
- **Relationships**: 
  - One-to-Many: User to Tasks
  - Many-to-Many: Tasks to Tags (via TaskTags junction table)
- **Migrations**: Automatic database schema generation and updates
- **Seeding**: Initial data population for testing and demonstration

### 2. State Management (Redux Toolkit)
- **RTK Query**: Automatic API call caching and state management
- **Normalized Cache**: Efficient data storage and updates
- **Automatic Refetching**: Data updates propagate across the application
- **Optimistic Updates**: Immediate UI feedback with rollback on error
- **Tagged Cache Invalidation**: Automatic data refresh on mutations

### 3. Validation Strategy

#### Frontend Validation (React)
- **Real-time validation**: As users type
- **Separate validation files**: Clean, maintainable code
- **Visual feedback**: Error messages, character counters, input constraints
- **Prevents invalid API calls**: Catches errors before submission

#### Backend Validation (.NET)
- **Data Annotations**: Model-level validation on DTOs
- **Custom validation methods**: Business logic validation
- **ModelState validation**: Automatic validation checking
- **Comprehensive error messages**: Detailed feedback for debugging

### 4. N:N Relationship Implementation
- **Junction Table**: TaskTags entity for Many-to-Many relationship
- **Navigation Properties**: Seamless traversal between Tasks and Tags
- **Cascade Behavior**: Proper handling of deletions
- **Tag Selection**: Multi-select dropdown with chip display
- **API DTOs**: TagIds array for efficient data transfer

### 5. Responsive UI Design
- **Material-UI Theme**: Consistent design system
- **Breakpoints**: Responsive layouts for different screen sizes
- **Mobile-First**: Works seamlessly on mobile devices
- **Accessibility**: ARIA labels and keyboard navigation
- **User Experience**: Loading states, success messages, error handling

### 6. Error Handling
- **Try-Catch Blocks**: Comprehensive error catching
- **Custom Error Messages**: User-friendly error descriptions
- **API Error Responses**: Structured error objects
- **Fallback UI**: Graceful degradation on errors
- **Logging**: Console and server-side logging for debugging

### 7. RESTful API Design
- **Resource-Based URLs**: Clear, predictable endpoint structure
- **HTTP Verbs**: Proper use of GET, POST, PUT, DELETE
- **Status Codes**: Correct HTTP status codes (200, 201, 400, 404, etc.)
- **DTOs**: Separation of database entities and API contracts
- **CORS Configuration**: Secure cross-origin access

## ✅ Validation Rules

### Task Validation
| Field | Rules |
|-------|-------|
| Title | Required, 3-200 characters |
| Description | Required, 10-2000 characters |
| Due Date | Required, cannot be in the past, max 10 years in future |
| Priority | Required, must be 1 (Low), 2 (Medium), or 3 (High) |
| User | Required, must exist in database |
| Tags | Required, 1-10 tags, all must exist in database |

### User Validation
| Field | Rules |
|-------|-------|
| Full Name | Required, 2-100 characters |
| Email | Required, valid email format, unique |
| Telephone | Required, valid phone format, unique |

### Tag Validation
| Field | Rules |
|-------|-------|
| Name | Required, 2-50 characters, unique |

## 🐛 Troubleshooting

### Common Issues

#### "Cannot connect to SQL Server"
- Verify SQL Server is running: `Get-Service MSSQLSERVER` (Windows)
- Check connection string in `API/appsettings.json`
- Try using LocalDB: `Server=(localdb)\\mssqllocaldb;...`

#### "Database already exists" error
- Drop and recreate: `dotnet ef database drop` then `dotnet ef database update`
- Or manually delete TaskManDb in SQL Server Management Studio

#### "Port already in use"
- Backend (5000/7000): Change ports in `API/Properties/launchSettings.json`
- Frontend (5173): Change port in `vite.config.ts` or use `PORT=3000 npm run dev`

#### Frontend can't connect to API
- Check CORS configuration in `API/Program.cs`
- Verify API is running on http://localhost:5000
- Check browser console for specific error messages

#### RabbitMQ connection failed
- Verify RabbitMQ service is running: `Get-Service RabbitMQ`
- Check admin user exists and has correct permissions
- Verify port 5672 is not blocked by firewall

### Database Connection Issues
```bash
# Test SQL Server connection
sqlcmd -S localhost -E -Q "SELECT @@VERSION"

# If using LocalDB
sqllocaldb start mssqllocaldb
```

### Entity Framework Issues
```bash
# Clear migrations and recreate
rm -rf Migrations/
dotnet ef migrations add InitialCreate
dotnet ef database update

# Update EF tools
dotnet tool update --global dotnet-ef
```

### Frontend Build Issues
```bash
# Clear node modules and reinstall
cd client
rm -rf node_modules package-lock.json
npm install

# Clear Vite cache
rm -rf .vite
npm run dev
```

### Port Conflicts
If ports 5000, 7000, or 5173 are in use:
- **Backend**: Modify `Properties/launchSettings.json`
- **Frontend**: Modify `vite.config.ts` or set `PORT=3000 npm run dev`

## 📝 Development Notes

### Best Practices Implemented
- ✅ Separation of Concerns (Controllers, Services, Entities, DTOs)
- ✅ Repository Pattern (via Entity Framework DbContext)
- ✅ Dependency Injection
- ✅ Async/Await for all I/O operations
- ✅ Strong typing with TypeScript
- ✅ Component-based architecture
- ✅ Clean code principles
- ✅ Comprehensive error handling
- ✅ Input validation (client and server)
- ✅ Responsive design patterns

### Future Enhancements
- Unit tests for API controllers
- Integration tests for database operations
- E2E tests with Playwright or Cypress
- User authentication and authorization
- Task comments and attachments
- Real-time updates with SignalR
- Advanced filtering and search
- Task status tracking (Todo, In Progress, Done)
- Pagination for large datasets
- Export tasks to CSV/PDF

## 📄 License

This project is developed as an interview assignment.

## 👤 Author

Interview Assignment - TaskMan Project

---

**Note**: This application is designed for local development and demonstration purposes. For production deployment, additional security measures, authentication, and configuration would be required.
