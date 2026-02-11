# 📚 OnlineLibrary - ASP.NET Fundamentals Project

A comprehensive online library management system built with **ASP.NET Core MVC** and **Entity Framework Core**. This project demonstrates modern web development practices including authentication, CRUD operations, and a clean layered architecture.

> **SoftUni Exam Project** - January 2026

---

## 🚀 Features

- **Book Management**: Add, edit, view, and delete books with detailed information
- **User Authentication**: Secure user registration and login using ASP.NET Core Identity
- **Reading Tracking**: Mark books as read, add reading dates, and rate books (1-5 stars)
- **Genre Categorization**: Organize books by genres (Biography, Romance, Mystery, Fantasy, Science Fiction, Horror, Thriller, Historical Fiction, Self-Help, Other)
- **Author & Publisher Management**: Complete CRUD operations (Create, Read, Update, Delete) for authors and publishers
- **Quick Add Feature**: Add new authors/publishers on-the-fly while creating books
- **My Books Collection**: View and manage only the books you've added to the library
- **User Book Collections**: Users can maintain their personal book collections
- **Filter Capabilities**: View all books by a specific author or publisher through Details pages
- **Responsive UI**: Modern Bootstrap 5 interface with custom styling

---

## 🏗️ Project Structure

The solution follows a **clean, layered architecture**:

```
OnlineLibrary/
├── OnlineLibrary/                          # Main web application (MVC)
│   ├── Areas/                              # Identity scaffolded pages
│   ├── Controllers/                        # MVC Controllers (Books, Authors, Publishers, Home)
│   ├── Views/                              # Razor views
│   ├── wwwroot/                            # Static files (CSS, JS, images)
│   ├── Properties/                         # Launch settings
│   ├── Program.cs                          # Application entry point
│   ├── appsettings.json                    # Configuration
│   └── OnlineLibrary.Web.csproj            # Web project file
│
├── OnlineLibrary.Data/                     # Data access layer
│   ├── Configuration/                      # Entity configurations (Fluent API)
│   ├── Migrations/                         # EF Core database migrations
│   ├── OnlineLibraryDbContext.cs           # EF Core DbContext
│   └── OnlineLibrary.Data.csproj           # Data project file
│
├── OnlineLibrary.Data.Models/              # Domain models
│   ├── Author.cs                           # Author entity
│   ├── Book.cs                             # Book entity
│   ├── BookAuthor.cs                       # Many-to-many relationship
│   ├── Publisher.cs                        # Publisher entity
│   ├── UserBook.cs                         # User book collection
│   ├── Enums/                              # Genre enum
│   └── OnlineLibrary.Data.Models.csproj    # Models project file
│
├── OnlineLibrary.Services.Core/            # Business logic layer
│   ├── Interfaces/                         # Service interfaces
│   │   ├── IBooksService.cs
│   │   ├── IAuthorService.cs
│   │   └── IPublisherService.cs
│   ├── Exceptions/                         # Custom exceptions
│   ├── BooksService.cs                     # Book operations
│   ├── AuthorService.cs                    # Author operations
│   ├── PublisherService.cs                 # Publisher operations
│   └── OnlineLibrary.Services.Core.csproj  # Services project file
│
├── OnlineLibrary.Web.ViewModels/           # View models and DTOs
│   ├── Books/                              # Book-related view models
│   ├── Authors/                            # Author-related view models
│   ├── Publishers/                         # Publisher-related view models
│   ├── ErrorViewModel.cs
│   └── OnlineLibrary.Web.ViewModels.csproj # ViewModels project file
│
├── OnlineLibrary.GCommon/                  # Shared constants and utilities
│   ├── ValidationConstants.cs              # Validation rules
│   └── OnlineLibrary.GCommon.csproj        # Common project file
│
└── OnlineLibrary.slnx                      # Solution file
```

---

## 🗄️ Database Schema

### Core Entities

**Books**
- `Id` (Guid) - Primary Key
- `Title` (string, max 250 chars)
- `Description` (string, max 1000 chars)
- `Genre` (Enum: Biography, Romance, Mystery, Fantasy, ScienceFiction, Horror, Thriller, HistoricalFiction, SelfHelp, Other)
- `IsRead` (bool)
- `DateRead` (DateTime?)
- `Rating` (int, 0-5)
- `CoverUrl` (string, max 2083 chars)
- `DateAdded` (DateTime)
- `PublisherId` (int, Foreign Key)
- `AddedByUserId` (string?, Foreign Key to AspNetUsers)
- `IsDeleted` (bool, soft delete)

**Authors**
- `Id` (int) - Primary Key
- `FullName` (string, max 150 chars)

**Publishers**
- `Id` (int) - Primary Key
- `Name` (string, max 200 chars)

**BooksAuthors** (Many-to-Many)
- `BookId` (Guid)
- `AuthorId` (int)

**UsersBooks** (User Collections)
- `UserId` (string)
- `BookId` (Guid)

---

## 🛠️ Technologies & Frameworks

| Technology | Version | Purpose |
|-----------|---------|---------|
| **ASP.NET Core MVC** | 6.0+ | Web framework |
| **Entity Framework Core** | 6.0+ | ORM & Data access |
| **ASP.NET Core Identity** | 6.0+ | Authentication & Authorization |
| **SQL Server** | - | Database |
| **Bootstrap** | 5.3.2 | UI framework |
| **Razor** | - | View engine |
| **C#** | 10.0+ | Programming language |

---

## 📋 Prerequisites

Before running this project, ensure you have:

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) or higher
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/) (optional)

---

## ⚙️ Installation & Setup

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/llefterov/OnlineLibrary-ASP-NET-Fundamentals_Jan26-_Project.git
cd OnlineLibrary-ASP-NET-Fundamentals_Jan26-_Project
```

### 2️⃣ Configure Connection String

Update the connection string in `OnlineLibrary/appsettings.json`:

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

This will create the database with seed data including:
- 5 Authors (Jane Austen, George Orwell, Isaac Asimov, R.R. Tolkien, Agatha Christie)
- 5 Publishers (Apress, Manning Publications, O'Reilly Media, Packt Publishing, Addison-Wesley)
- 5 Sample books with correct author-book mappings

### 4️⃣ Run the Application

```bash
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`

---

## 🎯 Usage

### Creating an Account
1. Click **Register** in the navigation menu
2. Fill in your email and password (minimum 6 characters, 4 unique characters)
3. Confirm your registration

### Adding a Book
1. Log in to your account
2. Navigate to **Books** > **Add New Book**
3. Fill in the book details:
   - Title
   - Description
   - Genre (dropdown)
   - Cover URL (image link)
   - Publisher (dropdown or quick-add new)
   - Reading status and rating (optional)
4. Select existing author or add new author on-the-fly
5. Click **Create**

### Viewing My Books
Navigate to **Books** > **My Books** to see a personalized view of all books you've added to the library. This filtered view shows only the books created by the currently logged-in user, making it easy to manage your contributions to the library.

### Managing Your Collection
- Mark books as read/unread
- Add reading dates
- Rate books (1-5 stars)
- View all books in the library
- Add books to your personal collection
- Remove books from your collection
- View your personal book contributions in 'My Books'
- Filter by author or publisher through Details pages

### Managing Authors & Publishers
1. **View All**: Navigate to Authors/Publishers section to see complete lists
2. **Add New**: Click "Add New Author/Publisher" to create entries
3. **Edit**: Modify existing author or publisher information
4. **Delete**: Remove authors or publishers (with validation checks)
5. **View Details**: See all books associated with a specific author or publisher
6. **Quick Add**: When creating a book, add new authors/publishers without leaving the page

---

## 📊 Sample Data

The database is seeded with sample data:

**Authors:**
| ID | Full Name |
|----|-----------|
| 1 | Jane Austen |
| 2 | George Orwell |
| 3 | Isaac Asimov |
| 4 | R.R. Tolkien |
| 5 | Agatha Christie |

**Books:**
| Title | Author | Publisher | Genre |
|-------|--------|-----------|-------|
| Pride and Prejudice | Jane Austen | Apress | Biography |
| 1984 | George Orwell | Manning Publications | ScienceFiction |
| Foundation | Isaac Asimov | O'Reilly Media | ScienceFiction |
| The Hobbit | R.R. Tolkien | Packt Publishing | Fantasy |
| Murder on the Orient Express | Agatha Christie | Addison-Wesley | Mystery |

**Publishers:**
- Apress
- Manning Publications
- O'Reilly Media
- Packt Publishing
- Addison-Wesley

---

## 🔐 Security Features

- **ASP.NET Core Identity** for user management
- Password hashing and validation
- Enhanced password requirements (6+ characters, 4 unique characters)
- Account lockout after 5 failed login attempts (5-minute duration)
- CSRF protection
- Secure cookie authentication
- User-specific book collections
- Soft delete for data integrity

---

## 🎨 UI/UX Features

- **Responsive Design**: Mobile-first approach with Bootstrap 5
- **Custom Branding**: Gradient navigation with brand colors
- **Card-Based Layout**: Modern card design for book display
- **User-Specific Views**: 'My Books' page for personal contributions
- **Hover Effects**: Interactive UI elements
- **Clean Typography**: Professional font hierarchy
- **Intuitive Navigation**: Easy access to all features

---

## 📝 Validation Rules

**Books:**
- Title: 2-250 characters
- Description: 2-1000 characters
- Cover URL: Valid URL format, 7-2083 characters
- Rating: 0-5 stars
- Genre: Must be valid enum value

**Authors:**
- Full Name: 2-150 characters

**Publishers:**
- Name: 2-200 characters

---

## 🧪 Future Enhancements

- [ ] Search functionality by title, author, or genre
- [ ] Book reviews and comments
- [ ] Reading statistics dashboard
- [ ] Book recommendations based on reading history
- [ ] Export/Import book collections
- [ ] Social features (share lists, follow users)
- [ ] Advanced filtering and sorting options
- [ ] Book cover upload (instead of URLs only)
- [ ] Multi-language support
- [ ] Reading goals and challenges
- [ ] Integration with external book APIs

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