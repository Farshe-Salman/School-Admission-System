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

        if (hold.Status == HoldStatus.Expired)
        {
            return Conflict(new
            {
                title = "Reservation Expired",
                message = "Your reservation has expired. Please create a new application."
            });
        }

        if (hold.Status == HoldStatus.Cancelled)
        {
            return Conflict(new
            {
                title = "Reservation Cancelled",
                message = "This reservation has already been cancelled."
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
        var status = await _holdService.CancelHoldAsync(
            holdId,
            studentId,
            cancellationToken);

        return status switch
        {
            HoldStatus.Cancelled => NoContent(),

            HoldStatus.Expired => Conflict(new
            {
                title = "Reservation Expired",
                message = "This reservation has expired and cannot be cancelled."
            }),

            HoldStatus.Confirmed => Conflict(new
            {
                title = "Reservation Already Confirmed",
                message = "This reservation has already been confirmed and cannot be cancelled."
            }),

            _ => Conflict(new
            {
                title = "Cancellation Failed",
                message = "This reservation cannot be cancelled."
            })
        };
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