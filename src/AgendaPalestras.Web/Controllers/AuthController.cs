using System.Security.Claims;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConviteService _conviteService;

    public AuthController(IAuthService authService, IConviteService conviteService)
    {
        _authService = authService;
        _conviteService = conviteService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginDto("", "", false));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
        {
            ModelState.AddModelError(string.Empty, "Email e senha são obrigatórios.");
            return View(dto);
        }

        var usuario = await _authService.AutenticarAsync(dto);
        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha incorretos.");
            return View(dto);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Papel)
        };

        if (usuario.CongregacaoId.HasValue)
        {
            claims.Add(new Claim("CongregacaoId", usuario.CongregacaoId.Value.ToString()));
            if (!string.IsNullOrWhiteSpace(usuario.CongregacaoNome))
                claims.Add(new Claim("CongregacaoNome", usuario.CongregacaoNome));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = dto.LembrarMe,
            ExpiresUtc = dto.LembrarMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register(string? token)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            ViewBag.ErroBloqueio = "A criação de conta é restrita. O Super Admin deve gerar um link de cadastro para você.";
            return View("RegistroRestrito");
        }

        var tokenLimpo = token.Trim();
        var valido = await _conviteService.ValidarConviteAsync(tokenLimpo);
        if (!valido)
        {
            ViewBag.ErroBloqueio = "Este link de convite é inválido, já foi utilizado ou expirou. Solicite um novo link ao Super Admin.";
            return View("RegistroRestrito");
        }

        var convite = await _conviteService.ObterPorCodigoAsync(tokenLimpo, $"{Request.Scheme}://{Request.Host}");
        var emailConvidado = convite?.EmailConvidado ?? "";

        return View(new RegistrarUsuarioDto("", emailConvidado, "", "", TokenConvite: tokenLimpo));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistrarUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TokenConvite))
        {
            ViewBag.ErroBloqueio = "Token de convite ausente. O cadastro só pode ser realizado através de um link de convite válido.";
            return View("RegistroRestrito");
        }

        var valido = await _conviteService.ValidarConviteAsync(dto.TokenConvite);
        if (!valido)
        {
            ModelState.AddModelError(string.Empty, "Link de convite inválido, expirado ou já utilizado.");
            return View(dto);
        }

        if (string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
        {
            ModelState.AddModelError(string.Empty, "Nome, e-mail e senha são obrigatórios.");
            return View(dto);
        }

        if (string.IsNullOrWhiteSpace(dto.NomeCongregacao))
        {
            ModelState.AddModelError(string.Empty, "O nome da congregação é obrigatório.");
            return View(dto);
        }

        try
        {
            var usuario = await _authService.RegistrarUsuarioAsync(dto);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nome),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Papel)
            };

            if (usuario.CongregacaoId.HasValue)
            {
                claims.Add(new Claim("CongregacaoId", usuario.CongregacaoId.Value.ToString()));
                if (!string.IsNullOrWhiteSpace(usuario.CongregacaoNome))
                    claims.Add(new Claim("CongregacaoNome", usuario.CongregacaoNome));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["Success"] = $"Conta criada com sucesso! Bem-vindo à Congregação {usuario.CongregacaoNome}.";
            return RedirectToAction("Index", "Agenda");
        }
        catch (AgendaPalestras.Domain.Exceptions.DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
