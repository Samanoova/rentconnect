using Microsoft.AspNetCore.Mvc;
using RentConnect.Data.Dtos;
using RentConnect.Data.Models;
using RentConnect.Data.UnitOfWork;

namespace RentConnect.Api.Controllers;

[ApiController]
[Route("api/listings")]
public class ListingsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ListingsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? region, [FromQuery] ListingStatus? status)
    {
        var listings = await _unitOfWork.Listings.GetAllAsync(region, status);
        return Ok(listings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var listing = await _unitOfWork.Listings.GetByIdAsync(id);
        if (listing is null) return NotFound();
        return Ok(listing);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ListingCreateDto dto)
    {
        var created = await _unitOfWork.Listings.AddAsync(dto);
        await _unitOfWork.CompleteAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ListingUpdateDto dto)
    {
        var success = await _unitOfWork.Listings.UpdateAsync(id, dto);
        if (!success) return NotFound();
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }

    // الزر السريع "متوفر / متأجّر / معلّق" من لوحة التحكم
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ListingStatusUpdateDto dto)
    {
        var listing = await _unitOfWork.Listings.UpdateStatusAsync(id, dto.NewStatus);
        if (listing is null) return NotFound();
        await _unitOfWork.CompleteAsync();
        return Ok(listing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _unitOfWork.Listings.DeleteAsync(id);
        if (!success) return NotFound();
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }

    [HttpGet("stale")]
    public async Task<IActionResult> GetStaleListings([FromQuery] int days = 7)
    {
        var stale = await _unitOfWork.Listings.GetStaleAsync(days);
        return Ok(stale);
    }
}
