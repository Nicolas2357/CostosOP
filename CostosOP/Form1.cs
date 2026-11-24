using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CostosOP
{
    public partial class Form1 : Form
    {
        private SistemaCostos sis = new SistemaCostos();

        public Form1()
        {
            InitializeComponent();
            cmbBase.Items.Clear();
            cmbBase.Items.AddRange(new object[] { "Horas de M.O.", "Mano de obra ($)", "Materia prima", "Partes iguales" });
            cmbBase.SelectedIndex = 0;
            ConstruirExtras();
            Conectar();
            ConstruirMenu();
            RefreshAll();
        }

        void Conectar()
        {
            btnAltaMat.Click += (s, e) => Ejecutar(() =>
            { sis.AltaMaterial(txtCod.Text, txtNom.Text, txtUni.Text); txtCod.Clear(); txtNom.Clear(); txtUni.Clear(); }, "Material agregado");

            btnEnt.Click += (s, e) => Ejecutar(() =>
                sis.Entrada(cmbMat.SelectedItem as Material,
                    cmbTipoEnt.SelectedIndex == 0 ? TipoMov.Inicial : TipoMov.Compra,
                    nudCantEnt.Value, nudCostoEnt.Value), "Entrada registrada en la tarjeta de almacén");

            btnEjemplo.Click += (s, e) => Ejecutar(CargarEjemplo,
                "Ejercicio del Excel cargado. Termine la OP 1025 y luego prorratee por Horas de M.O.");

            btnAltaOP.Click += (s, e) => Ejecutar(() =>
            { sis.AltaOrden((int)nudNumOrden.Value, txtDesc.Text); nudNumOrden.Value++; txtDesc.Clear(); }, "Orden dada de alta");

            btnReq.Click += (s, e) => Ejecutar(() =>
                sis.Salida(cmbMatReq.SelectedItem as Material, cmbOrdenMP.SelectedItem as OrdenProduccion, nudCantReq.Value),
                "Materia prima asignada a la orden");

            btnMO.Click += (s, e) => Ejecutar(() =>
                sis.AsignarMO(cmbOrdenMO.SelectedItem as OrdenProduccion, txtOperario.Text, nudMO.Value, nudTarifaMO.Value),
                "Mano de obra asignada");

            cmbMatReq.SelectedIndexChanged += (s, e) => ActualizarCosto();

            btnGI.Click += (s, e) => Ejecutar(() =>
            { sis.CapturarGasto(cmbTipoGI.Text, txtDescGI.Text, nudGI.Value); txtDescGI.Clear(); }, "Gasto indirecto capturado");

            btnPror.Click += (s, e) => Ejecutar(() => sis.Prorratear(cmbBase.Text), "Prorrateo aplicado a las órdenes en proceso");

            btnTerm.Click += (s, e) =>
            {
                var op = cmbOrdenTerm.SelectedItem as OrdenProduccion;
                if (op != null)
                {
                    string aviso = "¿Marcar la orden " + op.Numero + " como TERMINADA?\nYa no podrá recibir más costos.";
                    if (sis.Gastos.Any(g => !g.Prorrateado))
                        aviso += "\n\nOJO: hay gastos indirectos sin prorratear; esta orden NO los recibirá.";
                    if (MessageBox.Show(aviso, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                }
                Ejecutar(() => { sis.Terminar(op, nudUnidades.Value); nudUnidades.Value = 0; }, "Orden terminada y costeada");
            };
        }

        void Ejecutar(Action a, string ok = "Operación realizada")
        {
            try
            {
                a();
                lblEstado.Text = ok + "  (" + DateTime.Now.ToString("HH:mm:ss") + ")";
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Error: " + ex.Message;
                MessageBox.Show(ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            RefreshAll();
        }

        static void Bind(ComboBox c, IList datos)
        {
            object sel = c.SelectedItem;
            c.DataSource = datos;
            if (sel != null && datos.Contains(sel)) c.SelectedItem = sel;
            else c.SelectedIndex = datos.Count > 0 ? 0 : -1;
        }

        static void Pintar(DataGridView g, string col)
        {
            foreach (DataGridViewRow r in g.Rows)
                if (Convert.ToString(r.Cells[col].Value) == "Terminada")
                    r.DefaultCellStyle.ForeColor = Color.FromArgb(46, 125, 50);
        }

        void ActualizarCosto()
        {
            var m = cmbMatReq.SelectedItem as Material;
            lblCostoProm.Text = m == null ? "" :
                "Costo promedio: " + sis.CostoPromedio(m).ToString("N4") + "   |   Existencia: " + sis.Existencia(m).ToString("N2") + " " + m.Unidad;
        }

        void RefreshAll()
        {
            var activas = sis.Ordenes.Where(o => !o.Terminada).ToList();

            Bind(cmbMat, sis.Materiales.ToList());
            Bind(cmbMatReq, sis.Materiales.ToList());
            Bind(cmbOrdenMP, activas.ToList());
            Bind(cmbOrdenMO, activas.ToList());
            Bind(cmbOrdenTerm, activas.ToList());
            ActualizarCosto();

            dgvKardex.DataSource = sis.Movimientos.Select(m => new {
                m.Fecha,
                Material = m.Material.Nombre,
                Tipo = m.Tipo.ToString(),
                m.Cantidad,
                CostoUnit = m.CostoUnitario,
                m.Importe,
                Orden = m.Orden
            }).ToList();
            var saldos = sis.Materiales.Select(m => new {
                m.Codigo,
                m.Nombre,
                m.Unidad,
                Existencia = sis.Existencia(m),
                CostoPromedio = sis.CostoPromedio(m),
                Saldo = sis.Saldo(m)
            }).ToList();
            dgvSaldos.DataSource = saldos;
            dgvSaldoFinal.DataSource = saldos;

            dgvOrdenes.DataSource = sis.Ordenes.Select(o => new {
                o.Numero,
                o.Descripcion,
                o.Cliente,
                Estado = o.Terminada ? "Terminada" : "En proceso",
                MP = o.MateriaPrima,
                MO = o.ManoObra,
                GI = o.GastosIndirectos,
                Total = o.CostoTotal
            }).ToList();
            Pintar(dgvOrdenes, "Estado");
            dgvMO.DataSource = sis.ManoObra.Select(r => new { r.Fecha, r.Orden, r.Operario, r.Horas, r.Tarifa, r.Monto }).ToList();

            dgvGI.DataSource = sis.Gastos.Select(g => new {
                g.Fecha,
                g.Tipo,
                g.Descripcion,
                g.Monto,
                Prorrateado = g.Prorrateado ? "Sí" : "Pendiente"
            }).ToList();
            dgvProrrateo.DataSource = sis.UltimoProrrateo.ToList();
            decimal pend = sis.Gastos.Where(g => !g.Prorrateado).Sum(g => g.Monto);
            lblPend.Text = "Pendiente de prorratear: $" + pend.ToString("N2");

            dgvResumen.DataSource = sis.Ordenes.Select(o => new {
                Orden = o.Numero,
                o.Descripcion,
                Estado = o.Terminada ? "Terminada" : "En proceso",
                MP = o.MateriaPrima,
                MO = o.ManoObra,
                GI = o.GastosIndirectos,
                CostoTotal = o.CostoTotal,
                o.Unidades,
                CostoUnitario = o.CostoUnitario
            }).ToList();
            Pintar(dgvResumen, "Estado");

            decimal invFinal = sis.Materiales.Sum(m => sis.Saldo(m));
            decimal costoTot = sis.Ordenes.Sum(o => o.CostoTotal);
            lblTotales.Text = string.Format("   TOTALES  →  MP: {0:N2}  |  MO: {1:N2}  |  GI: {2:N2}  |  Costo total: {3:N2}  |  Inventario final MP: {4:N2}",
                sis.Ordenes.Sum(o => o.MateriaPrima), sis.Ordenes.Sum(o => o.ManoObra),
                sis.Ordenes.Sum(o => o.GastosIndirectos), costoTot, invFinal);

            lblKpiInv.Text = "$ " + invFinal.ToString("N2");
            lblKpiOrd.Text = activas.Count.ToString();
            lblKpiGI.Text = "$ " + pend.ToString("N2");
            lblKpiCosto.Text = "$ " + costoTot.ToString("N2");

            RefreshExtras();
        }

        // ============ EJERCICIO DE TU EXCEL (Ejercicio_Ordenes_3) ============
        void CargarEjemplo()
        {
            if (sis.Materiales.Any() || sis.Ordenes.Any()) throw new InvalidOperationException("Ya hay datos cargados.");
            foreach (var n in new[] { "A", "B", "C", "D" }) sis.AltaMaterial("MAT-" + n, "Material " + n, "pza");
            var a = sis.Materiales[0]; var b = sis.Materiales[1]; var c = sis.Materiales[2]; var d = sis.Materiales[3];
            sis.Entrada(a, TipoMov.Inicial, 300, 50); sis.Entrada(a, TipoMov.Compra, 250, 65);
            sis.Entrada(b, TipoMov.Inicial, 450, 75); sis.Entrada(b, TipoMov.Compra, 150, 80);
            sis.Entrada(c, TipoMov.Inicial, 320, 30); sis.Entrada(c, TipoMov.Compra, 280, 32);
            sis.Entrada(d, TipoMov.Inicial, 500, 35);

            sis.AltaOrden(1025, "Juguetes"); sis.AltaOrden(1026, "Juguetes"); sis.AltaOrden(1027, "Juegos didácticos");
            var o25 = sis.Ordenes[0]; var o26 = sis.Ordenes[1]; var o27 = sis.Ordenes[2];
            o25.Cliente = "JUGUETES DEL NORTE"; o26.Cliente = "JUGUETILOMAS"; o27.Cliente = "JUEGOS DIDACTICOS DEL OESTE";
            o25.SI_MP = 48000; o25.SI_MO = 17000; o25.SI_GI = 13500;

            sis.Salida(a, o25, 50); sis.Salida(d, o25, 30);
            sis.Salida(a, o26, 100); sis.Salida(c, o26, 150);
            sis.Salida(b, o27, 300);
            sis.Entrada(b, TipoMov.Compra, 250, 82);
            sis.Entrada(d, TipoMov.Compra, 100, 36.5m);
            sis.Salida(d, o26, 70);
            sis.Entrada(a, TipoMov.Compra, 100, 66);
            sis.Salida(a, o26, 50);
            sis.Entrada(c, TipoMov.Compra, 300, 34);
            sis.Salida(c, o27, 150);
            sis.Salida(d, o27, 50);

            sis.AsignarMO(o25, "Operarios", 50, 30);
            sis.AsignarMO(o26, "Operarios", 100, 30); sis.AsignarMO(o26, "Operarios", 125, 30);
            sis.AsignarMO(o27, "Operarios", 150, 20); sis.AsignarMO(o27, "Operarios", 250, 20);

            sis.CapturarGasto("Sueldos supervisores", "Gasto indirecto 1", 15000);
            sis.CapturarGasto("Luz", "Gasto indirecto 2", 5000);
            sis.CapturarGasto("Servicios", "Gasto indirecto 3", 3800);
            sis.CapturarGasto("Sueldos supervisores", "Gasto indirecto 4", 15000);
            sis.Recalcular();
        }
    }
}