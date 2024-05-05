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
        
        if (request.Amount <= 0)
            return BadRequest("Wartość ilości przekazanej w żądaniu musi być większa niż 0.");
        
        int productCount, warehouseCount, orderCount, fulfilledCount, orderId;
        
        string checkProductQuery = "SELECT COUNT(1) FROM Product WHERE IdProduct = @IdProduct";
        
        using (SqlCommand command = new SqlCommand(checkProductQuery, connection))
        {
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            productCount = (int)command.ExecuteScalar();
            if (productCount == 0)
                return NotFound("Produkt o podanym identyfikatorze nie istnieje.");
        }
        
        string checkWarehouseQuery = "SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";

        using (SqlCommand command = new SqlCommand(checkWarehouseQuery, connection))
        {
            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            warehouseCount = (int)command.ExecuteScalar();
            if (warehouseCount == 0)
                return NotFound("Magazyn o podanym identyfikatorze nie istnieje.");
        }
        
        string checkOrderQuery = "SELECT COUNT(1) FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < GETDATE()";
        
        using (SqlCommand command = new SqlCommand(checkOrderQuery, connection))
        {
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            orderCount = (int)command.ExecuteScalar();
            if (orderCount == 0)
                return BadRequest("Brak odpowiedniego zamówienia dla tego produktu.");
        }
              
        string checkIdOrder = "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount";
        
        using (SqlCommand command = new SqlCommand(checkIdOrder, connection))
        {
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            orderId = (int)command.ExecuteScalar();
        }
        
        string checkWarsehouseProduct = "SELECT COUNT(1) FROM Product_Warehouse WHERE IdOrder = @IdOrder";

        using (SqlCommand command = new SqlCommand(checkWarsehouseProduct, connection))
        {
            command.Parameters.AddWithValue("@IdOrder", orderId);
            fulfilledCount = (int)command.ExecuteScalar();
            if (fulfilledCount > 0)
                return BadRequest("To zamówienie zostało już zrealizowane.");
        }
        
        string updateOrderQuery = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @IdOrder";

        using (SqlCommand command = new SqlCommand(updateOrderQuery, connection))
        {
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.ExecuteNonQuery();
        }
        
        string insertProductWarehouseQuery = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, GETDATE());";

        using (SqlCommand command = new SqlCommand(insertProductWarehouseQuery, connection))
        {
            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            
            string getProductPriceQuery = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            using (SqlCommand priceCommand = new SqlCommand(getProductPriceQuery, connection))
            {
                priceCommand.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                decimal price = (decimal)priceCommand.ExecuteScalar();
                command.Parameters.AddWithValue("@Price", price * request.Amount);
            }
            command.Parameters.AddWithValue("CreatedAt", DateTime.Now);

            int productWarehouseId = Convert.ToInt32(command.ExecuteScalar());
            return Ok(productWarehouseId);
        }
    }
}
    
    
