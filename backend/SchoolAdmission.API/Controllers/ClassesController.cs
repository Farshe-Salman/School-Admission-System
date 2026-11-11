
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.API.DTOs.Requests;
using SchoolAdmission.API.DTOs.Responses;
using SchoolAdmission.Business.Interfaces.Services;
using SchoolAdmission.Business.Models;
using AutoMapper;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    private readonly IAdmissionClassService _classService;
    private readonly IAdmissionHoldService _holdService;
    private readonly IMapper _mapper;

    public ClassesController(
    IAdmissionClassService classService,
    IAdmissionHoldService holdService,
    IMapper mapper)
    {
        _classService = classService;
        _holdService = holdService;
        _mapper = mapper;
    }

    // POST: /classes
    [HttpPost]
    public async Task<ActionResult<CreateClassResponse>> CreateClass(
        [FromBody] CreateClassRequest request,
        CancellationToken cancellationToken)
    {
        var admissionClass = await _classService.CreateClassAsync(
            request.Name,
            request.TotalSeats,
            cancellationToken);

        var response = _mapper.Map<CreateClassResponse>(admissionClass);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    // GET: /classes
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClassResponse>>> GetAllClasses(
        CancellationToken cancellationToken)
    {
        var classes = await _classService.GetAllClassesAsync(
            cancellationToken);

        var response = _mapper.Map<List<ClassResponse>>(classes);

        return Ok(response);
    }

    // GET: /classes/{classId}
    [HttpGet("{classId:guid}")]
    public async Task<ActionResult<ClassResponse>> GetClassById(
        Guid classId,
        CancellationToken cancellationToken)
    {
        var admissionClass = await _classService.GetClassByIdAsync(
            classId,
            cancellationToken);

        if (admissionClass is null)
            return NotFound(new
            {
                message = "Admission class not found."
            });

        return Ok(_mapper.Map<ClassResponse>(admissionClass));
    }


    // POST: /classes/{classId}/holds
    [HttpPost("{classId:guid}/holds")]
    public async Task<ActionResult<CreateHoldResponse>> CreateHold(
        Guid classId,
        [FromHeader(Name = "X-Student-Id")] string studentId,
        CancellationToken cancellationToken)
    {
        var hold = await _holdService.CreateHoldAsync(
            classId,
            studentId,
            cancellationToken);

        var response = _mapper.Map<CreateHoldResponse>(hold);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }
}
