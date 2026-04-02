# 📚 OnlineLibrary - ASP.NET Fundamentals Project

A comprehensive online library management system built with **ASP.NET Core MVC** and **Entity Framework Core**. This project demonstrates modern web development practices including authentication, role-based authorization, a full Repository-Service-Controller architecture, CRUD operations, and a clean layered architecture.

> **SoftUni Exam Project** - January 2026

---

## 🚀 Features

- **Book Management**: Add, edit, view, and delete books with detailed information
- **User Authentication**: Secure user registration and login using ASP.NET Core Identity
- **Role-Based Authorization**: Admin, Manager, and User roles with a separate Admin area
- **Reading Tracking**: Mark books as read, add reading dates, and rate books (1-5 stars)
- **Genre Categorization**: Organize books by genres (Fiction, NonFiction, Mystery, Fantasy, ScienceFiction, Biography, History, Romance, Thriller, SelfHelp, Other)
- **Author & Publisher Management**: Complete CRUD operations for authors and publishers (both regular and admin)
- **Quick Add Feature**: Add new authors/publishers on-the-fly while creating books
- **My Books Collection**: View and manage only the books you've added to the library
- **Favorites Collection**: Add/remove books to/from a personal favorites list
- **Filter & Search Capabilities**: Full-text search plus genre/publisher filters across Books, My Books, and Favorites
- **Pagination**: Server-side pagination throughout all listing pages
- **Admin Panel**: Dedicated admin area for managing books (including restore of soft-deleted books), authors, publishers, and users
- **User Management (Admin)**: Assign/remove roles and delete user accounts
- **Slug-based URLs**: SEO-friendly URLs for book detail pages
- **Soft Delete**: Books are soft-deleted and can be restored by Admins
- **Responsive UI**: Modern Bootstrap 5 interface with custom styling

---

## 🏗️ Project Structure

The solution follows a **clean, layered Repository-Service-Controller architecture** split across 9 projects:

```
OnlineLibrary/
├── OnlineLibrary/                              # Main web application (ASP.NET Core MVC)
│   ├── Areas/
│   │   ├── Admin/                              # Admin area (Role-protected)
│   │   │   ├── Controllers/
│   │   │   │   ├── BaseAdminController.cs      # Base: [Area("Admin")][Authorize(Roles="Admin")]
│   │   │   │   ├── HomeController.cs           # Admin dashboard
│   │   │   │   ├── AuthorManagementController.cs
│   │   │   │   ├── BookManagementController.cs # Includes Restore action
│   │   │   │   ├── PublisherManagementController.cs
│   │   │   │   └── UserManagementController.cs # Role assignment & user deletion
│   │   │   └── Views/
│   │   │       ├── Home/
│   │   │       ├── AuthorManagement/
│   │   │       ├── BookManagement/
│   │   │       ├── PublisherManagement/
│   │   │       └── UserManagement/
│   │   └── Identity/
│   │       └── Pages/Account/                  # Scaffolded Identity pages
│   │           ├── Login.cshtml(.cs)
│   │           ├── Register.cshtml(.cs)        # Auto-assigns "User" role on registration
│   │           └── Logout.cshtml(.cs)
│   ├── Controllers/
│   │   ├── BaseController.cs                   # [AutoValidateAntiforgeryToken], GetUserId()
│   │   ├── HomeController.cs
│   │   ├── AuthorController.cs
│   │   ├── BooksController.cs
│   │   └── PublisherController.cs
│   ├── Views/
│   │   ├── Author/
│   │   ├── Books/
│   │   ├── Publisher/
│   │   ├── Home/
│   │   └── Shared/
│   ├── wwwroot/                                # Static files (CSS, JS, images, libs)
│   ├── Program.cs                              # App entry point, DI, middleware
│   ├── appsettings.json                        # Production configuration
│   └── appsettings.Development.json            # Development configuration
│
├── OnlineLibrary.Data/                         # Data access layer
│   ├── Repository/
│   │   ├── Contracts/
│   │   │   ├── IAuthorRepository.cs
│   │   │   ├── IBookRepository.cs
│   │   │   └── IPublisherRepository.cs
│   │   ├── BaseRepository.cs                   # Common EF Core operations
│   │   ├── AuthorRepository.cs
│   │   ├── BookRepository.cs                   # Soft delete, admin bypass methods
│   │   └── PublisherRepository.cs
│   ├── Configuration/
│   │   ├── AuthorConfiguration.cs              # Unique index on FullName
│   │   ├── BookConfiguration.cs
│   │   ├── PublisherConfiguration.cs           # Unique index on Name
│   │   ├── BookAuthorConfiguration.cs
│   │   └── DatabaseSeeder.cs                   # Seeds roles, admin user, sample data
│   ├── Migrations/
│   └── OnlineLibraryDbContext.cs
│
├── OnlineLibrary.Data.Models/                  # Domain/entity models
│   ├── ApplicationUser.cs                      # IdentityUser<Guid>
│   ├── Author.cs
│   ├── Book.cs                                 # Includes IsDeleted soft-delete flag
│   ├── Publisher.cs
│   ├── BookAuthor.cs                           # Many-to-many (Book ↔ Author)
│   └── UserBook.cs                             # Many-to-many (User ↔ Book favorites)
│
├── OnlineLibrary.Services.Core/                # Business logic layer
│   ├── Interfaces/
│   │   ├── IAuthorService.cs
│   │   ├── IBooksService.cs
│   │   └── IPublisherService.cs
│   ├── Admin/
│   │   ├── Interfaces/
│   │   │   ├── IAuthorManagementService.cs     # Extends IAuthorService
│   │   │   ├── IBookManagementService.cs       # Extends IBooksService + admin methods
│   │   │   └── IPublisherManagementService.cs  # Extends IPublisherService
│   │   ├── AuthorManagementService.cs
│   │   ├── BookManagementService.cs            # Inherits BooksService
│   │   └── PublisherManagementService.cs
│   ├── AuthorService.cs
│   ├── BooksService.cs
│   └── PublisherService.cs
│
├── OnlineLibrary.Services.Models/              # DTOs (Data Transfer Objects)
│   ├── Author/
│   │   ├── AuthorsAllDto.cs
│   │   ├── AuthorDetailsDto.cs
│   │   ├── AuthorBookDto.cs
│   │   └── AuthorDeleteDto.cs
│   ├── Book/
│   │   ├── BookAllDto.cs
│   │   ├── BookDetailsDto.cs
│   │   ├── BookCreateDto.cs
│   │   ├── BookEditDto.cs
│   │   ├── BookDeleteDto.cs
│   │   └── BookFavoritesDto.cs
│   └── Publisher/
│       ├── PublisherAllDto.cs
│       ├── PublisherDetailsDto.cs
│       ├── PublisherAddDto.cs
│       ├── PublisherBookDto.cs
│       └── PublisherDeleteDto.cs
│
├── OnlineLibrary.Services.CustomMappers/       # Manual mapping layer (no AutoMapper)
│   ├── AuthorMappers.cs
│   ├── BookMappers.cs
│   └── PublisherMappers.cs
│
├── OnlineLibrary.Web.ViewModels/               # View models for MVC views
│   ├── Author/
│   ├── Books/
│   ├── Publisher/
│   ├── Admin/UserManagement/
│   │   └── UserViewModel.cs
│   └── ErrorViewModel.cs
│
├── OnlineLibrary.Web.Infrastructure/           # Cross-cutting utilities & DI extensions
│   ├── Extensions/
│   │   └── WebApplicationBuilderExtension.cs  # Reflection-based DI auto-registration
│   └── Utilities/
│       ├── Contracts/ISlugGenerator.cs
│       └── SlugGenerator.cs                   # URL slug generation
│
├── OnlineLibrary.GCommon/                      # Shared constants & domain exceptions
│   ├── ApplicationConstants.cs                # DateTimeFormat, DefaultImageUrl
│   ├── ValidationConstants.cs                 # All validation min/max lengths & ranges
│   └── Exceptions/
│       ├── AuthorExceptions/                  # 5 typed author exceptions
│       └── PublisherExceptions/               # 5 typed publisher exceptions
│
└── OnlineLibrary.Tests/                        # Unit test project (20 test classes)
    ├── AdminAuthorManagementControllerTests.cs
    ├── AdminBookManagementControllerTests.cs
    ├── AdminBookRepositoryTests.cs
    ├── AdminHomeControllerTests.cs
    ├── AdminPublisherManagementControllerTests.cs
    ├── AdminUserManagementControllerTests.cs
    ├── AuthorControllerTests.cs
    ├── AuthorRepositoryTests.cs
    ├── AuthorServiceTests.cs
    ├── BaseControllerTests.cs
    ├── BookManagementServiceTests.cs
    ├── BookMappersTests.cs
    ├── BookRepositoryTests.cs
    ├── BooksControllerTests.cs
    ├── BooksServiceTests.cs
    ├── HomeControllerTests.cs
    ├── PublisherControllerTests.cs
    ├── PublisherRepositoryTests.cs
    ├── PublisherServiceTests.cs
    └── SlugGeneratorTests.cs
```

---

## 🏛️ Architecture — Repository-Service-Controller Pattern

The project strictly follows a **layered, loosely-coupled architecture**:

```
[ Views / Razor Pages ]
        ↓
[ Controllers ]          ← BaseController (CSRF protection, GetUserId())
        ↓
[ Service Layer ]        ← Interfaces in Services.Core/Interfaces/
        ↓                   Admin services extend core services
[ Repository Layer ]     ← Interfaces in Data/Repository/Contracts/
        ↓
[ EF Core DbContext ]    ← OnlineLibraryDbContext
        ↓
[ SQL Server Database ]
```

**Supporting layers:**
- `Services.Models` (DTOs) — flow between Repository ↔ Service
- `Web.ViewModels` — flow between Service ↔ Controller ↔ View
- `Services.CustomMappers` — explicit manual mappings (no third-party mapper)
- `GCommon` — shared validation constants and domain exceptions
- `Web.Infrastructure` — reflection-based DI registration and slug utility

**Dependency Injection** is auto-registered via reflection in `WebApplicationBuilderExtension.cs`, which scans assemblies and maps all `IRepository` / `IService` interface-to-implementation pairs by naming convention.

---

## 🔑 Admin Area

The Admin area is a fully separate MVC area accessible only to users in the **Admin** role. All admin controllers inherit `BaseAdminController`, which applies `[Area("Admin")]`, `[Authorize(Roles = "Admin")]`, and `[AutoValidateAntiforgeryToken]` globally.

| Admin Controller | Key Actions |
|-----------------|-------------|
| `HomeController` | Admin dashboard |
| `AuthorManagementController` | Manage, Add, Edit, Delete authors |
| `BookManagementController` | Manage (incl. soft-deleted), Create, Edit, Delete, **Restore** |
| `PublisherManagementController` | Manage, Add, Edit, Delete publishers |
| `UserManagementController` | List users, AssignRole, RemoveRole, DeleteUser |

Authenticated admins are **automatically redirected** to `/Admin/Home/Index` when visiting the home page (middleware in `Program.cs`).

---

## 🗄️ Database Schema

### Core Entities

**Books**
| Column | Type | Notes |
|--------|------|-------|
| `Id` | Guid | Primary Key |
| `Title` | string (max 250) | Required |
| `Description` | string (max 1000) | Required |
| `Genre` | Enum | Required (Fiction, NonFiction, Mystery, Fantasy, ScienceFiction, Biography, History, Romance, Thriller, SelfHelp, Other) |
| `IsRead` | bool | |
| `DateRead` | DateTime? | Optional |
| `Rating` | int (0–5) | 0 = not rated |
| `CoverUrl` | string? (max 2083) | Optional, valid URL when provided |
| `DateAdded` | DateTime | Required |
| `PublisherId` | Guid FK | Required |
| `AddedByUserId` | Guid FK | Required — every book is linked to a user |
| `IsDeleted` | bool | Soft delete flag |

**Authors**
| Column | Type | Notes |
|--------|------|-------|
| `Id` | Guid | Primary Key |
| `FullName` | string (max 150) | Required, **unique index** |

**Publishers**
| Column | Type | Notes |
|--------|------|-------|
| `Id` | Guid | Primary Key |
| `Name` | string (max 200) | Required, **unique index** |

**BooksAuthors** (Many-to-Many)
| Column | Type |
|--------|------|
| `BookId` | Guid (FK) |
| `AuthorId` | Guid (FK) |
| `IsDeleted` | bool |

**UsersBooks** (User Favorites — Many-to-Many)
| Column | Type |
|--------|------|
| `UserId` | Guid (FK) |
| `BookId` | Guid (FK) |

---

## 🛠️ Technologies & Frameworks

| Technology | Version | Purpose |
|-----------|---------|---------|
| **ASP.NET Core MVC** | 10.0 | Web framework |
| **Entity Framework Core** | 10.0 | ORM & Data access |
| **ASP.NET Core Identity** | 10.0 | Authentication & Authorization |
| **SQL Server** | — | Database |
| **Bootstrap** | 5.3.2 | UI framework |
| **Razor** | — | View engine |
| **C#** | 13.0 / .NET 10 | Programming language |
| **xUnit** | — | Unit testing |
| **Moq** | — | Mocking framework for tests |

---

## 📋 Prerequisites

Before running this project, ensure you have:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or higher
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full version)
- [Visual Studio 2022 or 2026](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/) (optional)

---

## ⚙️ Installation & Setup

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/llefterov/OnlineLibrary-ASP-NET-Fundamentals_Jan26-_Project.git
cd OnlineLibrary-ASP-NET-Fundamentals_Jan26-_Project
```

### 2️⃣ Configure Connection String

Update the connection string in `OnlineLibrary/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=OnlineLibraryDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3️⃣ Apply Database Migrations

```bash
cd OnlineLibrary
dotnet ef database update
```

This will create the database and run the seeder, which provisions:
- **3 Roles**: Admin, Manager, User
- **1 Admin User** (email: `admin@onlinelibrary.com`, password: `Admin123!`) — assigned the **Admin** role
- **5 Authors**: Jane Austen, George Orwell, Isaac Asimov, J.R.R. Tolkien, Agatha Christie
- **5 Publishers**: Apress, Manning Publications, O'Reilly Media, Packt Publishing, Addison-Wesley
- **5 Sample books** with correct author–book mappings (all added by the Admin user)

> **Note:** Every new user who self-registers via the Register page is automatically assigned the **User** role.

### 4️⃣ Run the Application

```bash
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`

---

## 👤 Default Admin User

| Property | Value |
|----------|-------|
| **Email** | admin@onlinelibrary.com |
| **Password** | Admin123! |
| **Role** | Admin |

> ⚠️ **Security Warning:** Change this password immediately in production environments!

---

## 🎯 Usage

### Creating an Account
1. Click **Register** in the navigation menu
2. Fill in your email and password
3. Confirm your registration — you are automatically assigned the **User** role

### Adding a Book
1. Log in to your account
2. Navigate to **Books** > **Add New Book**
3. Fill in: Title, Description, Genre, Cover URL, Publisher, Author(s), reading status and rating
4. Use the **Quick Add** links to create a new author or publisher on-the-fly
5. Click **Create**

### Viewing My Books
Navigate to **Books** > **My Books** to see all books you have personally added, with search and filter support.

### Managing Your Favorites
- Navigate to **Books** > **Favorites** to see your personal favorites collection
- Click **Save** on any book to add it to your favorites
- Click **Remove** to take it out of your favorites

### Filtering & Search
Use the search box and dropdowns on the **All Books**, **My Books**, and **Favorites** pages to filter by:
- Title keyword
- Publisher
- Genre

### Managing Authors & Publishers
1. **View All**: Navigate to Authors / Publishers section for a paginated, searchable list
2. **Add New**: Click "Add New Author/Publisher"
3. **Edit / Delete**: Use the action buttons on each entry
4. **Details**: Click an author or publisher to see all associated books

### Admin Panel
1. Log in as an Admin — you will be redirected to `/Admin/Home/Index`
2. Use the sidebar to manage Books, Authors, Publishers, or Users
3. In Book Management, soft-deleted books are shown and can be **Restored**
4. In User Management, assign/remove roles (Admin, Manager, User) or delete accounts

---

## 📊 Sample Data

**Authors:**
| Full Name |
|-----------|
| Jane Austen |
| George Orwell |
| Isaac Asimov |
| J.R.R. Tolkien |
| Agatha Christie |

**Books:**
| Title | Author | Publisher | Genre |
|-------|--------|-----------|-------|
| Pride and Prejudice | Jane Austen | Apress | Biography |
| 1984 | George Orwell | Manning Publications | ScienceFiction |
| Foundation | Isaac Asimov | O'Reilly Media | ScienceFiction |
| The Hobbit | J.R.R. Tolkien | Packt Publishing | Fantasy |
| Murder on the Orient Express | Agatha Christie | Addison-Wesley | Mystery |

**Publishers:** Apress, Manning Publications, O'Reilly Media, Packt Publishing, Addison-Wesley

---

## 🔐 Security Features

- **ASP.NET Core Identity** for user management with `IdentityUser<Guid>`
- Password hashing and validation
- **CSRF protection** via `[AutoValidateAntiforgeryToken]` on all controllers (base classes)
- **Role-based authorization** — 3 roles seeded: **Admin**, **Manager**, **User**
  - New registrations are automatically assigned the **User** role
  - Admin area protected by `[Authorize(Roles = "Admin")]`
  - Admins can assign/remove any role via the User Management panel

#### Role Permissions Matrix

> An Admin can **remove the "User" role** from any account via the User Management panel. Once the "User" role is removed the account becomes a **Blank (no role)** account and loses all authenticated-only features — it can only browse the public listing pages.

| Action | Blank (no role) | User | Manager | Admin |
|--------|:-----------:|:----:|:-------:|:-----:|
| **Books** | | | | |
| Browse all books — All | ✅ | ✅ | ✅ | ✅ |
| Book — Details | ❌ | ✅ | ✅ | ✅ |
| Favorites (Save / Remove) | ❌ | ✅ | ✅ | ✅ |
| View My Books list | ❌ | ❌ | ✅ | ✅ |
| Create a book | ❌ | ❌ | ✅ | ✅ |
| Edit a book | ❌ | ❌ | ✅ **own books only** | ✅ any book |
| Delete a book | ❌ | ❌ | ✅ **own books only** | ✅ any book |
| Restore soft-deleted books | ❌ | ❌ | ❌ | ✅ |
| **Authors** | | | | |
| Browse all authors — All | ✅ | ✅ | ✅ | ✅ |
| Author — Details | ❌ | ✅ | ✅ | ✅ |
| Author Add / Edit / Delete | ❌ | ❌ | ✅ | ✅ |
| **Publishers** | | | | |
| Browse all publishers — All | ✅ | ✅ | ✅ | ✅ |
| Publisher — Details | ❌ | ✅ | ✅ | ✅ |
| Publisher Add / Edit / Delete | ❌ | ❌ | ✅ | ✅ |
| **Administration** | | | | |
| Admin area (user mgmt, book/author/publisher mgmt) | ❌ | ❌ | ❌ | ✅ |

> **Manager ownership rule**: The `Edit` and `Delete` actions verify `book.AddedByUserId == currentUserId` at the repository level. A Manager who attempts to edit or delete a book they did not create receives an `UnauthorizedAccessException` (HTTP 401).
- Account lockout after failed login attempts
- Secure cookie authentication
- Soft delete for data integrity (books are never hard-deleted by regular users)
- Unique indexes on Author FullName and Publisher Name to prevent duplicates
- Domain exceptions (`AuthorAlreadyExistsException`, `PublisherAlreadyExistsException`, etc.) for meaningful error handling

### Environment-Specific Identity Options

| Setting | Development | Production |
|---------|-------------|------------|
| Min password length | 6 chars | 8+ chars |
| Required unique chars | 0 | 4 |
| Max failed login attempts | 255 | 5 |
| Lockout duration | 1 minute | 5 minutes |

> Development settings are defined in `appsettings.Development.json`; production settings in `appsettings.json`.

---

## 🎨 UI/UX Features

- **Responsive Design**: Mobile-first with Bootstrap 5
- **Custom Branding**: Gradient navigation with brand colors
- **Card-Based Layout**: Modern card design for book display
- **Pagination Controls**: On all listing pages
- **Search & Filter Bar**: Inline search + dropdown filters on book lists
- **Hover Effects**: Interactive UI elements
- **Default Cover Image**: Placeholder shown when no cover URL is provided
- **Intuitive Navigation**: Role-aware nav links (Admin panel visible only to Admins)

---

## 🗺️ Routing

| Route Name | Pattern | Purpose |
|-----------|---------|---------|
| `areas` | `{area:exists}/{controller=Home}/{action=Index}/{id?}` | Admin area routing |
| `slugRoute` | `Books/Details/{slug:required}/{id:guid}` | SEO-friendly book detail URLs |
| `default` | `{controller=Home}/{action=Index}/{id?}` | Standard MVC routing |

---

## 🧩 Service Layer Details

### Core Services

**`IBooksService` / `BooksService`**
- Paginated book listing with multi-filter support (search, publisher, genre)
- User-scoped queries: All Books, My Books, Favorites
- Book create / edit / delete (ownership-checked)
- Favorites management (Save / Remove)
- Date strings formatted as `yyyy-MM-dd` (ApplicationConstants.DateTimeFormat)

**`IAuthorService` / `AuthorService`**
- Paginated author listing with search
- Author CRUD with deletion guard (cannot delete if books exist)
- `AuthorAlreadyExistsException` on duplicate name

**`IPublisherService` / `PublisherService`**
- Paginated publisher listing with search
- Publisher CRUD with deletion guard
- `PublisherAlreadyExistsException` on duplicate name

### Admin Services (extend core services)

**`IBookManagementService` / `BookManagementService`**
- All `IBooksService` operations **without** ownership checks
- `GetAllBooksForAdminDtoAsync()` — includes soft-deleted books
- `RestoreBookForAdminDtoAsync()` — restore a soft-deleted book
- `EditBookForAdminDtoAsync()` / `DeleteBookForAdminDtoAsync()` — admin overrides

**`IAuthorManagementService`** / **`IPublisherManagementService`** — extend their respective core services (no additional methods)

---

## 🗃️ Repository Layer Details

### `IBookRepository` highlights
- `GetAllBooksForAdminAsync()` — returns all books including soft-deleted
- `RestoreBookForAdminAsync()` — clears the `IsDeleted` flag
- `GetBookForAdminEditAsync()` / `EditBookForAdminAsync()` — admin bypasses ownership check
- `IsBookAddedByUserAsync()` / `IsBookAddedToUserCollectionAsync()` — ownership/collection checks
- All list methods materialize with `ToListAsync()` and return `IEnumerable`

### `IAuthorRepository` / `IPublisherRepository`
- Full CRUD with unique-constraint-aware update
- `ExistsAsync()` for pre-deletion validation

---

## 🧪 Unit Tests

The `OnlineLibrary.Tests` project contains **20 test classes** covering all layers:

| Layer | Test Classes |
|-------|-------------|
| **Controllers (regular)** | `HomeControllerTests`, `BooksControllerTests`, `AuthorControllerTests`, `PublisherControllerTests`, `BaseControllerTests` |
| **Controllers (admin)** | `AdminHomeControllerTests`, `AdminBookManagementControllerTests`, `AdminAuthorManagementControllerTests`, `AdminPublisherManagementControllerTests`, `AdminUserManagementControllerTests` |
| **Services** | `BooksServiceTests`, `AuthorServiceTests`, `PublisherServiceTests`, `BookManagementServiceTests` |
| **Repositories** | `BookRepositoryTests`, `AuthorRepositoryTests`, `PublisherRepositoryTests`, `AdminBookRepositoryTests` |
| **Mappers & Utilities** | `BookMappersTests`, `SlugGeneratorTests` |

---

## 🧰 Infrastructure & Utilities

### Dependency Injection Auto-Registration
`WebApplicationBuilderExtension.cs` uses **reflection** to scan assemblies and automatically register all repository and service interfaces with their implementations by naming convention — no manual `AddScoped<IFoo, Foo>()` calls needed for new repositories/services.

### Slug Generator
`SlugGenerator` converts book titles to URL-friendly slugs:
- `"The Great Gatsby"` → `"the-great-gatsby"`
- Strips non-URL-safe characters, collapses hyphens, trims ends

Used in the `slugRoute` pattern: `Books/Details/{slug}/{id}`.

---

## 📝 Validation Rules

**Books:**
- `Title`: Required, 2–250 characters
- `Description`: Required, 2–1000 characters
- `Genre`: Required (must be valid enum value)
- `CoverUrl`: **Optional** — valid URL format when provided (7–2083 chars)
- `AddedByUserId`: Required — every book must be associated with a user
- `PublisherId`: Required — must reference an existing publisher
- `Rating`: 0–5 (0 = not rated)

**Authors:**
- `FullName`: Required, 2–150 characters, **unique**

**Publishers:**
- `Name`: Required, 2–200 characters, **unique**

---

## 🔮 Future Enhancements

- [ ] Search functionality by title, author, or genre (enhanced full-text)
- [ ] Book reviews and comments
- [ ] Reading statistics dashboard
- [ ] Book recommendations based on reading history
- [ ] Export/Import book collections
- [ ] Social features (share lists, follow users)
- [ ] Advanced sorting options
- [ ] Book cover upload (instead of URLs only)
- [ ] Multi-language support
- [ ] Reading goals and challenges
- [ ] Integration with external book APIs (Google Books, Open Library)

---

## 🤝 Contributing

This is an educational project for SoftUni. If you'd like to contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is created for educational purposes as part of the SoftUni ASP.NET Fundamentals course (January 2026).

---

## 👤 Author

**llefterov**
- GitHub: [@llefterov](https://github.com/llefterov)

---

## 🙏 Acknowledgments

- **SoftUni** - For the excellent ASP.NET Fundamentals course
- **Microsoft** - For the comprehensive .NET documentation
- **Bootstrap Team** - For the amazing UI framework

---

## 📞 Support

If you have any questions or issues, please open an issue in the GitHub repository.

---

<div align="center">

**⭐ Star this repository if you find it helpful! ⭐**

Made with ❤️ for learning ASP.NET Core MVC

</div>
