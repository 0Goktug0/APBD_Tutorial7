using Microsoft.AspNetCore.Mvc;
using Tutorial6.Dto;
using Tutorial6.Services;

namespace Tutorial6.Controllers
{
    [Route("api/warehouses")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public WarehousesController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }


        [HttpPost]
        public IActionResult AddProduct([FromBody] Warehouse warehouseBody)
        {
            if (_databaseService.CheckData(warehouseBody.IdProduct, warehouseBody.IdWarehouse, warehouseBody.Amount))
            {
                if (_databaseService.CheckOrder(warehouseBody.IdProduct, warehouseBody.Amount, warehouseBody.CreatedAt))
                {
                    if (_databaseService.IfCompleted(warehouseBody.IdProduct))
                    {
                        _databaseService.UpdateFullfill(warehouseBody.IdProduct);
                        _databaseService.RegisterProduct(warehouseBody.IdProduct, warehouseBody.IdWarehouse, warehouseBody.Amount, warehouseBody.CreatedAt);
                        return Ok();
                    }
                    return NotFound("The order has already been completed.");
                }
                return NotFound("There is no suitable order.");
            }
            return NotFound("The product/warehouse with the given id does not exist.");
        }



        [HttpPost("AddProductUsingSP")]
        public IActionResult AddProductUsingSP([FromBody] Warehouse warehouseBody)
        {
            try
            {
                if (warehouseBody == null)
                {
                    return BadRequest("Invalid data");
                }

                var result = _databaseService.AddProductUsingStoredProc(warehouseBody);

                if (result == "Completed")
                {
                    return Ok();
                }
                else
                {
                    switch (result)
                    {
                        case "NotFound":
                            return NotFound("The specified resource was not found.");
                        case "InvalidData":
                            return BadRequest("The provided data is not valid.");
                        case "Unauthorized":
                            return Unauthorized("You are not authorized to perform this action.");
                        default:
                            return BadRequest(result);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
