using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CostosOP
{
    public enum TipoMov { Inicial, Compra, Salida }

    public class Material
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Unidad { get; set; } = "";
        public override string ToString() { return Codigo + " - " + Nombre; }
    }

    public class MovimientoAlmacen
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string CodMat { get; set; } = "";
        [JsonIgnore] public Material Material { get; set; }
        public TipoMov Tipo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public int? Orden { get; set; }
        [JsonIgnore] public decimal Importe { get { return Math.Round(Cantidad * CostoUnitario, 2, MidpointRounding.AwayFromZero); } }
    }

    public class OrdenProduccion
    {
        public int Numero { get; set; }
        public string Descripcion { get; set; } = "";   // Artículo
        public string Cliente { get; set; } = "";
        public string Condiciones { get; set; } = "";
        public decimal PrecioVenta { get; set; }
        public DateTime? FechaPedido { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime? FechaTermino { get; set; }
        public bool Terminada { get; set; }
        public decimal Unidades { get; set; }
        public decimal SI_MP { get; set; }
        public decimal SI_MO { get; set; }
        public decimal SI_GI { get; set; }
        public decimal MateriaPrima { get; set; }
        public decimal ManoObra { get; set; }
        public decimal GastosIndirectos { get; set; }
        [JsonIgnore] public decimal CostoTotal { get { return MateriaPrima + ManoObra + GastosIndirectos; } }
        [JsonIgnore] public decimal CostoUnitario { get { return Unidades > 0 ? Math.Round(CostoTotal / Unidades, 4) : 0; } }
        public override string ToString() { return "OP " + Numero + " - " + Descripcion; }
    }

    public class RegistroMO
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int Orden { get; set; }
        public string Operario { get; set; } = "";
        public decimal Horas { get; set; }
        public decimal Tarifa { get; set; }
        [JsonIgnore] public decimal Monto { get { return Math.Round(Horas * Tarifa, 2, MidpointRounding.AwayFromZero); } }
    }

    public class GastoIndirecto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Monto { get; set; }
        public int? Lote { get; set; }
        [JsonIgnore] public bool Prorrateado { get { return Lote.HasValue; } }
    }

    public class DetalleProrrateo
    {
        public int Orden { get; set; }
        public decimal Base { get; set; }
        public decimal Tasa { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Importe { get; set; }
    }

    public class Lote
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string BaseNombre { get; set; } = "";
        public decimal TotalGI { get; set; }
        public decimal SumaBase { get; set; }
        public decimal Tasa { get; set; }
        public List<DetalleProrrateo> Detalle { get; set; } = new List<DetalleProrrateo>();
    }

    // Filas de reportes (no se guardan en el JSON)
    public class FilaHoja
    {
        public int Id; public DateTime Fecha; public string Concepto = "";
        public decimal? MPCant, MPPrecio, MPTotal, MOCant, MOPrecio, MOTotal, GICant, GIPrecio, GITotal;
        public decimal Total;
    }

    public class FilaTarjeta
    {
        public DateTime Fecha; public string Ref = "";
        public decimal? Entradas, Salidas, PrecioUnit, PrecioNeto, Debe, Haber;
        public decimal Existencia, Saldo;
    }

    public class MovGeneral
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = "";
        public string Detalle { get; set; } = "";
        public int? Orden { get; set; }
        public decimal Importe { get; set; }
    }

    public class SistemaCostos
    {
        public int Contador { get; set; }
        public List<Material> Materiales { get; set; } = new List<Material>();
        public List<MovimientoAlmacen> Movimientos { get; set; } = new List<MovimientoAlmacen>();
        public List<OrdenProduccion> Ordenes { get; set; } = new List<OrdenProduccion>();
        public List<RegistroMO> ManoObra { get; set; } = new List<RegistroMO>();
        public List<GastoIndirecto> Gastos { get; set; } = new List<GastoIndirecto>();
        public List<Lote> Lotes { get; set; } = new List<Lote>();

        [JsonIgnore]
        public List<DetalleProrrateo> UltimoProrrateo
        { get { return Lotes.Count == 0 ? new List<DetalleProrrateo>() : Lotes[Lotes.Count - 1].Detalle; } }

        int Sig() { return ++Contador; }

        // ---------- GUARDAR / ABRIR ----------
        static readonly JsonSerializerOptions Opc = new JsonSerializerOptions
        { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };

        public void Guardar(string ruta) { File.WriteAllText(ruta, JsonSerializer.Serialize(this, Opc)); }

        public static SistemaCostos Abrir(string ruta)
        {
            var s = JsonSerializer.Deserialize<SistemaCostos>(File.ReadAllText(ruta), Opc);
            if (s == null) throw new InvalidOperationException("El archivo está vacío o no es válido.");
            foreach (var m in s.Movimientos)
                m.Material = s.Materiales.First(x => x.Codigo == m.CodMat);
            s.Recalcular();
            return s;
        }

        // ---------- RECÁLCULO (costos de salidas y totales de cada orden) ----------
        public void Recalcular()
        {
            var exi = new Dictionary<string, decimal>();
            var sal = new Dictionary<string, decimal>();
            foreach (var mv in Movimientos)
            {
                decimal e, s;
                exi.TryGetValue(mv.CodMat, out e); sal.TryGetValue(mv.CodMat, out s);
                if (mv.Tipo == TipoMov.Salida)
                {
                    if (mv.Cantidad > e) throw new InvalidOperationException("La existencia de " + mv.CodMat + " quedaría negativa.");
                    mv.CostoUnitario = Math.Round(s / e, 4);
                    e -= mv.Cantidad; s -= mv.Importe;
                }
                else { e += mv.Cantidad; s += mv.Importe; }
                exi[mv.CodMat] = e; sal[mv.CodMat] = s;
            }
            foreach (var o in Ordenes)
            {
                o.MateriaPrima = o.SI_MP + Movimientos.Where(x => x.Tipo == TipoMov.Salida && x.Orden == o.Numero).Sum(x => x.Importe);
                o.ManoObra = o.SI_MO + ManoObra.Where(x => x.Orden == o.Numero).Sum(x => x.Monto);
                o.GastosIndirectos = o.SI_GI + Lotes.SelectMany(l => l.Detalle).Where(d => d.Orden == o.Numero).Sum(d => d.Importe);
            }
        }

        // ---------- ALMACÉN ----------
        public void AltaMaterial(string cod, string nom, string uni)
        {
            if (string.IsNullOrWhiteSpace(cod) || string.IsNullOrWhiteSpace(nom))
                throw new InvalidOperationException("Código y nombre son obligatorios.");
            if (Materiales.Any(m => m.Codigo.Equals(cod.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Ya existe ese código de material.");
            Materiales.Add(new Material { Codigo = cod.Trim(), Nombre = nom.Trim(), Unidad = (uni ?? "").Trim() });
        }

        public decimal Existencia(Material m)
        { return Movimientos.Where(x => x.CodMat == m.Codigo).Sum(x => x.Tipo == TipoMov.Salida ? -x.Cantidad : x.Cantidad); }

        public decimal Saldo(Material m)
        { return Movimientos.Where(x => x.CodMat == m.Codigo).Sum(x => x.Tipo == TipoMov.Salida ? -x.Importe : x.Importe); }

        public decimal CostoPromedio(Material m)
        { decimal e = Existencia(m); return e <= 0 ? 0 : Math.Round(Saldo(m) / e, 4); }

        public void Entrada(Material m, TipoMov tipo, decimal cant, decimal costo)
        {
            if (m == null) throw new InvalidOperationException("Seleccione un material.");
            if (tipo == TipoMov.Salida) throw new InvalidOperationException("Tipo inválido.");
            if (cant <= 0 || costo <= 0) throw new InvalidOperationException("Cantidad y costo deben ser mayores a 0.");
            Movimientos.Add(new MovimientoAlmacen { Id = Sig(), Fecha = DateTime.Now, Material = m, CodMat = m.Codigo, Tipo = tipo, Cantidad = cant, CostoUnitario = costo });
        }

        public void Salida(Material m, OrdenProduccion op, decimal cant)
        {
            if (m == null || op == null) throw new InvalidOperationException("Seleccione material y orden.");
            if (op.Terminada) throw new InvalidOperationException("La orden ya está terminada.");
            if (cant <= 0) throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
            if (cant > Existencia(m)) throw new InvalidOperationException("Existencia insuficiente. Disponible: " + Existencia(m));
            Movimientos.Add(new MovimientoAlmacen { Id = Sig(), Fecha = DateTime.Now, Material = m, CodMat = m.Codigo, Tipo = TipoMov.Salida, Cantidad = cant, CostoUnitario = CostoPromedio(m), Orden = op.Numero });
            Recalcular();
        }

        // ---------- ÓRDENES ----------
        public void AltaOrden(int num, string desc)
        {
            if (Ordenes.Any(o => o.Numero == num)) throw new InvalidOperationException("La orden ya existe.");
            Ordenes.Add(new OrdenProduccion { Numero = num, Descripcion = (desc ?? "").Trim(), FechaInicio = DateTime.Today });
        }

        public void AsignarMO(OrdenProduccion op, string operario, decimal horas, decimal tarifa)
        {
            if (op == null) throw new InvalidOperationException("Seleccione una orden.");
            if (op.Terminada) throw new InvalidOperationException("La orden ya está terminada.");
            if (horas <= 0 || tarifa <= 0) throw new InvalidOperationException("Horas y tarifa deben ser mayores a 0.");
            ManoObra.Add(new RegistroMO { Id = Sig(), Fecha = DateTime.Now, Orden = op.Numero, Operario = (operario ?? "").Trim(), Horas = horas, Tarifa = tarifa });
            Recalcular();
        }

        // ---------- GASTOS INDIRECTOS Y PRORRATEO ----------
        public void CapturarGasto(string tipo, string desc, decimal monto)
        {
            if (monto <= 0) throw new InvalidOperationException("El monto debe ser mayor a 0.");
            Gastos.Add(new GastoIndirecto { Id = Sig(), Fecha = DateTime.Now, Tipo = tipo, Descripcion = (desc ?? "").Trim(), Monto = monto });
        }

        // Tasa = total G.I. / total de la base;  G.I. de la orden = base de la orden x tasa
        public List<DetalleProrrateo> Prorratear(string baseCalculo)
        {
            var pend = Gastos.Where(g => !g.Prorrateado).ToList();
            if (!pend.Any()) throw new InvalidOperationException("No hay gastos indirectos pendientes de prorratear.");
            var act = Ordenes.Where(x => !x.Terminada).ToList();
            if (!act.Any()) throw new InvalidOperationException("No hay órdenes en proceso.");

            decimal total = pend.Sum(g => g.Monto);
            Func<OrdenProduccion, decimal> baseFn;
            switch (baseCalculo)
            {
                case "Horas de M.O.": baseFn = x => ManoObra.Where(r => r.Orden == x.Numero).Sum(r => r.Horas); break;
                case "Mano de obra ($)": baseFn = x => x.ManoObra; break;
                case "Materia prima": baseFn = x => x.MateriaPrima; break;
                default: baseFn = x => 1m; break;
            }
            decimal suma = act.Sum(baseFn);
            string nombre = baseCalculo;
            if (suma <= 0) { baseFn = x => 1m; suma = act.Count; nombre = "Partes iguales (respaldo)"; }

            var lote = new Lote { Id = Sig(), Fecha = DateTime.Now, BaseNombre = nombre, TotalGI = total, SumaBase = suma, Tasa = Math.Round(total / suma, 4) };
            decimal asignado = 0;
            for (int i = 0; i < act.Count; i++)
            {
                var ord = act[i];
                decimal imp = (i == act.Count - 1) ? total - asignado : Math.Round(total * baseFn(ord) / suma, 2, MidpointRounding.AwayFromZero);
                asignado += imp;
                lote.Detalle.Add(new DetalleProrrateo { Orden = ord.Numero, Base = baseFn(ord), Tasa = lote.Tasa, Porcentaje = Math.Round(baseFn(ord) / suma * 100, 2), Importe = imp });
            }
            Lotes.Add(lote);
            foreach (var g in pend) g.Lote = lote.Id;
            Recalcular();
            return lote.Detalle;
        }

        // ---------- TERMINADOS ----------
        public void Terminar(OrdenProduccion op, decimal unidades)
        {
            if (op == null) throw new InvalidOperationException("Seleccione una orden.");
            if (unidades <= 0) throw new InvalidOperationException("Indique las unidades producidas.");
            op.Unidades = unidades; op.Terminada = true; op.FechaTermino = DateTime.Now;
        }

        public void Reabrir(OrdenProduccion op)
        {
            if (op == null) throw new InvalidOperationException("Seleccione una orden terminada.");
            op.Terminada = false; op.Unidades = 0; op.FechaTermino = null;
        }

        // ---------- CANCELAR MOVIMIENTOS ----------
        bool OrdenTerminada(int n) { return Ordenes.Any(o => o.Numero == n && o.Terminada); }

        public string Cancelar(int id)
        {
            var mv = Movimientos.FirstOrDefault(x => x.Id == id);
            if (mv != null)
            {
                if (mv.Orden.HasValue && OrdenTerminada(mv.Orden.Value))
                    throw new InvalidOperationException("La orden " + mv.Orden + " está terminada. Reábrala primero.");
                int i = Movimientos.IndexOf(mv);
                Movimientos.RemoveAt(i);
                try { Recalcular(); }
                catch { Movimientos.Insert(i, mv); Recalcular(); throw; }
                return "Movimiento de almacén cancelado";
            }
            var mo = ManoObra.FirstOrDefault(x => x.Id == id);
            if (mo != null)
            {
                if (OrdenTerminada(mo.Orden)) throw new InvalidOperationException("La orden " + mo.Orden + " está terminada. Reábrala primero.");
                ManoObra.Remove(mo); Recalcular(); return "Mano de obra cancelada";
            }
            var gi = Gastos.FirstOrDefault(x => x.Id == id);
            if (gi != null)
            {
                if (gi.Prorrateado) throw new InvalidOperationException("Ese gasto ya fue prorrateado. Cancele primero el prorrateo #" + gi.Lote + ".");
                Gastos.Remove(gi); return "Gasto indirecto cancelado";
            }
            var lo = Lotes.FirstOrDefault(x => x.Id == id);
            if (lo != null)
            {
                foreach (var d in lo.Detalle)
                    if (OrdenTerminada(d.Orden)) throw new InvalidOperationException("La orden " + d.Orden + " está terminada. Reábrala primero.");
                foreach (var g in Gastos.Where(g => g.Lote == lo.Id)) g.Lote = null;
                Lotes.Remove(lo); Recalcular(); return "Prorrateo cancelado; los gastos quedaron pendientes";
            }
            throw new InvalidOperationException("Seleccione un movimiento.");
        }

        public List<MovGeneral> Bitacora()
        {
            var l = new List<MovGeneral>();
            foreach (var m in Movimientos)
                l.Add(new MovGeneral
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    Orden = m.Orden,
                    Importe = m.Importe,
                    Tipo = m.Tipo == TipoMov.Salida ? "Requisición MP" : (m.Tipo == TipoMov.Compra ? "Compra" : "Inventario inicial"),
                    Detalle = m.Material.Nombre + " x " + m.Cantidad.ToString("N2") + " a " + m.CostoUnitario.ToString("N4")
                });
            foreach (var r in ManoObra)
                l.Add(new MovGeneral
                {
                    Id = r.Id,
                    Fecha = r.Fecha,
                    Orden = r.Orden,
                    Importe = r.Monto,
                    Tipo = "Mano de obra",
                    Detalle = r.Operario + " - " + r.Horas.ToString("N2") + " h x " + r.Tarifa.ToString("N2")
                });
            foreach (var g in Gastos)
                l.Add(new MovGeneral
                {
                    Id = g.Id,
                    Fecha = g.Fecha,
                    Importe = g.Monto,
                    Tipo = "Gasto indirecto",
                    Detalle = g.Tipo + " - " + g.Descripcion + (g.Prorrateado ? " (prorrateo #" + g.Lote + ")" : " (pendiente)")
                });
            foreach (var p in Lotes)
                l.Add(new MovGeneral
                {
                    Id = p.Id,
                    Fecha = p.Fecha,
                    Importe = p.TotalGI,
                    Tipo = "Prorrateo",
                    Detalle = "Base: " + p.BaseNombre + " | Tasa " + p.Tasa.ToString("N4")
                });
            return l.OrderByDescending(x => x.Id).ToList();
        }

        // ---------- REPORTES ----------
        // Hoja de costos de la orden (formato de tu Excel)
        public List<FilaHoja> HojaCostos(OrdenProduccion op)
        {
            var f = new List<FilaHoja>();
            if (op.SI_MP + op.SI_MO + op.SI_GI != 0)
                f.Add(new FilaHoja { Id = 0, Fecha = op.FechaInicio ?? DateTime.Today, Concepto = "SI", MPTotal = op.SI_MP, MOTotal = op.SI_MO, GITotal = op.SI_GI });
            foreach (var m in Movimientos.Where(x => x.Tipo == TipoMov.Salida && x.Orden == op.Numero))
                f.Add(new FilaHoja { Id = m.Id, Fecha = m.Fecha, Concepto = m.Material.Nombre, MPCant = m.Cantidad, MPPrecio = m.CostoUnitario, MPTotal = m.Importe });
            foreach (var r in ManoObra.Where(x => x.Orden == op.Numero))
                f.Add(new FilaHoja { Id = r.Id, Fecha = r.Fecha, Concepto = r.Operario, MOCant = r.Horas, MOPrecio = r.Tarifa, MOTotal = r.Monto });
            foreach (var l in Lotes)
                foreach (var d in l.Detalle.Where(x => x.Orden == op.Numero))
                    f.Add(new FilaHoja { Id = l.Id, Fecha = l.Fecha, Concepto = "Prorrateo G.I.", GICant = d.Base, GIPrecio = d.Tasa, GITotal = d.Importe });
            f = f.OrderBy(x => x.Id).ToList();
            decimal acum = 0;
            foreach (var x in f) { acum += (x.MPTotal ?? 0) + (x.MOTotal ?? 0) + (x.GITotal ?? 0); x.Total = acum; }
            return f;
        }

        // Tarjeta de almacén de un material (formato de tu Excel)
        public List<FilaTarjeta> TarjetaAlmacen(Material m)
        {
            var l = new List<FilaTarjeta>();
            decimal e = 0, s = 0;
            foreach (var x in Movimientos.Where(v => v.CodMat == m.Codigo))
            {
                var f = new FilaTarjeta { Fecha = x.Fecha };
                if (x.Tipo == TipoMov.Salida)
                { f.Ref = "OP " + x.Orden; f.Salidas = x.Cantidad; f.PrecioNeto = x.CostoUnitario; f.Haber = x.Importe; e -= x.Cantidad; s -= x.Importe; }
                else
                {
                    f.PrecioUnit = x.CostoUnitario;
                    if (x.Tipo == TipoMov.Compra) { f.Ref = "Compra"; f.Entradas = x.Cantidad; f.Debe = x.Importe; } else f.Ref = "SI";
                    e += x.Cantidad; s += x.Importe;
                }
                f.Existencia = e; f.Saldo = s; l.Add(f);
            }
            return l;
        }
    }
}