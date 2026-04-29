# Orders Microservice — eCommerce Solution

Part of a **3-service microservices architecture** built with **ASP.NET Core**, demonstrating clean architecture, polyglot persistence, and resilient inter-service communication.

## 🏗️ Architecture Overview

This Orders Microservice is one of three services in a distributed eCommerce system:

| Service | Responsibility | Database |
|---------|---------------|----------|
| UsersService | User management — JWT & registration *(coming soon)* | PostgreSQL |
| ProductsService | Product catalog, search, inventory | MySQL |
| **OrdersService** (this repo) | Order processing, validation, aggregation | MongoDB |

Services communicate via **HTTP clients** with **Polly-based fault tolerance** (retry, circuit breaker, timeout, fallback).

## 🛠️ Tech Stack

**Backend**
- ASP.NET Core — Web API with Controllers
- MongoDB.Driver — native MongoDB client
- AutoMapper — DTO ↔ Entity mapping
- FluentValidation — nested validation (Order + OrderItems)
- Polly — Retry, Circuit Breaker, Timeout, Fallback policies

**Infrastructure**
- Docker & Docker Compose — multi-container orchestration  
- MongoDB — NoSQL document database
- Ocelot API Gateway
- Polyglot persistence (MongoDB, MySQL, PostgreSQL)

**Frontend** *(in development)*
- Angular — UI for orders, products, users

## 📐 Project Structure (Clean Architecture)
├── ApiGateway/                      # Ocelot gateway routing requests to services  
├── BusinessLogicLayer/              # Services, DTOs, Mappers, Validators, Policies  
├── DataAccessLayer/                 # Repositories, Entities, MongoDB context  
├── ECommerceSolution.OrderService/  # API layer, Controllers, Program.cs, Dockerfile  
├── docker-compose.yml               # Multi-container orchestration  
└── docker-compose.override.yml      # Development overrides  

## 🚀 Getting Started

Detailed setup instructions and prerequisites will follow soon. 🛠️

Project is actively under development — stay tuned.

## ✨ Key Features

- ✅ **Microservices architecture** with HTTP-based communication
- ✅ **Cross-service validation** (Orders validates against Users & Products services)
- ✅ **Fault tolerance** via Polly (exponential backoff retry, circuit breaker, fallback)
- ✅ **Polyglot persistence** — different database per service
- ✅ **API Gateway** for unified entry point
- ✅ **Clean Architecture** — strict layer separation
- ✅ **FluentValidation** with nested validation rules
- ✅ **AutoMapper profiles** for clean DTO mappings

## 🔮 Roadmap

- [ ] JWT authentication & user registration in UsersService
- [ ] RabbitMQ + MassTransit for event-driven communication
- [ ] Unit tests (xUnit + Moq)
- [ ] Angular frontend (in development)
- [ ] Azure deployment

## Learning Project

Built while working through ".NET Microservices with Azure DevOps & AKS | Basic to Master"(https://www.udemy.com/course/dot-net-microservices-ecommerce-project-azure-devops-kubernetes-aks/learn/lecture/45853823?start=1#overview) by Harsha Vardhan on Udemy.

## 👨‍💻 Author

**Akshat Parasher** — Software Engineer | C#/.NET Developer | Germany 🇩🇪

- 🔗 [Portfolio](https://akshat95-portfolio.netlify.app)
- 🔗 [GitHub](https://github.com/AkshatAspNetCore)
- 🔗 [GitLab](https://gitlab.com/arkhamknight95-group)
