using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize(Roles = "Admin")]
public class RoomsController : Controller
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllAsync();
        return View(rooms);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var room = await _roomService.GetByIdAsync(id.Value);
        if (room == null) return NotFound();
        return View(room);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoomDto dto)
    {
        if (ModelState.IsValid)
        {
            await _roomService.CreateAsync(dto);
            TempData["Success"] = "Salle créée avec succès.";
            return RedirectToAction(nameof(Index));
        }
        return View(dto);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var room = await _roomService.GetByIdAsync(id.Value);
        if (room == null) return NotFound();
        return View(new UpdateRoomDto { Id = room.Id, Name = room.Name, Capacity = room.Capacity, Location = room.Location });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateRoomDto dto)
    {
        if (id != dto.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                await _roomService.UpdateAsync(dto);
                TempData["Success"] = "Salle modifiée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        return View(dto);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var room = await _roomService.GetByIdAsync(id.Value);
        if (room == null) return NotFound();
        return View(room);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _roomService.DeleteAsync(id);
        TempData["Success"] = "Salle supprimée avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
