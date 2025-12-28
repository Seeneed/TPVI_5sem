using ASPA0010_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResultsAuthenticate;
using ResultsCollection;
using System.Linq;
using System.Threading.Tasks;

namespace ASPA0010_1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly IResultsService _resultsService;
        private readonly IAuthenticateService _authenticateService;

        public ResultsController(
            IResultsService resultsService,
            IAuthenticateService authenticateService)
        {
            _resultsService = resultsService;
            _authenticateService = authenticateService;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authenticateService.RegisterAsync(model.Login, model.Password, model.Role);
            if (result.Succeeded) return Ok("User registered successfully.");
            return BadRequest(result.Errors);
        }

        [AllowAnonymous]
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest("Login and Password are required.");
            var success = await _authenticateService.SignInAsync(model.Login, model.Password);
            if (success) return Ok("Sign-in successful.");
            return NotFound("Invalid login or password.");
        }

        [HttpGet("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await _authenticateService.SignOutAsync();
            return Ok("Sign-out successful.");
        }

        [HttpGet, Authorize(Roles = "READER,WRITER")]
        public async Task<IActionResult> GetResults()
        {
            var results = await _resultsService.GetResultsAsync();
            return !results.Any() ? NoContent() : Ok(results);
        }

        [HttpGet("{k:int}"), Authorize(Roles = "READER,WRITER")]
        public async Task<IActionResult> GetResult(int k)
        {
            var result = await _resultsService.GetResultAsync(k);
            return result.Key == 0 ? NotFound() : Ok(result);
        }

        [HttpPost, Authorize(Roles = "WRITER")]
        public async Task<IActionResult> PostResult([FromBody] ResultValue resultValue)
        {
            if (string.IsNullOrEmpty(resultValue?.Value)) return BadRequest();
            var newResult = await _resultsService.AddResultAsync(resultValue.Value);
            return CreatedAtAction(nameof(GetResult), new { k = newResult.Key }, newResult);
        }

        [HttpPut("{k:int}"), Authorize(Roles = "WRITER")]
        public async Task<IActionResult> PutResult(int k, [FromBody] ResultValue resultValue)
        {
            if (string.IsNullOrEmpty(resultValue?.Value)) return BadRequest();
            var updatedResult = await _resultsService.UpdateResultAsync(k, resultValue.Value);
            return updatedResult.Key == 0 ? NotFound() : Ok(updatedResult);
        }

        [HttpDelete("{k:int}"), Authorize(Roles = "WRITER")]
        public async Task<IActionResult> DeleteResult(int k)
        {
            var deletedResult = await _resultsService.DeleteResultAsync(k);
            return deletedResult.Key == 0 ? NotFound() : Ok(deletedResult);
        }
    }
}