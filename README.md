# MovieGO

## Instrukcja uruchamiania projektu

### Wymagania systemowe

- **System operacyjny**: Windows 10/11 lub Linux z obsługą .NET 8.
- **.NET SDK**: Wersja 8.0.
- **Serwer bazy danych**: MS SQL Server (preferowany) lub SQLite (dostosowanie projektu do SQLite wymaga dodatkowej konfiguracji).
- **IDE**: Visual Studio 2022 (lub nowszy) z:
  - ASP.NET
  - Entity Framework Core.

---

## Ścieżki pracy z projektem

### Opcja 1: Nowa baza danych

1. Otwórz kod źródłowy projektu w Visual Studio.
2. Otwórz **Package Manager Console** w Visual Studio
   - W menu wybierz **Tools** > **NuGet Package Manager** > **Package Manager Console**.
4. Wykonaj polecenie, aby zastosować migracje i utworzyć nową bazę danych:
   ```bash
   Update-Database
   
5. Uruchom aplikację za pomocą terminala:
   ```bash
   dotnet run
   ```
   - lub bezpośrednio w Visual Studio, klikając przycisk **Uruchom**.

6. Domyślnie aplikacja zostanie uruchomiona pod jednym z poniższych adresów:
   - `https://localhost:7291`
   - `http://localhost:5132`.

7. Otwórz wybrany adres w przeglądarce, aby rozpocząć korzystanie z aplikacji.

---

### Opcja 2: Gotowa baza danych

1. Do projektu dołączona jest wstępnie wypełniona baza danych. Znajdziesz ją w plikach projektu: **MovieGO\Data\MovieGO_DB_TEST.mdf**

2. Aby podłączyć bazę danych:
- Otwórz **SQL Server Object Explorer** w Visual Studio.
  ![image](https://github.com/user-attachments/assets/d308986d-b495-44c3-a068-a2fdf3f44e30)

- Wykonaj poniższe zapytanie:
  ```sql
  CREATE DATABASE MovieGO_Test
  ON (FILENAME = '<SCIEŻKA_DO_PROJEKTU>\MovieGO\Data\MovieGO_DB_TEST.mdf')
  FOR ATTACH;
  ```
  
3. Gdzie "<SCIEŻKA_DO_PROJEKTU>" wskaż lokalizację pliku `.mdf` z kopią bazy danych znajdującego się w folderze projektu: **MovieGO\Data\MovieGO_DB_TEST.mdf**

4. Przejdź do pliku `appsettings.json` i zmień connection string na ten z odkomentowanym parametrem `DefaultConnection`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Data\\MovieGO_DB_TEST.mdf;Integrated Security=True;MultipleActiveResultSets=True"
}
```

![image](https://github.com/user-attachments/assets/0c23b006-a593-4f77-8870-b4b6091bb3c4)

# MovieGO

## Konto administratora

Aplikacja automatycznie tworzy konto administratora oraz role użytkowników podczas pierwszego uruchomienia. Dane logowania do konta administratora to:

- **Login**: `admin@admin.com`
- **Hasło**: `Admin123#`

Kod odpowiedzialny za utworzenie konta administratora znajduje się w pliku `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var roles = new[] { "Administrator", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@admin.com";
    var adminPassword = "Admin123#";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
        else
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            foreach (var error in result.Errors)
            {
                logger.LogError($"Nie udało się utworzyć konta: {error.Description}");
            }
        }
    }
}
