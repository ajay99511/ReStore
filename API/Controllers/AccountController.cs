using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using ClosedXML.Excel;
using Microsoft.Data.Sqlite; // for SQLite
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Controllers
{
    public class AccountController(UserManager<User> userManager, 
    TokenService tokenService ,IUnitOfWork unitOfWork,RoleManager<Role> roleManager, IConfiguration configuration) : BaseApiController
    {

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.Username);
            if(user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized();
            }
            var userBasket = await unitOfWork.BasketRepository.retrieveBasket(loginDto.Username);
            var anonBasket = await unitOfWork.BasketRepository.retrieveBasket(Request.Cookies["buyerId"]);
            if(userBasket == null && anonBasket == null)
            {
               userBasket = CreateBasket();
               userBasket.BuyerId = user.UserName;
            }
            if(anonBasket != null)
            {
                if(userBasket != null)
                {
                    unitOfWork.BasketRepository.DeleteBasket(userBasket);
                    // Response.Cookies.Delete("buyerId");
                    // await unitOfWork.complete();
                }
                anonBasket.BuyerId = user.UserName;
                Response.Cookies.Delete("buyerId");
            }
            await unitOfWork.complete();
            var token = await tokenService.CreateToken(user);
            return new UserDto 
            {
                Username = user.UserName,
                Email = user.Email,
                Token = token,
                basket = anonBasket != null ? unitOfWork.BasketRepository.ConvertBasketDto(anonBasket) : 
                unitOfWork.BasketRepository.ConvertBasketDto(userBasket),
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
             var user = new User{UserName=registerDto.Username, Email=registerDto.Email};
             var result = await userManager.CreateAsync(user,registerDto.Password);
             if(!result.Succeeded)
             {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code,error.Description);
                }
                return ValidationProblem();
             }
             await userManager.AddToRoleAsync(user,"Member");
             return StatusCode(201);
        }

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var username = User.Identity.Name;
            var user = await userManager.FindByNameAsync(username);
            var Basket = await unitOfWork.BasketRepository.retrieveBasket(username);
            // if(Basket == null)
            // {
            // Basket = Create(username);
            // await unitOfWork.complete();
            // }
            return new UserDto{
                Email = user.Email,
                Username = username,
                Token = await tokenService.CreateToken(user),
                basket = Basket!=null?unitOfWork.BasketRepository.ConvertBasketDto(Basket):null,
            };

        }

        [Authorize(Policy="RequireAdmin")]
        [HttpPost("create-role")]
        public async Task<ActionResult> CreateRole(string role)
        {
            // var username = User.Identity.Name;
            // var user = await userManager.Users.FirstAsync(x=>x.UserName == username);
            // var roles = await userManager.GetRolesAsync(user);
            // if(roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))

            var roleExists = await roleManager.RoleExistsAsync(role);
            if(roleExists)
            {
                return BadRequest(new ProblemDetails{Title="Role Already exists"});
            }
            await roleManager.CreateAsync(new Role{Name=role});
            return Created();
        }

        [Authorize(Policy="RequireAdmin")]
        [HttpDelete("delete-role")]
        public async Task<ActionResult> DeleteRole(string rolename)
        {
            var role = await roleManager.FindByNameAsync(rolename);
            if(role == null)
            {
                return NotFound();
            }
            if(role.Name == "Admin" || role.Name == "Member")
            {
                return BadRequest(new ProblemDetails{ Title="Admin or Member cannot be deleted"});
            }
            await roleManager.DeleteAsync(role);
            return Ok();
        }

        [Authorize]
        [HttpGet("savedAddress")]
        public async Task<ActionResult<UserAddress>> GetSavedAddress()
        {
            return await userManager.Users
            .Where(u=>u.UserName == User.Identity.Name)
            .Select(u=>u.userAddress)
            .FirstOrDefaultAsync();
        }

        [HttpGet("exportcsv")]
        public async Task<ActionResult> Exportcsv ()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Use SqliteConnection instead of SqlClient.SqlConnection
            await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            await conn.OpenAsync();
            try
            {
                // var query = "SELECT TOP 10 Id, Name, Description, Price, PictureUrl, Type, Brand, QuantityInStock FROM Products ORDER BY Id DESC";
                var query = "SELECT Id, Name, Description, Price, PictureUrl, Type, Brand, QuantityInStock FROM Dummydata";
                
                // Use SqliteCommand instead of SqlClient.SqlCommand
                await using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                // Write headers
                var headerLine = string.Join(",", Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)));
                await writer.WriteLineAsync(headerLine);

                // Write data rows
                while (await reader.ReadAsync())
                {
                    var row = string.Join(",", Enumerable.Range(0, reader.FieldCount).Select(i =>
                        reader.IsDBNull(i) ? "" : reader.GetValue(i)?.ToString()));
                    await writer.WriteLineAsync(row);
                }
            }

            stream.Position = 0;

            // Set Content-Disposition header to force download
            Response.Headers.Add("Content-Disposition", "attachment; filename=products.csv");

            return File(stream.ToArray(), "text/csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }
        
        [HttpGet("export-excel")]
        public async Task<ActionResult> Exportexcel()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Use SqliteConnection instead of SqlClient.SqlConnection
            await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            await conn.OpenAsync();
            try
            {
                // var query = "SELECT TOP 10 Id, Name, Description, Price, PictureUrl, Type, Brand, QuantityInStock FROM Products ORDER BY Id DESC";
                var query = "SELECT Id, Name, Description, Price, PictureUrl, Type, Brand, QuantityInStock FROM Dummydata";
                
                // Use SqliteCommand instead of SqlClient.SqlCommand
                await using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Products");

                int rowIndex = 1;

                // Write headers and first row of data
                if (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = reader.GetName(i); // Column headers
                        worksheet.Cell(2, i + 1).Value = reader.IsDBNull(i) ? "" : reader.GetValue(i)?.ToString(); // First row of data
                    }
                    rowIndex = 3;
                }

                // Write remaining rows
                while (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        worksheet.Cell(rowIndex, i + 1).Value = reader.IsDBNull(i) ? "" : reader.GetValue(i)?.ToString();
                    }
                    rowIndex++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                Response.Headers.Add("Content-Disposition", "attachment; filename=products.xlsx");

                return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }




        private Basket CreateBasket()
        {
            string buyerId = User.Identity.Name;
            // var buyerId = User.Identity.Name;
            if(string.IsNullOrEmpty(buyerId))
            {
                buyerId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions{IsEssential=true,Expires=DateTime.Now.AddDays(30)};
                Response.Cookies.Append("buyerId",buyerId,cookieOptions);
            }
            Basket basket = new Basket
            {
                BuyerId =buyerId,
            };
            // _context.Baskets.Add(basket);
            unitOfWork.BasketRepository.AddBasket(basket);
            return basket;
        }

       
        // [HttpPost("registerProducts")]
        // public async Task<ActionResult> RegisterProducts()
        // {
        //     var ProductData = await System.IO.File.ReadAllTextAsync("Data/JsonData/productData.json");
        //     var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
        //     var products = JsonSerializer.Deserialize<List<Product>>(ProductData,options);
        //     foreach (var product in products)
        //     {
        //     unitOfWork.ProductRepository.AddProduct(product);
        //     }
        //     await unitOfWork.complete();
        //     return Ok(products);
        // }
    }
}






//         public class StringToDecimalConverter : JsonConverter<decimal>
// {
//     public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//     {
//         if (reader.TokenType == JsonTokenType.String && decimal.TryParse(reader.GetString(), out var value))
//         {
//             return value;
//         }
//         return reader.GetDecimal();
//     }

//     public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
//     {
//         writer.WriteNumberValue(value);
//     }
// }
// var options = new JsonSerializerOptions
// {
//     Converters =
//     {
//         new StringToDecimalConverter()
//     }
// };

// var result = JsonSerializer.Deserialize<List<YourModel>>(jsonString, options);
