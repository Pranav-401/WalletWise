# WalletWise

WalletWise is an ASP.NET Core MVC personal finance application for
recording and reviewing expenses. It uses Entity Framework Core with SQL
Server for data storage, Auth0 for authentication, and an AI-powered
insights service to help users understand their spending.

> **Project status:** Update this README as the application evolves. In
> particular, replace the configuration placeholders below with the
> exact settings used by your `AIInsightsService` before publishing the
> repository.

## Features

-   **Authentication:** Sign in with Auth0.
-   **Personal expense tracking:** Create, view, edit, and delete
    expenses.
-   **User-specific data:** Expense records are associated with the
    authenticated user's Auth0 identifier (`sub` claim). Queries and
    changes are scoped to that user.
-   **Expense analysis:** View spending summaries and category-based
    chart data.
-   **AI insights:** Generate insights from the current user's expense
    data using the configured AI service.
-   **Automated testing:** xUnit tests cover the expense service,
    including user isolation and CRUD behaviour.
-   **Continuous integration:** GitHub Actions builds the solution and
    runs the test suite on a Windows self-hosted runner.

## Technology stack

-   C# and ASP.NET Core MVC
-   .NET 10
-   Entity Framework Core 10
-   SQL Server
-   Auth0 (`Auth0.AspNetCore.Authentication`)
-   xUnit
-   SQLite in-memory database for service tests
-   GitHub Actions

## Repository structure

The solution is organized into an application project and a separate
test project. The exact files may change as the project grows.

``` text
WalletWise/
├── .github/
│   └── workflows/
│       └── dotnet.yml
├── WalletWise/
│   ├── Controllers/
│   ├── Data/
│   │   └── Service/
│   ├── Models/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   └── WalletWise.csproj
├── WalletWise.Tests/
│   ├── Services/
│   └── WalletWise.Tests.csproj
└── WalletWise.sln
```

## Prerequisites

Install the following before running the application:

-   .NET 10 SDK
-   SQL Server (local, Express, or another accessible SQL Server
    instance)
-   Visual Studio 2026 or another .NET-compatible IDE
-   An Auth0 application configured for the application
-   The AI provider credentials required by your `AIInsightsService` (if
    you want to use AI insights)

Check that the .NET SDK is installed:

``` bash
dotnet --list-sdks
```

## Getting started

### 1. Clone the repository

``` bash
git clone <YOUR_REPOSITORY_URL>
cd WalletWise
```

Open `WalletWise.sln` in Visual Studio, or continue using the .NET CLI
from the directory containing the solution file.

### 2. Configure the database

Add a connection string named `DefaultConnectionString` using User
Secrets or another secure configuration provider.

For example, from the application project directory:

``` bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnectionString" "YOUR_SQL_SERVER_CONNECTION_STRING"
```

Replace the placeholder with your own SQL Server connection string. Do
not commit real connection strings or passwords to source control.

### 3. Configure Auth0

Create or use an Auth0 application and configure its callback and logout
URLs to match the local URL and routes used by this project.

Store the Auth0 settings securely:

``` bash
dotnet user-secrets set "Auth0:Domain" "YOUR_AUTH0_DOMAIN"
dotnet user-secrets set "Auth0:ClientId" "YOUR_AUTH0_CLIENT_ID"
```

The application reads these values from configuration in `Program.cs`.
Do not commit Auth0 credentials or secrets.

### 4. Configure AI insights (optional)

If you want to use the AI insights feature, configure the API key and
any other settings expected by `AIInsightsService` using User Secrets or
environment variables.

The exact configuration key names depend on the current implementation
of `AIInsightsService`. Check that class before setting them. Never
commit API keys to the repository.

### 5. Apply database migrations

From the directory containing the solution file, run:

``` bash
dotnet ef database update --project WalletWise
```

If the Entity Framework Core CLI tool is not installed, install it:

``` bash
dotnet tool install --global dotnet-ef
```

If it is already installed, update it when needed:

``` bash
dotnet tool update --global dotnet-ef
```

### 6. Run the application

``` bash
dotnet restore
dotnet build
dotnet run --project WalletWise
```

Alternatively, select the `WalletWise` project as the startup project in
Visual Studio and run it using the development profile.

## Running tests

The test project is separate from the MVC application. Tests use xUnit
and SQLite in-memory so the expense service can be tested without
connecting to the application's SQL Server database.

Run all tests from the solution directory:

``` bash
dotnet test
```

Run tests with more detailed output:

``` bash
dotnet test --verbosity normal
```

Build the application in Release configuration:

``` bash
dotnet build --configuration Release
```

## Continuous integration

The workflow is located at:

``` text
.github/workflows/dotnet.yml
```

It runs for pushes and pull requests targeting the `master` branch, and
can also be started manually with `workflow_dispatch`.

The workflow:

1.  Checks out the repository.
2.  Displays the installed .NET SDK information.
3.  Restores NuGet dependencies.
4.  Builds the solution in Release configuration.
5.  Runs the xUnit test suite.

The current workflow uses a **self-hosted Windows runner**. The runner
must be online, have access to this repository, and have the .NET 10 SDK
installed. The workflow uses Windows PowerShell (`powershell`) for its
commands.

If your default branch is not `master`, update the branch filters in
`.github/workflows/dotnet.yml`.

## Data isolation and security

-   Each expense is associated with the authenticated user's Auth0
    identifier.
-   Expense retrieval, updates, and deletions are scoped to the current
    user.
-   The application should obtain the user identifier from the
    authenticated claims; it should not trust a `UserId` submitted by a
    browser form.
-   Keep database connection strings, Auth0 credentials, and AI provider
    API keys out of source control.
-   Use HTTPS and keep dependencies and the .NET SDK up to date.

## Development notes

-   Keep automated tests in `WalletWise.Tests`, separate from
    application code.
-   When adding a new service method or changing data access behaviour,
    add or update tests.
-   When changing an entity, update the EF Core model and create a
    migration when the database schema changes.
-   Review the GitHub Actions run after pushing changes to verify that
    the solution builds and tests pass.

## Troubleshooting

### The GitHub Actions job stays at "Waiting for a runner"

Check that the self-hosted runner is online, is registered with the
correct repository or an authorized runner group, and has the labels
requested by the workflow. Keep the runner process or service running.

### `pwsh: command not found`

The workflow can use Windows PowerShell by setting:

``` yaml
shell: powershell
```

This requires `powershell.exe` to be available on the Windows runner.

### Database connection errors

Verify that SQL Server is running, the connection string is correct, and
the database user has the required permissions. Confirm that the
migrations have been applied.

### Auth0 sign-in errors

Check the Auth0 domain and client ID, and verify that the local callback
and logout URLs in the Auth0 application settings match the URLs used by
WalletWise.
