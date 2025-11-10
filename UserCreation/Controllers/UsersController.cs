using Common.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserCreation.Business.Constants;
using UserCreation.Business.Dto;
using UserCreation.Business.Entities;
using UserCreation.Business.Services.Interface;

namespace UserCreation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] UserCertationDto request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                _logger.LogInformation(AppMessages.UserCreatedSuccess, user.Id);
                return Ok(new ApiResponse<User>(true, AppMessages.UserCreatedResponse, user));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.UnexpectedError);
                return StatusCode(500, new { message = AppMessages.ErrorCreatingUserResponse });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorRetrievingUsers);
                return StatusCode(500, new { message = AppMessages.FailedToRetrieveUsersResponse });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(AppMessages.InvalidId);
            }
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, AppMessages.UserNotFound, id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AppMessages.ErrorGettingUserById, id);
                return StatusCode(500, new { message = AppMessages.FailedToRetrieveUserResponse });
            }
        }
    }
}
