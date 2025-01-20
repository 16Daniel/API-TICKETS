using DashboardApi.Controllers;
using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<CatalogosController> _logger;
        protected BD2Context _contextdb2;
        protected TicketsContext _tdbContext;


        public UsuariosController(ILogger<CatalogosController> logger, BD2Context db2c, TicketsContext tdbc)
        {
            _logger = logger;
            _contextdb2 = db2c;
            _tdbContext = tdbc;
        }

        [HttpGet]
        [Route("getusUarios")]
        public async Task<ActionResult> Getusuarios()
        {
            try
            {
                List<Usuario> usuarios = new List<Usuario>();
                usuarios = _tdbContext.Usuarios.ToList();

                return StatusCode(200, usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }


        [HttpPost]
        [Route("createUser")]
        public async Task<ActionResult> createuser(Usuario model)
        {
            try
            {
                _tdbContext.Usuarios.Add
                    (
                        new Usuario()
                        {
                            Nombre = model.Nombre,
                            ApellidoP = model.ApellidoP,
                            ApellidoM = model.ApellidoM,
                            IdRol = model.IdRol,
                            Email = model.Email,
                            Pass = model.Pass,
                        }
                    );
                await _tdbContext.SaveChangesAsync();
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }


        [HttpPost]
        [Route("updateUser")]
        public async Task<ActionResult> updateuser(Usuario model)
        {
            try
            {
                _tdbContext.Usuarios.Update(model);
                await _tdbContext.SaveChangesAsync();
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }

        [HttpGet]
        [Route("deleteUser/{id}")]
        public async Task<ActionResult> deleteuser(int id)
        {
            try
            {
                var user = _tdbContext.Usuarios.Find(id);
                if (user != null)
                {
                    _tdbContext.Usuarios.Remove(user);
                    await _tdbContext.SaveChangesAsync();
                }
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> Login(LoginModel model)
        {
            try
            {
                var usuario = _tdbContext.Usuarios.Where(x => x.Email == model.email && x.Pass == model.pass).FirstOrDefault();
                if (usuario != null)
                {
                    //var sesion = _tdbContext.Sesiones.Where(x => x.Idu == usuario.Id).FirstOrDefault();
                    //if (sesion == null)
                    //{
                    //    _tdbContext.Sesiones.Add(new Sesione() { Idu = usuario.Id, Activo = true });
                    //    await _tdbContext.SaveChangesAsync();
                    //    return StatusCode(200, usuario);
                    //}
                    //else
                    //{
                    //    if (sesion.Activo == true)
                    //    {
                    //        return StatusCode(StatusCodes.Status423Locked);
                    //    }
                    //    else
                    //    {
                    //        sesion.Activo = true;
                    //        _tdbContext.Sesiones.Update(sesion);
                    //        await _tdbContext.SaveChangesAsync();

                    //    }
                    //}
                    return StatusCode(200, usuario);

                }
                else { return StatusCode(StatusCodes.Status404NotFound); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }


        [HttpGet]
        [Route("logout/{idu}")]
        public async Task<ActionResult> logout(int idu)
        {
            try
            {
                //var sesion = _tdbContext.Sesiones.Where(x => x.Idu == idu).FirstOrDefault();
                //if (sesion == null)
                //{
                //    return StatusCode(StatusCodes.Status200OK);
                //}
                //else
                //{
                //    sesion.Activo = false;

                //    _tdbContext.Sesiones.Update(sesion);
                //    await _tdbContext.SaveChangesAsync();
                //    return StatusCode(StatusCodes.Status200OK);
                //}
                return StatusCode(StatusCodes.Status200OK);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }


        [HttpGet]
        [Route("TestCon")]
        public async Task<ActionResult> testCon()
        {
            var usuario = _tdbContext.Usuarios.FirstOrDefault();
            return StatusCode(200);
        }

    }

    public class LoginModel
    {
        public string email { get; set; }
        public string pass { get; set; }
    }
}
