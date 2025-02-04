# Delivery-System
This project is a RESTful API for a restaurant application. It allows users to browse dishes, manage their shopping baskets, place orders, and rate dishes. The API uses JWT (JSON Web Tokens) for authentication and authorization.

## Table of Contents

-   [Features](#features)
-   [Technologies Used](#technologies-used)
-   [Getting Started](#getting-started)
    -   [Prerequisites](#prerequisites)
    -   [Installation](#installation)
    -   [Database Setup](#database-setup)
    -   [Configuration](#configuration)
-   [Running the Application](#running-the-application)
-   [API Documentation](#api-documentation)
    -   [Authentication](#authentication)
    -   [User Endpoints](#user-endpoints)
    -   [Dish Endpoints](#dish-endpoints)
    -   [Basket Endpoints](#basket-endpoints)
    -   [Order Endpoints](#order-endpoints)
    -    [Rating Endpoints](#rating-endpoints)
-   [Contributing](#contributing)
-   [License](#license)

## Features

-   **User Management:** User registration, login, logout, profile management.
-   **Dish Catalog:** Browse dishes with filtering and sorting options.
-   **Shopping Basket:** Add, remove, and view items in the user's shopping basket.
-   **Order Placement:** Place orders and track their status.
-   **Dish Rating:** Rate dishes based on user orders.
-   **JWT Authentication:** Secure endpoints using JWT for authorization.
-   **API Documentation:** Swagger UI for interactive API exploration.

## Technologies Used

-   **C#:** Programming language.
-   **.NET 8:** Framework.
-   **ASP.NET Core:** Web framework.
-   **Entity Framework Core:** ORM for database interaction.
-   **SQL Server:** Database system.
-   **AutoMapper:** Library for object-to-object mapping.
-   **JWT Bearer:** Authentication and authorization scheme.
-   **Swagger/OpenAPI:** API documentation.

## Getting Started

Follow these steps to set up and run the application:

### Prerequisites

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) or [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
-   A code editor such as [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/)

### Installation

1.  Clone the repository:

    ```bash
    git clone <repository-url>
    ```
2.  Navigate to the project directory:

    ```bash
    cd <project-directory>
    ```

### Database Setup

1.  Create a new SQL Server database.
2.  Update the connection string in `appsettings.json` with your SQL Server connection details:

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=<your-server>;Database=<your-database>;User Id=<your-user-id>;Password=<your-password>;TrustServerCertificate=True"
    }
    ```

3. Run the following command to apply migrations. Make sure you are in the root project folder.

   ```bash
    dotnet ef database update
    ```

### Configuration

1.  Update `appsettings.json` with the following settings:

    -   **JWT Settings**:
        ```json
        "JWT": {
            "SigningKey": "<your-secret-key>",
            "Issuer": "<your-issuer>",
            "Audience": "<your-audience>"
        }
        ```
        - Replace `<your-secret-key>` with a secure key used to sign JWT tokens.
        - Replace `<your-issuer>` and `<your-audience>` with the desired JWT issuer and audience.

## Running the Application

1.  Navigate to the project's root directory.
2.  Run the application using the .NET CLI:

    ```bash
    dotnet run
    ```

3.  The application will start and be accessible at `https://localhost:<port>`, with the port specified in the `launchSettings.json`.

## API Documentation

The API documentation can be accessed through the Swagger UI, which is available at `https://localhost:<port>/swagger`.

### Authentication

All the protected endpoints require a valid JWT token to authorize access. To obtain a token use the `/api/user/login` endpoint and provide the credentials of a valid user. Then, add the token as a `Bearer` token in the authorization header.

### User Endpoints

-   **`POST /api/user/register`**: Registers a new user.
    -   Request Body: `RegisterDto`
    -   Response: `200` if successfull, `400` otherwise.
-   **`POST /api/user/login`**: Logs in a user and generates a JWT token.
    -   Request Body: `LoginDto`
    -   Response: `200` with JWT token if successfull, `401` if invalid credentials, `400` otherwise
-   **`POST /api/user/logout`**: Logs out the current user. Requires a valid JWT Token in the headers.
    -   Response: `200` if successfull.
-   **`GET /api/user/profile`**: Gets the profile information of the current user. Requires a valid JWT Token in the headers.
     -   Response: `200` if successfull, `404` if the user is not found, `400` if the token is invalid
 -   **`PUT /api/user/updateprofile`**: Updates the profile information of the current user. Requires a valid JWT Token in the headers
     -    Request Body: `UpdateProfileDto`
     -    Response: `200` if successfull, `404` if the user is not found, `400` if there is a model state error

### Dish Endpoints

-   **`GET /api/dish`**: Retrieves a list of dishes.
    -   Query Parameters: `categories`, `Vegetarian`, `sortBy`, `page`
    -   Response: `200` with a list of `DishDto`, `400` if there is an invalid parameter, `404` if the dishes are not found, `500` for internal server errors
-   **`GET /api/dish/{id}`**: Retrieves a single dish by its ID.
    -   Response: `200` with `DishDto`, `400` if there is an invalid id, `404` if the dish is not found, `500` for internal server errors

### Basket Endpoints

-   **`GET /api/basket`**: Retrieves the basket of the authenticated user. Requires a valid JWT Token in the headers.
    -   Response: `200` with `BasketDto`, `404` if the basket is not found, `400` if the token is not valid, `500` if there is an internal server error
-   **`POST /api/basket/dish/{dishId}`**: Adds a dish to the authenticated user's basket. Requires a valid JWT Token in the headers
     - Query Parameters: `amount`
    -   Response: `200` if successfull, `404` if the dish is not found, `400` if the token is not valid or the amount is invalid, `500` if there is an internal server error.
-   **`DELETE /api/basket/dish/{dishId}`**: Removes a specific item from the user's basket. Requires a valid JWT Token in the headers
     -  Query Parameters: `increase` - if it is true then the amount of the item will be reduced, otherwise it will be removed
     -   Response: `200` with `BasketDto`, `404` if the item is not found, `400` if the user id is not found, `500` if there is an internal server error

### Order Endpoints

-   **`POST /api/order`**: Creates a new order for the authenticated user. Requires a valid JWT Token in the headers.
     -  Request Body: `CreateOrderDTO`
    -    Response: `200` if successfull, `400` if the token is invalid or there is a problem creating the order.
-   **`GET /api/order/user`**: Retrieves all orders placed by the authenticated user. Requires a valid JWT Token in the headers.
    -   Response: `200` with a list of `OrderInfoDTO`, `400` if the user id is invalid, `500` if there is an internal server error
-   **`GET /api/order/{orderId}`**: Retrieves a specific order by its ID. Requires a valid JWT Token in the headers.
    -  Response: `200` with the order details, `400` if the user id is invalid, `404` if the order is not found, `500` if there is an internal server error
-   **`PUT /api/order/{orderId}/delivered`**: Sets the status of a specific order to "Delivered". Requires a valid JWT Token in the headers.
    -    Response: `200` if successfull, `400` if the user id is invalid, `404` if the order is not found, `500` if there is an internal server error

### Rating Endpoints
 -  **`GET /api/rating/{dishId}/rating`**: Check if the user is allowed to rate a dish. Requires a valid JWT Token in the headers.
       -  Response: `200` with a boolean value indicating if the user can rate the dish, `401` if the user is not authenticated, `500` if there is an internal server error.
-  **`POST /api/rating/{dishId}/rate/{score}`**: Creates a rating for a specific dish. Requires a valid JWT Token in the headers.
      - Response: `200` with a `RatingDTO`, `400` if the score is invalid or there is a problem creating the rating, `500` if there is an internal server error

## Contributing

Contributions are welcome! Please follow these steps:

1.  Fork the repository.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes.
4.  Test your changes.
5.  Submit a pull request.

