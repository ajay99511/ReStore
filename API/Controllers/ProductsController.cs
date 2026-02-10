using System.Text.Json;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.RequestHelpers;
using ClosedXML.Excel;
using Microsoft.Data.Sqlite; // for SQLite
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProductsController(StoreContext _context,IUnitOfWork unitOfWork, IConfiguration configuration):BaseApiController
    {


        [HttpGet]
        public async Task<ActionResult<PagedList<Product>>> GetProducts([FromQuery]ProductParams productParams)
        {
            var products = await unitOfWork.ProductRepository.GetProductsAll(productParams);
            // Response.Headers.Append("Pagination",JsonSerializer.Serialize(products.metaData));
            Response.AddPaginationHeader(products.metaData);
            return Ok(products);
        } 

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id )
        {
            // var product = await _context.Products.FindAsync(id);
            // if(product == null) return NotFound();
            // return product;
            var product = await unitOfWork.ProductRepository.GetProductById(id);
            if(product == null) return NotFound();
            return Ok(product);
        }
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var brands = await _context.Products.Select(p=>p.Brand).Distinct().Take(50).ToListAsync();
            var types = await _context.Products.Select(p=>p.Type).Distinct().ToListAsync();
            return Ok(new {brands,types});
            // return await unitOfWork.ProductRepository.GetFilters();
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

    }
}


        // private readonly StoreContext _context;
        // private readonly IUnitOfWork _unitOfWork;
        // public ProductsController (StoreContext context,IUnitOfWork unitOfWork)
        // {
        //     _context = context;
        //     _unitOfWork = unitOfWork;
        // }


//  public async Task<ActionResult<PagedList<Product>>> GetProducts([FromQuery]ProductParams productParams)
//         {
//             var query = _context.Products
//             .Sort(productParams.OrderBy)
//             .Search(productParams.SearchTerm)
//             .Filter(productParams.Brands, productParams.Types)
//             .AsQueryable();
//             var products = await PagedList<Product>.ToPagedList(query,productParams.PageNumber,productParams.PageSize);
//             // Response.Headers.Append("Pagination",JsonSerializer.Serialize(products.metaData));
//             Response.AddPaginationHeader(products.metaData);
//             return Ok(products);
//         } 
