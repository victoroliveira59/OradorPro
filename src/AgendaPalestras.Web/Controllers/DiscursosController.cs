using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize]
public class DiscursosController : Controller
{
    private readonly IDiscursoService _service;

    public DiscursosController(IDiscursoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var discursos = await _service.ListarTodosAsync();
        return View(discursos);
    }

    public IActionResult Create()
    {
        return View(new CriarDiscursoDto(0, ""));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarDiscursoDto dto)
    {
        try
        {
            await _service.CriarAsync(dto);
            TempData["Success"] = "Discurso cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var d = await _service.ObterPorIdAsync(id);
        if (d == null) return NotFound();

        var dto = new AtualizarDiscursoDto(d.Id, d.Numero, d.Tema, d.Ativo);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AtualizarDiscursoDto dto)
    {
        try
        {
            await _service.AtualizarAsync(dto);
            TempData["Success"] = "Discurso atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarStatus(Guid id)
    {
        await _service.AlternarStatusAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
