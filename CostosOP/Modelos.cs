using System;
using System.Collections.Generic;
using System.Linq;

namespace CostosOP
{
    public enum TipoMov { Inicial, Compra, Salida }

    public class Material
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        public override string ToString() { return Codigo + " - " + Nombre; }
    }

    // Una fila de la Tarjeta de Almacén (kardex)
    public class MovimientoAlmacen
    {
        public DateTime Fecha { get; set; }
        public Material Material { get; set; }
        public TipoMov Tipo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public int? Orden { get; set; }          // solo en salidas
        public decimal Importe { get { return Math.Round(Cantidad * CostoUnitario, 2); } }
    }

    public class OrdenProduccion
    {
        public int Numero { get; set; }
        public string Descripcion { get; set; }
        public bool Terminada { get; set; }
        public decimal Unidades { get; set; }
        public decimal MateriaPrima { get; set; }
        public decimal ManoObra { get; set; }
        public decimal GastosIndirectos { get; set; }
        public decimal CostoTotal { get { return MateriaPrima + ManoObra + GastosIndirectos; } }
        public decimal CostoUnitario { get { return Unidades > 0 ? Math.Round(CostoTotal / Unidades, 4) : 0; } }
        public override string ToString() { return "OP " + Numero + " - " + Descripcion; }
    }

    public class RegistroMO
    {
        public DateTime Fecha { get; set; }
        public int Orden { get; set; }
        public string Operario { get; set; }
        public decimal Monto { get; set; }
    }

    public class GastoIndirecto
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public bool Prorrateado { get; set; }
    }

    public class DetalleProrrateo
    {
        public int Orden { get; set; }
        public decimal Base { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Importe { get; set; }
    }

    // Lógica de negocio de todo el sistema
    public class SistemaCostos
    {
        public List<Material> Materiales = new List<Material>();
        public List<MovimientoAlmacen> Movimientos = new List<MovimientoAlmacen>();
        public List<OrdenProduccion> Ordenes = new List<OrdenProduccion>();
        public List<RegistroMO> ManoObra = new List<RegistroMO>();
        public List<GastoIndirecto> Gastos = new List<GastoIndirecto>();
        public List<DetalleProrrateo> UltimoProrrateo = new List<DetalleProrrateo>();

        // ---------- ALMACÉN ----------
        public void AltaMaterial(string cod, string nom, string uni)
        {
            if (string.IsNullOrWhiteSpace(cod) || string.IsNullOrWhiteSpace(nom))
                throw new InvalidOperationException("Código y nombre son obligatorios.");
            if (Materiales.Any(m => m.Codigo.Equals(cod, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Ya existe ese código de material.");
            Materiales.Add(new Material { Codigo = cod.Trim(), Nombre = nom.Trim(), Unidad = uni.Trim() });
        }

        public decimal Existencia(Material m)
        {
            return Movimientos.Where(x => x.Material == m)
                .Sum(x => x.Tipo == TipoMov.Salida ? -x.Cantidad : x.Cantidad);
        }

        public decimal Saldo(Material m)
        {
            return Movimientos.Where(x => x.Material == m)
                .Sum(x => x.Tipo == TipoMov.Salida ? -x.Importe : x.Importe);
        }

        // Costo promedio ponderado
        public decimal CostoPromedio(Material m)
        {
            decimal e = Existencia(m);
            return e <= 0 ? 0 : Math.Round(Saldo(m) / e, 4);
        }

        public void Entrada(Material m, TipoMov tipo, decimal cant, decimal costo)
        {
            if (m == null) throw new InvalidOperationException("Seleccione un material.");
            if (tipo == TipoMov.Salida) throw new InvalidOperationException("Tipo inválido.");
            if (cant <= 0 || costo <= 0) throw new InvalidOperationException("Cantidad y costo deben ser mayores a 0.");
            Movimientos.Add(new MovimientoAlmacen { Fecha = DateTime.Now, Material = m, Tipo = tipo, Cantidad = cant, CostoUnitario = costo });
        }

        // Salida/requisición de MP hacia una O.P.
        public void Salida(Material m, OrdenProduccion op, decimal cant)
        {
            if (m == null || op == null) throw new InvalidOperationException("Seleccione material y orden.");
            if (op.Terminada) throw new InvalidOperationException("La orden ya está terminada.");
            if (cant <= 0) throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
            if (cant > Existencia(m)) throw new InvalidOperationException("Existencia insuficiente. Disponible: " + Existencia(m));
            var mov = new MovimientoAlmacen
            {
                Fecha = DateTime.Now,
                Material = m,
                Tipo = TipoMov.Salida,
                Cantidad = cant,
                CostoUnitario = CostoPromedio(m),
                Orden = op.Numero
            };
            Movimientos.Add(mov);
            op.MateriaPrima += mov.Importe;
        }

        // ---------- ÓRDENES ----------
        public void AltaOrden(int num, string desc)
        {
            if (Ordenes.Any(o => o.Numero == num)) throw new InvalidOperationException("La orden ya existe.");
            Ordenes.Add(new OrdenProduccion { Numero = num, Descripcion = desc });
        }

        public void AsignarMO(OrdenProduccion op, string operario, decimal monto)
        {
            if (op == null) throw new InvalidOperationException("Seleccione una orden.");
            if (op.Terminada) throw new InvalidOperationException("La orden ya está terminada.");
            if (monto <= 0) throw new InvalidOperationException("El monto debe ser mayor a 0.");
            ManoObra.Add(new RegistroMO { Fecha = DateTime.Now, Orden = op.Numero, Operario = operario, Monto = monto });
            op.ManoObra += monto;
        }

        // ---------- GASTOS INDIRECTOS ----------
        public void CapturarGasto(string tipo, string desc, decimal monto)
        {
            if (monto <= 0) throw new InvalidOperationException("El monto debe ser mayor a 0.");
            Gastos.Add(new GastoIndirecto { Fecha = DateTime.Now, Tipo = tipo, Descripcion = desc, Monto = monto });
        }

        // Prorrateo: reparte los G.I. pendientes entre las órdenes activas
        public List<DetalleProrrateo> Prorratear(string baseCalculo)
        {
            var pend = Gastos.Where(g => !g.Prorrateado).ToList();
            if (!pend.Any()) throw new InvalidOperationException("No hay gastos indirectos pendientes de prorratear.");
            var act = Ordenes.Where(x => !x.Terminada).ToList();
            if (!act.Any()) throw new InvalidOperationException("No hay órdenes activas.");

            decimal total = pend.Sum(g => g.Monto);
            Func<OrdenProduccion, decimal> baseFn;
            if (baseCalculo == "Materia prima") baseFn = x => x.MateriaPrima;
            else if (baseCalculo == "Partes iguales") baseFn = x => 1m;
            else baseFn = x => x.ManoObra;

            decimal sumaBase = act.Sum(baseFn);
            if (sumaBase <= 0) { baseFn = x => 1m; sumaBase = act.Count; } // respaldo

            var res = new List<DetalleProrrateo>();
            decimal asignado = 0;
            for (int i = 0; i < act.Count; i++)
            {
                var ord = act[i];
                decimal imp = (i == act.Count - 1)
                    ? total - asignado                       // último absorbe el redondeo
                    : Math.Round(total * baseFn(ord) / sumaBase, 2);
                asignado += imp;
                ord.GastosIndirectos += imp;
                res.Add(new DetalleProrrateo
                {
                    Orden = ord.Numero,
                    Base = baseFn(ord),
                    Porcentaje = Math.Round(baseFn(ord) / sumaBase * 100, 2),
                    Importe = imp
                });
            }
            foreach (var g in pend) g.Prorrateado = true;
            UltimoProrrateo = res;
            return res;
        }

        // ---------- TERMINADOS ----------
        public void Terminar(OrdenProduccion op, decimal unidades)
        {
            if (op == null) throw new InvalidOperationException("Seleccione una orden.");
            if (unidades <= 0) throw new InvalidOperationException("Indique las unidades producidas.");
            if (Gastos.Any(g => !g.Prorrateado))
                throw new InvalidOperationException("Hay gastos indirectos sin prorratear. Ejecute el prorrateo primero.");
            op.Unidades = unidades;
            op.Terminada = true;
        }
    }
}