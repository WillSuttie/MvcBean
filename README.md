# AllTheBeans

This ASP.NET Core Web App (MVC) was created to complete the requirements of the Coffee Beans coding challenge.

## Design Decisions

Used standard MVC web app with Identity to allow for user authentication.
The app uses Entity Framework Core to interact with the database and manage the beans. 
The CRUD pages to manage the beans are scaffolded from the EF model
The app is created inside a single project as it is a small project and the separation of concerns is not a priority as it won't be extended or maintained longterm.

## Requirements

- Visual studio
- .NET 9.0 SDK
- Sql Server

## How to run

- open project in Visual Studio
- point connection string in appSettings.json to local instance of sql Server*

*For the first time running the software you should create a beans database by running the sql command `create database beans` once this is done go to package manager console in visual studio and run `Update-Database` to run the EF migrations.



"WHAT DOES A BEAN MEAN?"
             - Kevin Malone



API URL - https://localhost:7208/api/dataapi/BeanOfTheDay
