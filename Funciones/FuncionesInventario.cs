using DashboardApi.ModelsBD2;
using Microsoft.EntityFrameworkCore;
using TICKETSAPI.Controllers;

namespace TICKETSAPI.Funciones
{
    public class FuncionesInventario
    {
        protected BD2Context _context;
        public FuncionesInventario(BD2Context contextdb2) 
        {
            _context = contextdb2;
        }

        public decimal StockValidate(int id_sucursal, int codarticulo)
        {
            decimal _stock = 0;
            var serie = _context.RemCajasfronts.FirstOrDefault(x => x.Idfront == id_sucursal).Codalmventas;
            if (serie != null)
            {
                _stock = (decimal)_context.Stocks
                    .Join(_context.Articuloscamposlibres,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new
                    {
                        Codalmacen = art.Codalmacen,
                        Codarticulo = stk.Codarticulo,
                        Regulariza = stk.Regulariza,
                        Unidadessat = stk.Unidadessat,
                        Unidadmedida = stk.UnidadmedidaReg,
                        art.Stock1
                    })
                    .Join(_context.Articulos1,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new
                    {
                        Codalmacen = art.Codalmacen,
                        Descripcion = stk.Descripcion,
                        Codarticulo = art.Codarticulo,
                        Regulariza = art.Regulariza,
                        Unidadessat = art.Unidadessat,
                        Unidadmedida = art.Unidadmedida,
                        art.Stock1
                    })
                    .SingleOrDefault(s => s.Codalmacen == serie && s.Codarticulo == codarticulo && s.Regulariza == "T").Stock1.Value;

            }

            return _stock;
        }
        public decimal StockValidateV(int id_sucursal, int codarticulo)
        {




            var diaSemana = DateTime.Now.Date.DayOfWeek.ToString();
            decimal _stock = 0;
            var serie = _context.RemCajasfronts.FirstOrDefault(x => x.Idfront == id_sucursal).Codalmventas;
            if (serie != null)
            {
                _stock = (decimal)_context.Stocks
                    .Join(_context.Articuloscamposlibres,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new
                    {
                        Codalmacen = art.Codalmacen,
                        Codarticulo = stk.Codarticulo,
                        RegularizaSemanal = stk.RegularizaSemanal,
                        Unidadessat = stk.Unidadessat,
                        Unidadmedida = stk.UnidadmedidaReg,
                        art.Stock1
                    })
                    .Join(_context.Articulos1,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new
                    {
                        Codalmacen = art.Codalmacen,
                        Descripcion = stk.Descripcion,
                        Codarticulo = art.Codarticulo,
                        RegularizaSemanal = art.RegularizaSemanal,
                        Unidadessat = art.Unidadessat,
                        Unidadmedida = art.Unidadmedida,
                        art.Stock1
                    })
                    .SingleOrDefault(s => s.Codalmacen == serie && s.Codarticulo == codarticulo && s.RegularizaSemanal == "T").Stock1.Value;

            }



            return _stock;
        }


        public StockDto UpdateStock(int codArticulo, string codAlmacen, double cantidad)
        {
            StockDto _stock = new StockDto();

            //FECHA DE INVENTARIOS
            var tablaInv = DateTime.Now.Date.AddDays(-1);
            if (_context.Inventarios.FirstOrDefault(x => x.Codalmacen == codAlmacen && x.Fecha == DateTime.Now.Date) != null)
            {
                tablaInv = _context.Inventarios.FirstOrDefault(x => x.Codalmacen == codAlmacen && x.Fecha == DateTime.Now.Date).Fecha;
            }
            else { tablaInv = DateTime.Now.Date.AddDays(-1); }

            var __stock = _context.Stocks.FirstOrDefault(x => x.Codarticulo == codArticulo && x.Codalmacen == codAlmacen);
            double _stockAnterior = __stock.Stock1.Value;
            if (__stock != null)
            {
                __stock.Stock1 = cantidad;

                _stock.Codalmacen = codAlmacen;
                _stock.Descripcion = _stock.Descripcion;
                _stock.Codarticulo = codArticulo;
                _stock.Regulariza = "T";
                _stock.Unidadessat = "";
                _stock.Stock1 = cantidad;

                _context.Stocks.Update(__stock);
                if (tablaInv != DateTime.Now.Date)
                {
                    Inventario _inventario = new Inventario();
                    _inventario.Fecha = DateTime.Now.Date;
                    _inventario.Codalmacen = codAlmacen;
                    _inventario.Tipovaloracion = -3;
                    _inventario.Serie = "";
                    _inventario.Numero = 0;
                    _inventario.Codvendedor = -1;
                    _inventario.Completo = "F";
                    _inventario.Metodo = 1;
                    _inventario.Inicial = "F";
                    _inventario.Bloqueado = "F";
                    _inventario.Tipovaloraciondmn = null;
                    _inventario.Estado = 0;
                    _inventario.Escierre = false;
                    _inventario.EnlaceEjercicio = null;
                    _inventario.EnlaceEmpresa = null;
                    _inventario.EnlaceUsuario = null;
                    _inventario.EnlaceAsiento = null;

                    _context.Inventarios.Add(_inventario);

                }

                Moviment _moviment = new Moviment();
                _moviment.Codalmacenorigen = codAlmacen;
                _moviment.Codalmacendestino = "";
                _moviment.Numserie = "";
                _moviment.Codarticulo = codArticulo;
                _moviment.Talla = ".";
                _moviment.Color = ".";
                _moviment.Precio = _context.Articuloscamposlibres.FirstOrDefault(x => x.Codarticulo == codArticulo)?.Precioproveedor;
                _moviment.Fecha = DateTime.Now.Date;
                _moviment.Hora = Convert.ToDateTime("1899-12-30 " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ".000");
                _moviment.Codprocli = 0;
                _moviment.Tipo = "REG";
                _moviment.Unidades = cantidad;
                _moviment.Seriedoc = "";
                _moviment.Numdoc = 0;
                _moviment.Seriecompra = "";
                _moviment.Numfaccompra = -1;
                _moviment.Caja = "";
                _moviment.Stock = _stockAnterior;
                _moviment.Pvp = 0;
                _moviment.Codmonedapvp = 1;
                _moviment.Calcmovpost = "F";
                _moviment.Udmedida2 = 0;
                _moviment.Zona = "";
                _moviment.Pvpdmn = null;
                _moviment.Preciodmn = null;
                _moviment.Stock2 = 0;

                _context.Moviments.Add(_moviment);

                _context.SaveChanges();
                return _stock;
            }
            else
            {
                return _stock;
            }
        }

        public StockDto UpdateStockV(int codArticulo, string codAlmacen, double cantidad)
        {
            StockDto _stock = new StockDto();
            // FECHA DE INVENTARIOS
            var tablaInv = DateTime.Now.Date;
            var Hrs = DateTime.Now.Hour;
            var ampm = Hrs >= 12 ? "PM" : "AM";
            var diAsignado = DateTime.Now.Date;

            if (ampm.ToString().Equals("PM"))
            {
                Console.WriteLine("SI ES PM");
                tablaInv = DateTime.Now.Date.AddDays(1);
                diAsignado = DateTime.Now.Date.AddDays(1);
            }
            else
            {
                Console.WriteLine("SI ES AM ");
                tablaInv = DateTime.Now.Date;
                diAsignado = DateTime.Now.Date;
            }

            if (_context.Inventarios.FirstOrDefault(x => x.Codalmacen == codAlmacen && x.Fecha == diAsignado.Date) != null)
            {
                tablaInv = _context.Inventarios.FirstOrDefault(x => x.Codalmacen == codAlmacen && x.Fecha == diAsignado.Date).Fecha;
            }
            else
            {
                if (ampm.ToString().Equals("PM"))
                {
                    tablaInv = DateTime.Now.Date.AddDays(1); ;
                }

            }

            var __stock = _context.Stocks.FirstOrDefault(x => x.Codarticulo == codArticulo && x.Codalmacen == codAlmacen);
            double _stockAnterior = __stock.Stock1.Value;
            if (__stock != null)
            {
                __stock.Stock1 = cantidad;

                _stock.Codalmacen = codAlmacen;
                _stock.Descripcion = _stock.Descripcion;
                _stock.Codarticulo = codArticulo;
                _stock.Regulariza = "T";
                _stock.Unidadessat = "";
                _stock.Stock1 = cantidad;

                _context.Stocks.Update(__stock);

                if (tablaInv != diAsignado)
                {
                    var FechV = diAsignado;

                    Inventario _inventario = new Inventario();

                    _inventario.Fecha = FechV.Date;
                    _inventario.Codalmacen = codAlmacen;
                    _inventario.Tipovaloracion = -3;
                    _inventario.Serie = "";
                    _inventario.Numero = 0;
                    _inventario.Codvendedor = -1;
                    _inventario.Completo = "F";
                    _inventario.Metodo = 1;
                    _inventario.Inicial = "F";
                    _inventario.Bloqueado = "F";
                    _inventario.Tipovaloraciondmn = null;
                    _inventario.Estado = 0;
                    _inventario.Escierre = false;
                    _inventario.EnlaceEjercicio = null;
                    _inventario.EnlaceEmpresa = null;
                    _inventario.EnlaceUsuario = null;
                    _inventario.EnlaceAsiento = null;

                    _context.Inventarios.Add(_inventario);

                }

                DateTime FechVC = diAsignado.Date.AddHours(2);

                Moviment _moviment = new Moviment();
                _moviment.Codalmacenorigen = codAlmacen;
                _moviment.Codalmacendestino = "";
                _moviment.Numserie = "";
                _moviment.Codarticulo = codArticulo;
                _moviment.Talla = ".";
                _moviment.Color = ".";
                _moviment.Precio = _context.Articuloscamposlibres.FirstOrDefault(x => x.Codarticulo == codArticulo)?.Precioproveedor;
                _moviment.Fecha = FechVC.Date;
                _moviment.Hora = Convert.ToDateTime("1899-12-30 " + FechVC.Hour + ":" + FechVC.Minute + ":" + FechVC.Second + ".000");
                _moviment.Codprocli = 0;
                _moviment.Tipo = "REG";
                _moviment.Unidades = cantidad;
                _moviment.Seriedoc = "";
                _moviment.Numdoc = 0;
                _moviment.Seriecompra = "";
                _moviment.Numfaccompra = -1;
                _moviment.Caja = "";
                _moviment.Stock = _stockAnterior;
                _moviment.Pvp = 0;
                _moviment.Codmonedapvp = 1;
                _moviment.Calcmovpost = "F";
                _moviment.Udmedida2 = 0;
                _moviment.Zona = "";
                _moviment.Pvpdmn = null;
                _moviment.Preciodmn = null;
                _moviment.Stock2 = 0;

                _context.Moviments.Add(_moviment);


                _context.SaveChanges();
                return _stock;
            }
            else
            {
                return _stock;
            }
        }


    }
}
