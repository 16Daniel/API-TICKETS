using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Drawing;
using TICKETSAPI.ModelsTickets;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AceiteController : ControllerBase
    {
        private readonly ILogger<CalendarioNominaController> _logger;
        protected TicketsContext _tdbContext;
        protected BD2Context _bd2Context;

        public AceiteController(ILogger<CalendarioNominaController> logger, TicketsContext tkc, BD2Context bd2c)
        {
            _logger = logger;
            _tdbContext = tkc;
            _bd2Context = bd2c; 
        }

        [HttpPost]
        [Route("getRemisionesAceite")]
        public async Task<IActionResult> GetRemisionesAceite([FromForm] DateTime fechaInicio, [FromForm] DateTime fechaFin)
        {
            var resultados = new List<RemisionAceite>();
            var connectionString = _tdbContext.Database.GetDbConnection().ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("SP_GET_REMINISIONES_ACEITE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@FI", SqlDbType.DateTime).Value = fechaInicio;
                    command.Parameters.Add("@FF", SqlDbType.DateTime).Value = fechaFin;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new RemisionAceite
                            {
                                CodAlmacen = reader["CODALMACEN"].ToString(),
                                NombreAlmacen = reader["NOMBREALMACEN"].ToString(),
                                Compras = Convert.ToDecimal(reader["COMPRAS"]),
                                Consumos = Convert.ToDecimal(reader["CONSUMOS"]),
                                Descripcion = reader["DESCRIPCION"].ToString(),
                                Referencia = reader["REFERENCIA"].ToString(),
                                CodigoInterno = reader["CODIGO_INTERNO"].ToString(),
                                Marca = reader["MARCA"].ToString(),
                                Fecha = Convert.ToDateTime(reader["FECHA"])
                            };

                            resultados.Add(item);
                        }
                    }
                }
            }

            foreach (var item in resultados) 
            {
                int idf = -1;
                try { idf = int.Parse(item.CodAlmacen);  }catch (Exception ex) { idf = -1; }
                if (idf > -1) 
                {
                    var reg = _tdbContext.ControlAceites.Where( x=>x.IdSucursal == idf && x.Fecha.Date == item.Fecha.Date && x.Manual ==  null).FirstOrDefault();
                    if (reg == null) 
                    {
                        _tdbContext.ControlAceites.Add(new ControlAceite()
                        {
                            IdSucursal = idf,
                            Fecha = item.Fecha, 
                            EntregaCedis = (double)item.Compras,
                            Status = 1,
                            Fecharecoleccion = item.Fecha,
                        });
                       await _tdbContext.SaveChangesAsync();
                    }
                }
            }
           
            return Ok(resultados);
        }


        [HttpGet]
        [Route("getEntregasAceitePendientes/{ids}")]
        public async Task<IActionResult> GetEntregasAceiteP(int ids)
        {
            var data = _tdbContext.ControlAceites.Where(x=> (x.Status == 1 || x.Status == 4) && x.IdSucursal == ids).OrderBy(x => x.Fecha).ToList(); 
            return Ok(data);

        }

        [HttpGet]
        [Route("getEntregasAceitePendientesAdmin")]
        public async Task<IActionResult> GetEntregasAceitePAdmin()
        {
            var data = _tdbContext.ControlAceites.Where(x => (x.Status == 1 || x.Status == 4)).OrderBy(x => x.Fecha).ToList();
            return Ok(data);

        }

        [HttpGet]
        [Route("getEntregasAceitePendientesCedis")]
        public async Task<IActionResult> GetEntregasAceitePCedis()
        {
            var data = _tdbContext.ControlAceites.Where(x => x.Status == 2).OrderBy(x => x.Fecha).ToList();
            return Ok(data);

        }

        [HttpPost]
        [Route("getEntregasAceiteH")]
        public async Task<IActionResult> GetEntregasAceiteH([FromForm]int ids, [FromForm] DateTime fechaini, [FromForm] DateTime fechafin)
        {
            var data = _tdbContext.ControlAceites.Where(x => (x.Status == 2 || x.Status == 3) && x.IdSucursal == ids && x.Fecha.Date >= fechaini.Date && x.Fecha.Date<= fechafin.Date).OrderByDescending(x => x.Fecha).ToList();
            return Ok(data);

        }

        [HttpPost]
        [Route("getEntregasAceiteCedisH")]
        public async Task<IActionResult> GetEntregasAceiteCedisH([FromForm] string ids, [FromForm] DateTime fechaini, [FromForm] DateTime fechafin)
        {
            int[] sucursales = System.Text.Json.JsonSerializer.Deserialize<int[]>(ids);
            List<ControlAceite> data = new List<ControlAceite>();

            foreach (int idsuc in sucursales)
            {
                var dataSuc = _tdbContext.ControlAceites.Where(x => x.Status == 3 && x.Fecha.Date >= fechaini.Date && x.Fecha.Date <= fechafin.Date && x.IdSucursal == idsuc).OrderByDescending(x => x.Fecha).ToList();
                if(dataSuc.Count > 0) { data.AddRange(dataSuc); }
            }

            return Ok(data);

        }

        [HttpPost]
        [Route("UpdateEntregaAceite")]
        public async Task<IActionResult> UpdateEntrega([FromForm] int idReg, [FromForm]double cantidad, [FromForm] string comentarioSuc)
        {  

            var reg = _tdbContext.ControlAceites.Where(x => x.Id == idReg).FirstOrDefault();
            if (reg != null) 
            {  
                var reganterior = _tdbContext.ControlAceites.Where(x => x.IdSucursal == reg.IdSucursal && x.Fecha.Date < reg.Fecha.Date).OrderByDescending(x => x.Fecha).FirstOrDefault();
                reg.EntregaSucursal = cantidad;
                reg.ComentariosSucursal = comentarioSuc;

                double porcentajedevuelto = 1;
                reg.Diferencia = "0"; 
                
                if (reganterior == null && reg.EntregaCedis>0)
                {
                  porcentajedevuelto = (double)(reg.EntregaSucursal / reg.EntregaCedis);
                    reg.Diferencia = (reg.EntregaCedis - reg.EntregaSucursal).ToString();
                }
                else 
                {
                    if (reg.EntregaCedis > 0) 
                    {
                        porcentajedevuelto = (double)(reg.EntregaSucursal / reganterior.EntregaCedis);
                        reg.Diferencia = (reganterior.EntregaCedis - reg.EntregaSucursal).ToString();
                    }
                    
                }

                reg.Porcentaje75 = porcentajedevuelto;
                if (porcentajedevuelto < 0.75) { reg.Intercambio = 1; }
                if (porcentajedevuelto >= 0.75 && porcentajedevuelto <= 1) { reg.Intercambio = 2; }
                if (porcentajedevuelto >1) { reg.Intercambio = 3; }
                
                reg.Status = 2; 
                _tdbContext.ControlAceites.Update(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }


        [HttpPost]
        [Route("ValidacionCedis")]
        public async Task<IActionResult> ValidacionCedis([FromForm] int idReg, [FromForm] string comentarioCedis)
        {
            var reg = _tdbContext.ControlAceites.Where(x => x.Id == idReg).FirstOrDefault();
            if (reg != null)
            {
                reg.ComentariosCedis = comentarioCedis;
                reg.Status = 3;
                reg.Fecharecoleccion = DateTime.Now;    
                _tdbContext.ControlAceites.Update(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }


        [HttpPost]
        [Route("RechazoCedis")]
        public async Task<IActionResult> rechazoCedis([FromForm] int idReg, [FromForm] string comentarioCedis)
        {
            var reg = _tdbContext.ControlAceites.Where(x => x.Id == idReg).FirstOrDefault();
            if (reg != null)
            {
                reg.EntregaSucursal = null;
                reg.ComentariosSucursal = "";
                reg.ComentariosCedis = comentarioCedis; 
                reg.Status = 4;
                _tdbContext.ControlAceites.Update(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }


        [HttpPost]
        [Route("agregarEntregaManual")]
        public async Task<IActionResult> agregarEntregaManual([FromForm] int ids, [FromForm] DateTime fecha)
        {
            try 
            {
                _tdbContext.ControlAceites.Add(new ControlAceite()
                {
                    IdSucursal = ids,
                    Fecha = fecha,
                    EntregaCedis = 0,
                    Status = 1,
                    Manual = true,
                    Fecharecoleccion = DateTime.Now,
                });
                await _tdbContext.SaveChangesAsync();
                return Ok();    
            } catch (Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("agregarRecoleccionTrampaAceite")]
        public async Task<IActionResult> agregarRecoleccionTrampaAceite([FromForm] int ids, [FromForm] DateTime fecha)
        {
            try
            {
                _tdbContext.ControlTrampaAceites.Add(new ControlTrampaAceite()
                {
                    IdSucursal = ids,
                    Fecha = fecha,
                    EntregaCedis = 0,
                    Status = 1,
                    Manual = true,
                    Fecharecoleccion = DateTime.Now,
                });
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        [Route("getEntregasTrampaAceitePendientes/{ids}")]
        public async Task<IActionResult> GetEntregasTrampaAceiteP(int ids)
        {
            var data = _tdbContext.ControlTrampaAceites.Where(x => (x.Status == 1 || x.Status == 4) && x.IdSucursal == ids).OrderBy(x => x.Fecha).ToList();
            return Ok(data);

        }

        [HttpPost]
        [Route("UpdateEntregaAceiteTA")]
        public async Task<IActionResult> UpdateEntregaTA([FromForm] int idReg, [FromForm] double cantidad, [FromForm] string comentarioSuc)
        {

            var reg = _tdbContext.ControlTrampaAceites.Where(x => x.Id == idReg).FirstOrDefault();
            if (reg != null)
            {
                var reganterior = _tdbContext.ControlTrampaAceites.Where(x => x.IdSucursal == reg.IdSucursal && x.Fecha.Date < reg.Fecha.Date).OrderByDescending(x => x.Fecha).FirstOrDefault();
                reg.EntregaSucursal = cantidad;
                reg.ComentariosSucursal = comentarioSuc;

                double porcentajedevuelto = 1;
                reg.Diferencia = "0";

                if (reganterior == null && reg.EntregaCedis > 0)
                {
                    porcentajedevuelto = (double)(reg.EntregaSucursal / reg.EntregaCedis);
                    reg.Diferencia = (reg.EntregaCedis - reg.EntregaSucursal).ToString();
                }
                else
                {
                    if (reg.EntregaCedis > 0)
                    {
                        porcentajedevuelto = (double)(reg.EntregaSucursal / reganterior.EntregaCedis);
                        reg.Diferencia = (reganterior.EntregaCedis - reg.EntregaSucursal).ToString();
                    }

                }

                reg.Porcentaje75 = porcentajedevuelto;
                if (porcentajedevuelto < 0.75) { reg.Intercambio = 1; }
                if (porcentajedevuelto >= 0.75 && porcentajedevuelto <= 1) { reg.Intercambio = 2; }
                if (porcentajedevuelto > 1) { reg.Intercambio = 3; }

                reg.Status = 2;
                _tdbContext.ControlTrampaAceites.Update(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }

        [HttpPost]
        [Route("ValidacionCedisTA")]
        public async Task<IActionResult> ValidacionCedisTA([FromForm] int idReg, [FromForm] string comentarioCedis)
        {
            var reg = _tdbContext.ControlTrampaAceites.Where(x => x.Id == idReg).FirstOrDefault();
            if (reg != null)
            {
                reg.ComentariosCedis = comentarioCedis;
                reg.Status = 3;
                reg.Fecharecoleccion = DateTime.Now;
                _tdbContext.ControlTrampaAceites.Update(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }


        [HttpPost]
        [Route("RechazoCedisTA")]
        public async Task<IActionResult> rechazoCedisTA([FromForm] int idReg, [FromForm] string comentarioCedis)
        {
            var reg = _tdbContext.ControlTrampaAceites.Where(x => x.Id == idReg).FirstOrDefault();
            if (reg != null)
            {
                reg.EntregaSucursal = null;
                reg.ComentariosSucursal = "";
                reg.ComentariosCedis = comentarioCedis;
                reg.Status = 4;
                _tdbContext.ControlTrampaAceites.Update(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }

        [HttpGet]
        [Route("getEntregasAceitePendientesCedisTA")]
        public async Task<IActionResult> GetEntregasAceitePCedisTA()
        {
            var data = _tdbContext.ControlTrampaAceites.Where(x => x.Status == 2).OrderBy(x => x.Fecha).ToList();
            return Ok(data);

        }


        [HttpPost]
        [Route("getEntregasAceiteTAH")]
        public async Task<IActionResult> GetEntregasAceiteTAH([FromForm] int ids, [FromForm] DateTime fechaini, [FromForm] DateTime fechafin)
        {
            var data = _tdbContext.ControlTrampaAceites.Where(x => (x.Status == 2 || x.Status == 3) && x.IdSucursal == ids && x.Fecha.Date >= fechaini.Date && x.Fecha.Date <= fechafin.Date).OrderByDescending(x => x.Fecha).ToList();
            return Ok(data);

        }


        [HttpPost]
        [Route("getEntregasAceiteCedisTAH")]
        public async Task<IActionResult> GetEntregasAceiteCedisTAH([FromForm] string ids, [FromForm] DateTime fechaini, [FromForm] DateTime fechafin)
        {
            int[] sucursales = System.Text.Json.JsonSerializer.Deserialize<int[]>(ids);
            List<ControlTrampaAceite> data = new List<ControlTrampaAceite>();

            foreach (int idsuc in sucursales)
            {
                var dataSuc = _tdbContext.ControlTrampaAceites.Where(x => x.Status == 3 && x.Fecha.Date >= fechaini.Date && x.Fecha.Date <= fechafin.Date && x.IdSucursal == idsuc).OrderByDescending(x => x.Fecha).ToList();
                if (dataSuc.Count > 0) { data.AddRange(dataSuc); }
            }

            return Ok(data);

        }


        [HttpDelete]
        [Route("eliminarLineaAceite/{id}")]
        public async Task<IActionResult> eliminarLineaAceite(int id)
        {
            var reg = _tdbContext.ControlAceites.Where(x => x.Id == id).FirstOrDefault();
            if (reg != null)
            {
                _tdbContext.ControlAceites.Remove(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }

        [HttpGet]
        [Route("getEntregasAceitePendientesAdminTA")]
        public async Task<IActionResult> GetEntregasAceitePAdminTA()
        {
            var data = _tdbContext.ControlTrampaAceites.Where(x => (x.Status == 1 || x.Status == 4)).OrderBy(x => x.Fecha).ToList();
            return Ok(data);

        }

        [HttpDelete]
        [Route("eliminarLineaAceiteTA/{id}")]
        public async Task<IActionResult> eliminarLineaAceiteTA(int id)
        {
            var reg = _tdbContext.ControlTrampaAceites.Where(x => x.Id == id).FirstOrDefault();
            if (reg != null)
            {
                _tdbContext.ControlTrampaAceites.Remove(reg);
                await _tdbContext.SaveChangesAsync();
            }
            return Ok();

        }

        [HttpPost]
        [Route("getReporteRecoleccionAceite")]
        public async Task<IActionResult> getReporteRecoleccionAceite([FromForm] string ids, [FromForm] DateTime fechaini, [FromForm] DateTime fechafin)
        {
            int[] sucursales = System.Text.Json.JsonSerializer.Deserialize<int[]>(ids);
            List<ReporteRA> data = new List<ReporteRA>();
            var connectionString = _tdbContext.Database.GetDbConnection().ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();

            foreach (int idsuc in sucursales) 
            {
                string codalmacen = "0"; 
                var cajafront = _bd2Context.RemCajasfronts.Where(x => x.Idfront == idsuc && x.Cajafront == 1).FirstOrDefault();
                if (cajafront != null) { codalmacen = cajafront.Codalmventas; }
                var resultados = new List<RemisionAceite>();

                var command = new SqlCommand("SP_GET_REMINISIONES_ACEITE_SUC", connection); 
                    
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@FI", SqlDbType.DateTime).Value = fechaini.Date;
                        command.Parameters.Add("@FF", SqlDbType.DateTime).Value = fechafin.Date;
                        command.Parameters.Add("@IDS", SqlDbType.NVarChar).Value = codalmacen;


                var reader = command.ExecuteReader(); 
                      
                            while (reader.Read())
                            {
                                var item = new RemisionAceite
                                {
                                    CodAlmacen = reader["CODALMACEN"].ToString(),
                                    NombreAlmacen = reader["NOMBREALMACEN"].ToString(),
                                    Compras = Convert.ToDecimal(reader["COMPRAS"]),
                                    Consumos = Convert.ToDecimal(reader["CONSUMOS"]),
                                    Descripcion = reader["DESCRIPCION"].ToString(),
                                    Referencia = reader["REFERENCIA"].ToString(),
                                    CodigoInterno = reader["CODIGO_INTERNO"].ToString(),
                                    Marca = reader["MARCA"].ToString(),
                                    Fecha = Convert.ToDateTime(reader["FECHA"])
                                };

                                resultados.Add(item);
                            }
                reader.Close(); 

                var dataSuc = _tdbContext.ControlAceites.Where(x => x.Fecha.Date >= fechaini.Date && x.Fecha.Date <= fechafin.Date && x.IdSucursal == idsuc).OrderByDescending(x => x.Fecha).ToList();

                ReporteRA dataR = new ReporteRA();
                dataR.idf = idsuc;
               dataR.entregaCedis = (int)resultados.Sum(x => x.Compras);
                dataR.recoleccionConfirmada = (int)dataSuc.Where(x=>x.Status == 3).ToList().Sum(x => x.EntregaSucursal);
                dataR.recoleccion = (int)dataSuc.Sum(x => x.EntregaSucursal); 
                data.Add(dataR);

            }
            connection.Close(); 
            return Ok(data);

        }

        [HttpPost("generar-excel")]
        public IActionResult GenerarExcel([FromBody] List<ControlAceite> entregas)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Importante para usar EPPlus

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Entregas");

            // Encabezados
            var headers = new[]
            {
            "SUCURSAL", "FECHA", "ENTREGA CEDIS", "RECOLECCIÓN",
            "PORCENTAJE","DIFERENCIA","COMENTARIOS CEDIS", "COMENTARIOS SUCURSAL"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            using (var range = worksheet.Cells["A1:H1"])
            {
                Color colorFondo = ColorTranslator.FromHtml("#00000000");
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(colorFondo);
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.AutoFitColumns();
            }

    

            // Datos
            for (int i = 0; i < entregas.Count; i++)
            {
                string nombreSuc = ""; 
                var e = entregas[i];
                double porcentaje = e.Porcentaje75.Value * 100; 
                Color colorFondo = ColorTranslator.FromHtml("#FFFFFF");

                if (e.Porcentaje75 < .75) 
                {
                  colorFondo = ColorTranslator.FromHtml("#ffb5b5"); 
                }

                if (e.Porcentaje75 >= .75 && e.Porcentaje75 <= 1 )
                {
                    colorFondo = ColorTranslator.FromHtml("#b7feb5");
                }

                if (e.Porcentaje75>1)
                {
                    colorFondo = ColorTranslator.FromHtml("#ffed93");
                }

                var sucursal = _bd2Context.RemFronts.Where(x=>x.Idfront == e.IdSucursal).FirstOrDefault();
                if (sucursal != null) { nombreSuc = sucursal.Titulo;  } 
                worksheet.Cells[i + 2, 1].Value = nombreSuc;
                worksheet.Cells[i + 2, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 1].Style.Fill.BackgroundColor.SetColor(colorFondo);

                worksheet.Cells[i + 2, 2].Value = e.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cells[i + 2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 2].Style.Fill.BackgroundColor.SetColor(colorFondo);

                worksheet.Cells[i + 2, 3].Value = e.EntregaCedis;
                worksheet.Cells[i + 2, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 3].Style.Fill.BackgroundColor.SetColor(colorFondo);

                worksheet.Cells[i + 2, 4].Value = e.EntregaSucursal;
                worksheet.Cells[i + 2, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 4].Style.Fill.BackgroundColor.SetColor(colorFondo);

                worksheet.Cells[i + 2, 5].Value = porcentaje.ToString("F2");
                worksheet.Cells[i + 2, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 5].Style.Fill.BackgroundColor.SetColor(colorFondo);

                worksheet.Cells[i + 2, 6].Value = e.Diferencia;
                worksheet.Cells[i + 2, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 6].Style.Fill.BackgroundColor.SetColor(colorFondo);

                worksheet.Cells[i + 2, 7].Value = e.ComentariosCedis;
                worksheet.Cells[i + 2, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 7].Style.Fill.BackgroundColor.SetColor(colorFondo);

                worksheet.Cells[i + 2, 8].Value = e.ComentariosSucursal;
                worksheet.Cells[i + 2, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[i + 2, 8].Style.Fill.BackgroundColor.SetColor(colorFondo);
            }

            var fileBytes = package.GetAsByteArray();
            var base64 = Convert.ToBase64String(fileBytes);

            return Ok(new { archivoBase64 = base64 });
        }

    }

    public class RemisionAceite
        {
            public string CodAlmacen { get; set; }
            public string NombreAlmacen { get; set; }
            public decimal Compras { get; set; }
            public decimal Consumos { get; set; }
            public string Descripcion { get; set; }
            public string Referencia { get; set; }
            public string CodigoInterno { get; set; }
            public string Marca { get; set; }
            public DateTime Fecha { get; set; }
        }

    public class EntregaAceite
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public DateTime Fecha { get; set; }
        public int? EntregaCedis { get; set; }
        public int? EntregaSucursal { get; set; }
        public double? Porcentaje75 { get; set; }
        public double? Intercambio { get; set; }
        public double? Diferencia { get; set; }
        public string? ComentariosCedis { get; set; }
        public string? ComentariosSucursal { get; set; }
        public int Status { get; set; }
    }

    public class ReporteRA 
    {
        public int idf { get; set; }
        public int entregaCedis { get; set; }
        public int recoleccion { get; set; }  
        public int recoleccionConfirmada { get; set; }  

    }

}
