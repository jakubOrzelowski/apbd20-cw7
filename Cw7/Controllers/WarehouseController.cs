using System.Data;
using Cw7.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Cw7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public WarehouseController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult AddProductToWarehouse(WarehouseRequest request)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        SqlCommand command = new SqlCommand();
        command.Connection = connection;
        
        command.ExecuteNonQuery();
    }
    
    
}