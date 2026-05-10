# RestaurantOrderApp

A modern desktop application developed in WPF (Windows Presentation Foundation) using the **MVVM** (Model-View-ViewModel) pattern to manage the order flow within a restaurant.

## 🚀 Key Features

* **Role-Based Authentication**: Secure login for Clients and Administrators.
* **Menu Management**: Browse products by category, check ingredients and view allergens.
* **Ordering Process**: Add products to the cart, calculate total costs and place orders.
* **Staff/Admin Dashboard**:
    * Inventory and product management.
    * Monitor and update order statuses.
    * Sales report generation.
* **Database Integration**: Powered by SQL Server via Entity Framework Core.

## 🛠️ Tech Stack

* **.NET 8.0** & **WPF**
* **Entity Framework Core** (SQL Server)
* **MVVM Pattern** (implemented using `BaseViewModel` and `RelayCommand`)
* **Microsoft Data SqlClient** for stored procedures

## 📂 Project Structure

* **Models/**: Database entities (`User`, `Product`, `Order`, etc.) and the `RestaurantDbContext`.
* **ViewModels/**: Application logic and state management for the UI.
* **Views/**: XAML-based user interfaces.
* **Helpers/**: Utility classes for commands, user sessions, and converters.

## ⚙️ Setup and Installation

1.  **Database**: 
    * Ensure a local `SQLEXPRESS` server is running.
    * The application targets a database named `RestaurantDB`.
2.  **Connection Configuration**:
    * The default connection string is located in `RestaurantDbContext.cs`.
3.  **Run**:
    * Open `RestaurantOrderApp.sln` in Visual Studio and run the project.

## 👥 Author

* **Cosmin Pelin**
