using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.API.DTOs.Responses;
using SchoolAdmission.Business.Interfaces.Services;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("holds")]
public class HoldsController : ControllerBase
{
    private readonly IAdmissionHoldService _holdService;
    private readonly IMapper _mapper;

    public HoldsController(
        IAdmissionHoldService holdService,
        IMapper mapper)
    {
        _holdService = holdService;
        _mapper = mapper;
    }

    // GET: /holds/{holdId}
    [HttpGet("{holdId:guid}")]
    public async Task<ActionResult<CreateHoldResponse>> GetHoldById(
        Guid holdId,
        CancellationToken cancellationToken)
    {
        var hold = await _holdService.GetHoldByIdAsync(
            holdId,
            cancellationToken);

        if (hold is null)
        {
            return NotFound(new
            {
                message = "Hold not found."
            });
        }

        var response = _mapper.Map<CreateHoldResponse>(hold);

        return Ok(response);
    }

    // POST: /holds/{holdId}/confirm
    [HttpPost("{holdId:guid}/confirm")]
    public async Task<IActionResult> ConfirmHold(
        Guid holdId,
        [FromHeader(Name = "X-Student-Id")] string studentId,
        CancellationToken cancellationToken)
    {
        var hold = await _holdService.ConfirmHoldAsync(
            holdId,
            studentId,
            cancellationToken);

        if (hold.Status != HoldStatus.Confirmed)
        {
            return Conflict(new
            {
                message = "This hold cannot be confirmed."
            });
        }

        return Ok(new
        {
            holdId = hold.Id,
            status = hold.Status.ToString().ToUpperInvariant()
        });
    }

    // DELETE: /holds/{holdId}
    [HttpDelete("{holdId:guid}")]
    public async Task<IActionResult> CancelHold(
        Guid holdId,
        [FromHeader(Name = "X-Student-Id")] string studentId,
        CancellationToken cancellationToken)
    {
        await _holdService.CancelHoldAsync(
            holdId,
            studentId,
            cancellationToken);

        return NoContent();
    }

    // GET: /me/holds
    [HttpGet("/me/holds")]
    public async Task<IActionResult> GetMyHolds(
        [FromHeader(Name = "X-Student-Id")] string studentId,
        CancellationToken cancellationToken)
    {
        var holds = await _holdService.GetStudentHoldsAsync(
            studentId,
            cancellationToken);

        var response = _mapper.Map<List<CreateHoldResponse>>(holds);

        return Ok(response);
    }


}