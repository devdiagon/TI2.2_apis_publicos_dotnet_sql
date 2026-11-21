using ec.edu.monster.excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ec.edu.monster.controlador;

[AutoValidateAntiforgeryToken]
public abstract class ControladorBase : Controller
{
    private const string MsgGenerico = "Error interno del servidor";

    protected string RetornoError { get; set; } = "/paises";

    public override async Task OnActionExecutionAsync(ActionExecutingContext contexto, ActionExecutionDelegate siguiente)
    {
        var ejecutada = await siguiente();
        if (ejecutada.Exception is not { } error)
        {
            return;
        }

        var log = HttpContext.RequestServices.GetRequiredService<ILogger<ControladorBase>>();
        string mensaje;
        int estado;
        if (error is ExcepcionMonster monster)
        {
            if (monster.InnerException is not null)
            {
                log.LogWarning(monster.InnerException, "{Mensaje}", monster.Message);
            }
            mensaje = monster.Message;
            estado = StatusCodes.Status200OK;
        }
        else
        {
            log.LogError(error, "Error inesperado");
            mensaje = MsgGenerico;
            estado = StatusCodes.Status500InternalServerError;
        }

        ejecutada.ExceptionHandled = true;
        if (HttpMethods.IsPost(Request.Method))
        {
            Avisar("error", mensaje);
            ejecutada.Result = Redirigir(RetornoError);
        }
        else
        {
            ViewData["MensajeError"] = mensaje;
            var vista = View("Error");
            vista.StatusCode = estado;
            ejecutada.Result = vista;
        }
    }

    protected void Avisar(string tipo, string mensaje)
    {
        TempData["tipo"] = tipo;
        TempData["mensaje"] = mensaje;
    }
    
    protected IActionResult Redirigir(string ruta) => Redirect(Url.Content("~" + ruta));
}
