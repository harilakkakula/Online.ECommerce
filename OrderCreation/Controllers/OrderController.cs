using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderCreation.Business.Dto;
using OrderCreation.Business.Entities;
using OrderCreation.Business.Services.Interface;
using OrderCreation.Business.Constants;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.General;

namespace OrderCreation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {

        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }


        /// <summary>
        /// Creates a new order.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Order), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreationDto request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning(AppMessages.InvalidOrderRequest);
                    return BadRequest(AppMessages.InvalidOrderRequest);
                }

                var order = await _orderService.CreateOrderAsync(request);
                _logger.LogInformation(AppMessages.OrderCreatedSuccess, order.Id);
                return Ok(new ApiResponse<Order>(true, string.Format(AppMessages.OrderCreatedSuccess,order.Id), order));
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, AppMessages.OrderValidationFailed);
                return BadRequest(string.Format(AppMessages.OrderValidationFailedWithDetails, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, string.Format(AppMessages.UserNotFound,request.UserId));
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.UnexpectedErrorCreatingOrder);
                return StatusCode((int)HttpStatusCode.InternalServerError, AppMessages.UnexpectedErrorCreatingOrder);
            }
        }

        /// <summary>
        /// Retrieves all orders with pagination.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<Order>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
                _logger.LogInformation(AppMessages.OrdersRetrievedSuccess);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorRetrievingOrders);
                return StatusCode((int)HttpStatusCode.InternalServerError, AppMessages.ErrorRetrievingOrders);
            }
        }

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Order), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            if(id==Guid.Empty)
            {
                return BadRequest(AppMessages.InvalidId);
            }
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning(AppMessages.OrderNotFound, id);
                    return NotFound(string.Format(AppMessages.OrderNotFound, id));
                }

                _logger.LogInformation(AppMessages.OrderRetrievedSuccess, id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, AppMessages.OrderNotFound, id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorGettingOrderById, id);
                return StatusCode((int)HttpStatusCode.InternalServerError, AppMessages.ErrorGettingOrderById);
            }
        }

    }
}
