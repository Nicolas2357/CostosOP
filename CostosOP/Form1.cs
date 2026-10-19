using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CostosOP
{
    public partial class Form1 : Form
    {
        private readonly SistemaCostos sis = new SistemaCostos();

        public Form1()
        {
            InitializeComponent();
            Conectar();
            RefreshAll();
        }

        // ============ EVENTOS ============
        void Conectar()
        {
            btnAltaMat.Click += (s, e) => Ejecutar(() =>
            { sis.AltaMaterial(txtCod.Text, txtNom.Text, txtUni.Text); txtCod.Clear(); txtNom.Clear(); txtUni.Clear(); }, "Material agregado");

            btnEnt.Click += (s, e) => Ejecutar(() =>
                sis.Entrada(cmbMat.SelectedItem as Material,
                    cmbTipoEnt.SelectedIndex == 0 ? TipoMov.Inicial : TipoMov.Compra,
                    nudCantEnt.Value, nudCostoEnt.Value), "Entrada registrada en la tarjeta de almacén");

            btnEjemplo.Click += (s, e) => Ejecutar(CargarEjemplo, "Datos de ejemplo cargados");

            btnAltaOP.Click += (s, e) => Ejecutar(() =>
            { sis.AltaOrden((int)nudNumOrden.Value, txtDesc.Text); nudNumOrden.Value++; txtDesc.Clear(); }, "Orden dada de alta");

            btnReq.Click += (s, e) => Ejecutar(() =>
                sis.Salida(cmbMatReq.SelectedItem as Material, cmbOrdenMP.SelectedItem as OrdenProduccion, nudCantReq.Value),
                "Materia prima asignada a la orden");

            btnMO.Click += (s, e) => Ejecutar(() =>
                sis.AsignarMO(cmbOrdenMO.SelectedItem as OrdenProduccion, txtOperario.Text, nudMO.Value), "Mano de obra asignada");

            cmbMatReq.SelectedIndexChanged += (s, e) => ActualizarCosto();

            btnGI.Click += (s, e) => Ejecutar(() =>
            { sis.CapturarGasto(cmbTipoGI.Text, txtDescGI.Text, nudGI.Value); txtDescGI.Clear(); }, "Gasto indirecto capturado");

            btnPror.Click += (s, e) => Ejecutar(() => sis.Prorratear(cmbBase.Text), "Prorrateo aplicado a las órdenes activas");

            btnTerm.Click += (s, e) =>
            {
                var op = cmbOrdenTerm.SelectedItem as OrdenProduccion;
                if (op != null && MessageBox.Show("¿Marcar la orden " + op.Numero + " como TERMINADA?\nYa no podrá recibir más costos.",
                        "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
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

        // ============ ENLACE DATOS -> CONTROLES ============
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
                    r.DefaultCellStyle.ForeColor = Color.FromArgb(21, 128, 61);
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

            // Almacén
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

            // Órdenes
            dgvOrdenes.DataSource = sis.Ordenes.Select(o => new {
                o.Numero,
                o.Descripcion,
                Estado = o.Terminada ? "Terminada" : "En proceso",
                MP = o.MateriaPrima,
                MO = o.ManoObra,
                GI = o.GastosIndirectos,
                Total = o.CostoTotal
            }).ToList();
            Pintar(dgvOrdenes, "Estado");
            dgvMO.DataSource = sis.ManoObra.ToList();

            // Gastos
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

            // Resultados
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

            // KPIs
            lblKpiInv.Text = "$ " + invFinal.ToString("N2");
            lblKpiOrd.Text = activas.Count.ToString();
            lblKpiGI.Text = "$ " + pend.ToString("N2");
            lblKpiCosto.Text = "$ " + costoTot.ToString("N2");
        }

        // ============ DATOS DE EJEMPLO ============
        void CargarEjemplo()
        {
            if (sis.Materiales.Any()) throw new InvalidOperationException("Ya hay datos cargados.");
            sis.AltaMaterial("MP-01", "Madera (tabla)", "pza");
            sis.AltaMaterial("MP-02", "Tela", "m");
            var a = sis.Materiales[0]; var b = sis.Materiales[1];
            sis.Entrada(a, TipoMov.Inicial, 500, 40);
            sis.Entrada(b, TipoMov.Inicial, 300, 25);
            sis.Entrada(a, TipoMov.Compra, 200, 44);
            sis.AltaOrden(1050, "Sillas ejecutivas");
            sis.AltaOrden(1051, "Sofás");
            sis.AltaOrden(1052, "Mesas");
            sis.Salida(a, sis.Ordenes[0], 150); sis.Salida(b, sis.Ordenes[0], 80);
            sis.Salida(a, sis.Ordenes[1], 200); sis.Salida(b, sis.Ordenes[1], 120);
            sis.Salida(a, sis.Ordenes[2], 100);
            sis.AsignarMO(sis.Ordenes[0], "Juan Pérez", 8000);
            sis.AsignarMO(sis.Ordenes[1], "Luis Gómez", 12000);
            sis.AsignarMO(sis.Ordenes[2], "Ana Ruiz", 5000);
            sis.CapturarGasto("Sueldos supervisores", "Supervisor de planta", 9000);
            sis.CapturarGasto("Luz", "Recibo CFE", 3500);
            sis.CapturarGasto("Materia prima indirecta", "Pegamento, clavos", 1500);
        }
    }
}