# Online.ECommerce Microservices

this Microservice e-commerce platform using:
- **UserCreation API** – manages user accounts and publishes `user.created` events.  
- **OrderCreation API** – manages orders and publishes `order.ceated` events.  
- **Kafka** – for event-driven communication.  
- **Docker Compose** – for full environment orchestration.  
- **HealthChecks UI** – for live health status.

## Project Structure

Online.ECommerce/
│
├── Common/ # Shared library (models, messaging, Kafka setup)
├── Common.Test/ # Shared library tests
│
├── UserCreation/ # User service API
├── UserCreation.Business/ # Business logic for user service
├── UserCreation.Test/ # Tests for user service
│
├── OrderCreation/ # Order service API
├── OrderCreation.Business/ # Business logic for order service
├── OrderCreation.Test/ # Tests for order service
│
├── docker-compose.yml # Multi-container orchestration
└── README.md # Project documentation (this file)


## Technologies Used

| Category | Technology |
|-----------|-------------|
| Language | C# (.NET 8) |
| Framework | ASP.NET Core Web API |
| Messaging | Apache Kafka |
| Database | In-Memory EF Core |
| Health Checks|
| Containerization | Docker & Docker Compose |


## Prerequisites

Make sure you have installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/downloads)

###  Clone the Repository

bash git clone https://github.com/harilakkakula/Online.ECommerce.git cd Online.ECommerce

## Run with Docker Compose

docker-compose up --build

-- Make sure you have to be in the solution folder

## This will start:

Zookeeper (:2181)
Kafka (:9092)
UserCreation API (:8081)
OrderCreation API (:8082)

## Verify Services

| Service           | URL                                                            |
| ----------------- | -------------------------------------------------------------- |
| UserCreation API  | [http://localhost:8081/swagger]                                |
| OrderCreation API | [http://localhost:8082/swagger]                                |
| HealthChecks UI   | [http://localhost:8081/health] [http://localhost:8082/health]  |


### API Endpoints

## UserCreation API

Base URL: http://localhost:8081
POST /users

Create a new user.
Request Body:
--------------------------------
{
    "Name":"Test",
    "Email":"sample4@gmail.com"
}

Response
-----------------------------
{
    "success": true,
    "message": "User created successfully.",
    "data": {
        "id": "cb1abd9c-299b-4072-9889-49f0c4cdc195",
        "name": "Test",
        "email": "sample4@gmail.com"
    }
}

This publishes a UserCreated Kafka event.

GET /users/{id}
Fetch user details by ID.
---------------------------------------

Response
------------------------
{
    "userId": "df56fa34-4d8e-486b-8acc-62edd9e07dd6",
    "name": "Test",
    "email": "sample4@gmail.com",
    "orders": [
        {
            "id": "0efc3cba-2ff3-4259-b04f-d48fdd957748",
            "product": " Mouse",
            "quantity": 2,
            "price": 29.99
        }
    ]
}


Get ALl User
---------------------------
GET /users/all

Parameters (Query):

| Name         | Type | Default | Description                |
| ------------ | ---- | ------- | -------------------------- |
| `pageNumber` | int  | 1       | Page number to retrieve    |
| `pageSize`   | int  | 10      | Number of records per page |

Sample Request:
GET http://localhost:8081/users/all?pageNumber=1&pageSize=10


[
    {
        "userId": "361404e1-fb96-43fe-81bb-7ce099f41d87",
        "name": "Test1",
        "email": "sample4@gmail.com",
        "orders": [
            {
                "id": "0a20b118-e84c-4b20-aa61-b64e14935435",
                "product": " Mouse",
                "quantity": 2,
                "price": 29.99
            }
        ]
    }
]



## OrderCreation API

Base URL: http://localhost:8082
POST /orders

Create a new order.
-------------------------------------------
Request Body:

{
  "userId": "cb1abd9c-299b-4072-9889-49f0c4cdc195",
  "product": " Mouse",
  "quantity": 2,
  "price": 29.99
}


Response
{
    "success": true,
    "message": "Order created successfully with ID: 0a20b118-e84c-4b20-aa61-b64e14935435.",
    "data": {
        "id": "0a20b118-e84c-4b20-aa61-b64e14935435",
        "userId": "cb1abd9c-299b-4072-9889-49f0c4cdc195",
        "product": " Mouse",
        "quantity": 2,
        "price": 29.99
    }
}


Get Order By Id
----------------------------------------
GET /orders/{id}

Fetch order details by ID.


Get all order
------------------------------------------
GET /orders/all

Parameters (Query):
| Name         | Type | Default | Description                |
| ------------ | ---- | ------- | -------------------------- |
| `pageNumber` | int  | 1       | Page number to retrieve    |
| `pageSize`   | int  | 10      | Number of records per page |

Sample Request:
GET http://localhost:8082/orders/all?pageNumber=1&pageSize=10


Response:

[
    {
        "id": "0a20b118-e84c-4b20-aa61-b64e14935435",
        "userId": "361404e1-fb96-43fe-81bb-7ce099f41d87",
        "product": " Mouse",
        "quantity": 2,
        "price": 29.99
    }
]


## Event Communication

| Event        | Producer      | Consumer      | Kafka Topic     |
| ------------ | ------------- | ------------- | --------------- |
| UserCreated  | UserCreation  | OrderCreation | `user-created`  |
| OrderCreated | OrderCreation | UserCreation  | `order-created` |


Each service includes:

A Kafka producer for publishing domain events.
A Kafka consumer for handling cross-service events.


## Health Checks & Monitoring

Health Endpoints

| Service       | Endpoint   |
| ------------- | ---------- |
| UserCreation  | `http://localhost:8081/health` |
| OrderCreation | `http://localhost:8082/health` |


## Testing
Each service includes .Test projects for:

Unit tests (business logic)
Integration tests (Kafka, controller behavior)

Run all tests with: dotnet test

Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 2 s - Common.Test.dll (net8.0)

Passed!  - Failed:     0, Passed:    22, Skipped:     0, Total:    22, Duration: 30 ms - UserCreation.Test.dll (net8.0)

Passed!  - Failed:     0, Passed:    20, Skipped:     0, Total:    20, Duration: 40 ms - OrderCreation.Test.dll (net8.0)

## Sample Docker Compose Summary

| Container         | Port | Description        |
| ----------------- | ---- | ------------------ |
| zookeeper         | 2181 | Kafka dependency   |
| kafka             | 9092 | Message broker     |
| usercreation-api  | 8081 | User service       |
| ordercreation-api | 8082 | Order service      |



## Example Workflow

1.Create a user:

curl -X POST http://localhost:8081/users \
-H "Content-Type: application/json" \
-d '{"name":"Alice","email":"alice@example.com"}'


2.Create an order for that user:

curl -X POST http://localhost:8082/orders \
-H "Content-Type: application/json" \
-d '{"userId":"<AliceId>","product":"MacBook Pro","quantity":1,"price":2999.99}'


Observe:
UserCreated event in Kafka consumed by OrderCreation
OrderCreated event published from OrderCreation


## Design Decisions

In-memory EF Core for simplicity (no SQL Server required)
Kafka for event-driven communication
Common library for shared contracts and utilities
Health checks
Docker Compose for local orchestration


## AI tools usage
Used ChatGPT to generate:
Format the Readme file
Markdown tables and formatted examples
Manually reviewed and reworded for clarity and tone consistency.
Generate the Class Object based on the give in Object style
A few test case generations, 
Code alignment


## Author
Hari Lakkakula
Senior .NET Developer
📧 [harilakkakula28.com]
📍 Singapore