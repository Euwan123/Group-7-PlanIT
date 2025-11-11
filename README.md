# 🛰️ PlanIt

**PlanIt** is an all-in-one productivity system developed in **C#** with **MySQL** as its database.  
It combines **note-taking**, **checklists**, **scheduling**, and a **built-in dictionary** in one platform to help users stay organized and productive.

---

## 🧩 Technologies Used

| Technology | Purpose |
|-------------|----------|
| **C#** | Main programming language used to develop the system |
| **MySQL / phpMyAdmin** | Database used for storing user information and records |
| **MySQL Connector for .NET** | Connects C# application to the MySQL database |
| **Visual Studio 2022** | IDE used for writing, debugging, and testing the project |
| **GitHub** | Version control and collaboration platform |

---

## ⚙️ Features and Functionalities

- **User Login & Registration** – Secure account management using MySQL authentication.  
- **Text Tab** – Note-taking area with word and character count.  
- **Checklist Tab** – To-do list system for task management.  
- **Schedule Tab** – Add and view schedules or reminders.  
- **Drawing Tab** – Simple sketching feature for quick visual notes.  
- **Dictionary Module** – Search, add, and manage words; includes admin-only approvals.  
- **Database Connectivity** – All user data saved and retrieved using MySQL Connector for .NET.

---

## 💻 Programming Paradigms

- **Imperative Programming:** Controls login flow and workspace operations.  
- **Logic Programming:** Rules for dictionary validation and admin privileges.  
- **Functional Programming:** LINQ methods for text analysis (`Count()`, `Split()`).  
- **Object-Oriented Programming (OOP):** Uses classes and **data encapsulation** to protect user information.

Example of encapsulation:
```csharp
private string username;
public string Username
{
    get { return username; }
    set { if (!string.IsNullOrEmpty(value)) username = value; }
}


How to Run the Program
🪜 Requirements

Visual Studio 2022 (or newer)

MySQL Server / phpMyAdmin

MySQL Connector for .NET installed

🪜 Steps

Clone this repository:

git clone https://github.com/<your-username>/PlanIt.git


Open the project in Visual Studio 2022.

Import the database:

Open phpMyAdmin or MySQL Workbench.

Import the file from /database/planitdb.sql.

Update the connection string in the C# code:

string connStr = "server=localhost;user=root;database=planitdb;password=;";


Run the system:

Press F5 or click Start in Visual Studio.

📊 System Architecture

Front-End: Text-based menus and user prompts in C#.

Back-End: Handles login, input validation, and database queries.

Database: MySQL storing users, notes, schedules, checklists, and dictionary data.

Connector: MySQL Connector for .NET for secure SQL transactions.


📜 License

This project is licensed under the MIT License.
You are free to use, modify, and distribute it with proper credit.

⭐ Developed by Team PlanIt – built with C#, MySQL, and innovation for productivity.
