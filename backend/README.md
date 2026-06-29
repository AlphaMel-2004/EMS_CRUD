# Employee management setup

This API now backs an employee management system with PostgreSQL, JWT login, and role-based access control. The app applies migrations on startup when it runs in Development, so the tables are created automatically after the database is available.

## Start PostgreSQL locally

From the `backend` folder:

```powershell
docker compose up -d db pgadmin
```

The database settings used by the API are:

- Host: `localhost`
- Port: `5432`
- Database: `crud_learning_db`
- User: `postgres`
- Password: `postgres`

## Open pgAdmin

After the containers start, open:

```text
http://localhost:5050
```

Log in with:

- Email: `admin@crudlearning.local`
- Password: `admin`

To register the PostgreSQL server in pgAdmin, add a new server with:

This project already preloads that server for you, so it should appear automatically in pgAdmin as `Crud Learning PostgreSQL`.

If you want to confirm the settings manually, the connection uses:

- Host name/address: `db`
- Port: `5432`
- Maintenance database: `crud_learning_db`
- Username: `postgres`
- Password: `postgres`

## Run the API

From `backend/CrudLearning.Api`:

```powershell
dotnet run
```

If the database is running, EF Core will apply the migration and create the employee, user, and attendance tables.

In Development, the API resets the local database on startup to recover cleanly if the schema gets out of sync. That is controlled by `ResetDatabaseOnStartup=true` in launch settings.

## Default logins

Use these seeded accounts with the frontend:

- Admin: `admin` / `Admin123!`
- Employee: `employee` / `Employee123!`

## API highlights

- Admin can create, update, soft delete, and view employees.
- Employee can update their own status and check in or check out.
- Password hashing uses BCrypt with 12 rounds.

## See the tables in PostgreSQL

### Option 1: Use `psql`

Open a shell inside the container:

```powershell
docker compose exec db psql -U postgres -d crud_learning_db
```

Inside `psql`, use these commands:

```sql
\dt
\d "Students"
SELECT * FROM "Students";
```

### Option 2: Use a GUI

Use pgAdmin, DBeaver, or TablePlus using:

- Host: `localhost`
- Port: `5432`
- Database: `crud_learning_db`
- Username: `postgres`
- Password: `postgres`

Then open the `Employees`, `Users`, or `AttendanceEntries` tables from the schema browser or run `SELECT * FROM "Employees";`.

## Open the web app locally

From `frontend/crud-client`:

```powershell
npm install
npm run dev
```

Then open:

```text
http://localhost:5173
```

If the login page loads but sign-in fails, check that the API is running on `http://localhost:5299` and that PostgreSQL is available.