﻿using Data.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoIt2.Services;

namespace Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;
        public LoginController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _loginService = new LoginService(httpClientFactory, configuration);
        }
        public IActionResult Index()
        {
            if (TempData["ErrorLogin"] != null)
            {
                ViewBag.ErrorLogin = TempData["ErrorLogin"].ToString();
            }
            return View();
        }

        public IActionResult CrearCuenta()
        {
            return View();
        }

        public IActionResult OlvidoClave()
        {
            return View();
        }

        public IActionResult RecuperarCuenta()
        {
            return View();
        }


        public async Task<ActionResult> LoginLocal(LoginDto loginDto)
        {
            var resultadoUsuario = await _loginService.ObtenerUsuario(loginDto);

            if (resultadoUsuario != null)
            {
                var principalClaim = await _loginService.ClaimLogin(resultadoUsuario);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principalClaim, new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.Now.AddMinutes(30)
                });

                return View("~/Views/Home/Index.cshtml");
            }

            return RedirectToAction("Index", "Login");
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        public async Task<ActionResult> CrearUsuario(CrearCuentaDto crearUsuarioDto)
        {
            var responseCuenta = await _loginService.GuardarUsuario(crearUsuarioDto);

            if (responseCuenta != null && Convert.ToBoolean(responseCuenta.Value))
            {
                TempData["ErrorLogin"] = "Se creo el usuario correctamente";
            }
            else
            {
                TempData["ErrorLogin"] = "no se pudo crear el usuario. Contacte a sistemas";
            }

            return RedirectToAction("index", "Login");
        }
    
        public async Task<ActionResult> EnviarMail(LoginDto loginDto)
        {
            _loginService.EnviarMail(loginDto);
            TempData["Mail"] = loginDto.Mail;
            return RedirectToAction("RecuperarCuenta", "Login");
        }

        public async Task<ActionResult> CambiarClave(LoginDto loginDto)
        {
            var resultadoCuenta = await _loginService.CambiarClave(loginDto, TempData["Mail"].ToString());
            if (resultadoCuenta)
            {
                TempData["ErrorLogin"] = "Se ha cambiado la clave correctamente";
            }
            else
            {
                TempData["ErrorLogin"] = "El codigo ingresado no coincide con el enviado al mail";
            }

            return RedirectToAction("Index", "Login");
        }
    
    }
}
