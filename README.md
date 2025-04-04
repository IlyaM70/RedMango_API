# RedMango API Documentation

## Overview

**RedMango API** is a RESTful service for restaurant built with C# and designed following the OpenAPI Specification (OAS) 3.0. This API (version 1.0) provides endpoints for user authentication, menu management, order processing, payment handling, and shopping cart functionality.

## Features

- **Menu Management:** Provides CRUD operations for restaurant menu items.
- **Order Processing:** Endpoints to place, track, and manage orders.
- **User Authentication:** Secure endpoints for user login and registration.
- **Scalability:** Designed to handle high traffic and concurrent requests.

## Technologies

- **Framework:** ASP.NET Core, C#
- **Data Access:** Entity Framework Core
- **Database:** SQL Server on Azure
- **Tools:** Visual Studio, Git, GitHub

## Demo & Client

- **Live Demo:** Check out the [RedMango API Demo](https://redmangoapi12.azurewebsites.net/index.html) to experience the restaurant website in action.
- **Web Client:** Explore the Web Client code in the [RedMango Repository](https://github.com/IlyaM70/redmango).

## API Endpoints

### Authentication

- **POST /api/auth/login**  
  Authenticate a user using credentials.  
  _Payload:_ `LoginRequestDto`

- **POST /api/auth/register**  
  Register a new user.  
  _Payload:_ `RegisterRequestDto`

### Authorization Test

- **GET /api/AuthTest**  
  Test the authentication mechanism; returns a general authentication test result.

- **GET /api/AuthTest/{id}**  
  Retrieve a specific authentication test result by ID.

### MenuItem

- **GET /api/MenuItem**  
  Retrieve a list of all menu items.

- **POST /api/MenuItem**  
  Create a new menu item.  
  _Payload:_ Menu item details.

- **GET /api/MenuItem/{id}**  
  Retrieve details for a specific menu item by ID.

- **PUT /api/MenuItem/{id}**  
  Update an existing menu item.  
  _Payload:_ Updated menu item details.

- **DELETE /api/MenuItem/{id}**  
  Delete a menu item by ID.

### Order

- **GET /api/Order**  
  Retrieve a list of all orders.

- **POST /api/Order**  
  Create a new order.  
  _Payload:_ Order details (e.g., using `OrderHeaderCreateDto` and `OrderDetailsCreateDto`).

- **GET /api/Order/{id}**  
  Retrieve details for a specific order by ID.

- **PUT /api/Order/{id}**  
  Update an existing order.  
  _Payload:_ Updated order details (e.g., `OrderHeaderUpdateDto`).

### Payment

- **POST /api/Payment**  
  Process a payment transaction.  
  _Payload:_ Payment details.

### ShoppingCart

- **GET /api/ShoppingCart**  
  Retrieve the current shopping cart for a user.

- **POST /api/ShoppingCart**  
  Add items to the shopping cart or create a new shopping cart.  
  _Payload:_ Shopping cart details.
