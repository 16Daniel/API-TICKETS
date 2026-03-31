using System.Net.Mail;
using System.Net;
using System.Text;
using OfficeOpenXml;
using System.Drawing;
using System.Data;
using OfficeOpenXml.Style;
using System.Globalization;
using TICKETSAPI.Controllers;

namespace DashboardApi.Mail
{
    public class MailC
    {

        public void EnviarCorreo(string destinatario,string bodymail, string asuntop)
        {
            //// Configurar la información de la cuenta de Gmail
            string correoRemitente = "gilberto.r@operamx.com";
            string contraseña = "sjlh rtya uehm pjmk";

            // Configurar la información del destinatario
            string correoDestinatario = destinatario;
            //string correoDestinatario = "arturo.m@operamx.com";
            string asunto = asuntop;

            // Configurar el cliente SMTP de Gmail
            SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoRemitente, contraseña),
                EnableSsl = true,
            };

            // Crear el mensaje de correo
            MailMessage mensaje = new MailMessage(correoRemitente, correoDestinatario, asunto, string.Empty)
            {
                IsBodyHtml = true,
                Body = bodymail,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            try
            {
                // Enviar el mensaje
                clienteSmtp.Send(mensaje);
                Console.WriteLine("Correo enviado con éxito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
            }
            finally
            {
                // Liberar recursos
                mensaje.Dispose();
            }
        }

        public void EnviarCorreoTareas(string destinatario, string bodymail, string asuntop)
        {
            //// Configurar la información de la cuenta de Gmail
            string correoRemitente = "actividades@operamx.com";
            string contraseña = "ptlx ddmb daso edfo";

            // Configurar la información del destinatario
            string correoDestinatario = destinatario;
            //string correoDestinatario = "arturo.m@operamx.com";
            string asunto = asuntop;

            // Configurar el cliente SMTP de Gmail
            SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoRemitente, contraseña),
                EnableSsl = true,
            };

            // Crear el mensaje de correo
            MailMessage mensaje = new MailMessage(correoRemitente, correoDestinatario, asunto, string.Empty)
            {
                IsBodyHtml = true,
                Body = bodymail,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                From = new MailAddress(correoRemitente,"Actividades")
        };

            try
            {
                // Enviar el mensaje
                clienteSmtp.Send(mensaje);
                Console.WriteLine("Correo enviado con éxito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
            }
            finally
            {
                // Liberar recursos
                mensaje.Dispose();
            }
        }

        public void EnviarCorreoFaltas(string bodymail, string asuntop)
        {
            //// Configurar la información de la cuenta de Gmail
            string correoRemitente = "gilberto.r@operamx.com";
            string contraseña = "sjlh rtya uehm pjmk";

            //// Configurar la información de la cuenta de Gmail
            //string correoRemitente = "it_token@operamx.com";
            //string contraseña = "M@5TERKEY";

            // Configurar la información del destinatario
            string correoDestinatario = "enrique.j@operamx.com";
            //string correoDestinatario = "arturo.m@operamx.com";
            string asunto = asuntop;

            // Configurar el cliente SMTP de Gmail
            SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoRemitente, contraseña),
                EnableSsl = true,
            };

            // Crear el mensaje de correo
            MailMessage mensaje = new MailMessage(correoRemitente, correoDestinatario, asunto, string.Empty)
            {
                IsBodyHtml = true,
                Body = bodymail,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            mensaje.To.Add("daniel.l@operamx.com");
            mensaje.Bcc.Add("daniel.h@operamx.com");
            mensaje.To.Add("gilberto.r@operamx.com");
            mensaje.Bcc.Add("arturo.m@operamx.com");
            mensaje.To.Add("carlos.c@operamx.com");
            mensaje.To.Add("jorge.j@operamx.com");
            mensaje.To.Add("ulises.m@operamx.com");

            //regionales 
            mensaje.To.Add("jose.r@operamx.com");
            mensaje.To.Add("monica.r@operamx.com");
            mensaje.To.Add("edith.h@operamx.com");
            mensaje.To.Add("christopher.m@operamx.com");
            mensaje.To.Add("eduardo.p@operamx.com");
            mensaje.To.Add("sergio.g@operamx.com");
            mensaje.To.Add("carlos.t@operamx.com");

            try
            {
                // Enviar el mensaje
                clienteSmtp.Send(mensaje);
                Console.WriteLine("Correo enviado con éxito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
            }
            finally
            {
                // Liberar recursos
                mensaje.Dispose();
            }
        }

        public string generarMailBodyPersonalNomina(List<PersonalFaltante> data,string sucursal,string nombreRegional)
        {
            string body = @"
                                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                          <meta charset=""UTF-8"">
                          <title>Alerta de Personal Faltante</title>
                          <style>
                            .table
                            {
                                width: 100%;
                                table-layout: auto;
                                text-align: center;
                            }
                          </style>
                        </head>
                        <body style=""font-family: Arial, sans-serif; background-color: #f9f9f9; color: #333; padding: 20px;"">
                          <table style=""max-width: 600px; margin: auto; background-color: #ffffff; border: 1px solid #ddd; border-radius: 5px;"">
                           
  <tr>
    <td align=""center"" style=""padding-top: 20px;"">
      <img src=""https://rebelwings.mx/wp-content/uploads/2017/12/RW_LogoWEB.png"" alt=""logo"" style=""display: block;"" width=""150px"" border=""0""> 
    </td>
  </tr>
  
<tr>
                              <td style=""padding: 20px; padding-top: 0px;"">

                                <h2 style=""color: #d9534f; text-align: center;"">⚠️ ALERTA DE PERSONAL FALTANTE</h2>
                                <p>REGIONAL: --regional</p>

                                <p>Se informa que la sucursal <strong style=""color: orangered;"">--sucursal</strong> presenta personal faltante en este momento.</p>

                                <h3>Detalles:</h3>
                                <div style=""margin: 10px;"">
                                    --data
                                </div>

                                <p>Se recomienda tomar las medidas necesarias para garantizar la operación mínima en la sucursal y evitar afectaciones al servicio.</p>

                                <p>Saludos cordiales</p>
                              </td>
                            </tr>
                          </table>
                        </body>
                        </html>
            
            ";

            string tabla = @"
  <table class=""table"">
  <thead>
    <tr>
      <th scope=""col"">PUESTO</th>
      <th scope=""col"">EMPLEADOS CALENDARIZADOS</th>
      <th scope=""col"">EMPLEADOSS FALTANTES</th>
    </tr>
  </thead>
  <tbody>"; 
            foreach (PersonalFaltante personal in data) 
            {
                tabla += "<tr>";
                tabla += "<td>" + personal.nombrepuesto.Trim()+"</td>";
                tabla += "<td>" + personal.empleadosRequeridos + "</td>";
                tabla += @"<td style=""color:red"">" + personal.empleadosFaltantes + "</td>";
                tabla += "</tr>";
            }
            tabla += "</tbody></table>"; 

            body = body.Replace("--sucursal", sucursal);
            body = body.Replace("--data", tabla); 
            body = body.Replace("--regional",nombreRegional); 
            return body;
        }

    }
}
