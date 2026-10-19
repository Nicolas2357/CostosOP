using System;
using System.Drawing;
using System.Windows.Forms;

namespace CostosOP
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        // ---------- Paleta ----------
        static readonly Color Navy = Color.FromArgb(17, 24, 39);
        static readonly Color Accent = Color.FromArgb(37, 99, 235);
        static readonly Color Naranja = Color.FromArgb(234, 88, 12);
        static readonly Color Verde = Color.FromArgb(22, 163, 74);
        static readonly Color Gris = Color.FromArgb(107, 114, 128);
        static readonly Color Fondo = Color.FromArgb(243, 244, 246);

        // ---------- Controles ----------
        Panel pnlHeader; Label lblTitulo, lblSub;
        TableLayoutPanel tlpKpi; Label lblKpiInv, lblKpiOrd, lblKpiGI, lblKpiCosto;
        StatusStrip statusStrip1; ToolStripStatusLabel lblEstado;
        TabControl tabs; TabPage tabAlmacen, tabOrdenes, tabGastos, tabResultados;

        // Almacén
        GroupBox gbMaterial, gbEntrada;
        TextBox txtCod, txtNom, txtUni;
        Button btnAltaMat, btnEjemplo, btnEnt;
        ComboBox cmbMat, cmbTipoEnt;
        NumericUpDown nudCantEnt, nudCostoEnt;
        DataGridView dgvKardex, dgvSaldos;

        // Órdenes
        GroupBox gbOrden, gbReq, gbMO;
        NumericUpDown nudNumOrden, nudCantReq, nudMO;
        TextBox txtDesc, txtOperario;
        ComboBox cmbOrdenMP, cmbMatReq, cmbOrdenMO;
        Button btnAltaOP, btnReq, btnMO;
        Label lblCostoProm;
        DataGridView dgvOrdenes, dgvMO;

        // Gastos
        GroupBox gbGasto, gbProrr;
        ComboBox cmbTipoGI, cmbBase;
        TextBox txtDescGI;
        NumericUpDown nudGI;
        Button btnGI, btnPror;
        Label lblPend;
        DataGridView dgvGI, dgvProrrateo;

        // Resultados
        GroupBox gbTerm;
        ComboBox cmbOrdenTerm;
        NumericUpDown nudUnidades;
        Button btnTerm;
        Label lblTotales;
        DataGridView dgvResumen, dgvSaldoFinal;

        // ---------- Helpers de construcción ----------
        static Label Lb(string t, int x, int y)
        { return new Label { Text = t, Left = x, Top = y + 4, AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) }; }
        static TextBox Tx(int x, int y, int w)
        { return new TextBox { Left = x, Top = y, Width = w, BorderStyle = BorderStyle.FixedSingle }; }
        static ComboBox Cb(int x, int y, int w)
        { return new ComboBox { Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Color.White }; }
        static NumericUpDown Nu(int x, int y, int w, int dec)
        { return new NumericUpDown { Left = x, Top = y, Width = w, DecimalPlaces = dec, Minimum = 0, Maximum = 1000000000, ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right }; }
        static Button Bt(string t, int x, int y, int w, Color c)
        {
            var b = new Button { Text = t, Left = x, Top = y, Width = w, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
        static GroupBox Gb(string t, int x, int y, int w, int h)
        { return new GroupBox { Text = t, Left = x, Top = y, Width = w, Height = h, BackColor = Color.White, ForeColor = Navy, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }; }
        static Panel Pn(int h) { return new Panel { Dock = DockStyle.Top, Height = h, BackColor = Color.Transparent }; }
        static Label Tit(string t)
        { return new Label { Text = t, Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Navy }; }
        static TabPage Tab(string t)
        { return new TabPage { Text = t, Padding = new Padding(8), BackColor = Fondo, UseVisualStyleBackColor = false }; }

        static Panel Card(string titulo, Color acento, out Label valor)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6) };
            p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 5, BackColor = acento });
            p.Controls.Add(new Label { Text = titulo, Left = 16, Top = 8, AutoSize = true, ForeColor = Gris });
            valor = new Label { Left = 16, Top = 28, AutoSize = true, Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Navy, Text = "0.00" };
            p.Controls.Add(valor);
            return p;
        }

        static DataGridView Dg(DockStyle d, int h)
        {
            var g = new DataGridView
            {
                Dock = d,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 231, 235),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 34,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            if (h > 0) g.Height = h;
            g.RowTemplate.Height = 27;
            g.ColumnHeadersDefaultCellStyle.BackColor = Navy;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            g.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewColumn c in g.Columns)
                {
                    if (c.ValueType == typeof(decimal) || c.ValueType == typeof(decimal?))
                    {
                        bool cu = c.Name.StartsWith("CostoUnit") || c.Name == "CostoPromedio";
                        c.DefaultCellStyle.Format = cu ? "N4" : "N2";
                        c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    else if (c.ValueType == typeof(DateTime))
                        c.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            };
            return g;
        }

        // ---------- InitializeComponent ----------
        private void InitializeComponent()
        {
            SuspendLayout();
            Font = new Font("Segoe UI", 9F);
            Text = "Sistema de Costos por Órdenes de Producción";
            ClientSize = new Size(1240, 780);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Fondo;

            // ----- Encabezado -----
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Navy };
            lblTitulo = new Label { Text = "SISTEMA DE COSTOS POR ÓRDENES DE PRODUCCIÓN", Left = 20, Top = 9, AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
            lblSub = new Label { Text = "Materia prima  ·  Mano de obra  ·  Gastos indirectos  ·  Artículos terminados", Left = 22, Top = 39, AutoSize = true, ForeColor = Color.FromArgb(156, 163, 175) };
            pnlHeader.Controls.Add(lblTitulo); pnlHeader.Controls.Add(lblSub);

            // ----- KPIs -----
            tlpKpi = new TableLayoutPanel { Dock = DockStyle.Top, Height = 86, ColumnCount = 4, RowCount = 1, Padding = new Padding(6, 6, 6, 0), BackColor = Color.Transparent };
            for (int i = 0; i < 4; i++) tlpKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpKpi.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpKpi.Controls.Add(Card("INVENTARIO FINAL DE MP", Accent, out lblKpiInv), 0, 0);
            tlpKpi.Controls.Add(Card("ÓRDENES ACTIVAS", Verde, out lblKpiOrd), 1, 0);
            tlpKpi.Controls.Add(Card("G.I. PENDIENTES DE PRORRATEAR", Naranja, out lblKpiGI), 2, 0);
            tlpKpi.Controls.Add(Card("COSTO ACUMULADO EN ÓRDENES", Navy, out lblKpiCosto), 3, 0);

            // ----- Barra de estado -----
            statusStrip1 = new StatusStrip { BackColor = Color.White };
            lblEstado = new ToolStripStatusLabel("Listo");
            statusStrip1.Items.Add(lblEstado);

            // ----- Tabs -----
            tabs = new TabControl { Dock = DockStyle.Fill, ItemSize = new Size(250, 32), SizeMode = TabSizeMode.Fixed, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            tabAlmacen = Tab("1. Almacén (Materia Prima)");
            tabOrdenes = Tab("2. Órdenes de Producción");
            tabGastos = Tab("3. Gastos Indirectos y Prorrateo");
            tabResultados = Tab("4. Terminados / Resultados");
            tabs.TabPages.AddRange(new[] { tabAlmacen, tabOrdenes, tabGastos, tabResultados });

            // ===== TAB 1: ALMACÉN =====
            var p1 = Pn(145);
            gbMaterial = Gb("Alta de material", 0, 0, 560, 135);
            txtCod = Tx(80, 26, 110); txtNom = Tx(265, 26, 270); txtUni = Tx(80, 62, 110);
            btnAltaMat = Bt("Agregar material", 265, 60, 170, Accent);
            btnEjemplo = Bt("Cargar datos de ejemplo", 15, 98, 175, Gris);
            gbMaterial.Controls.AddRange(new Control[] { Lb("Código:", 15, 26), txtCod, Lb("Nombre:", 205, 26), txtNom, Lb("Unidad:", 15, 62), txtUni, btnAltaMat, btnEjemplo });

            gbEntrada = Gb("Entrada de inventario (inicial / compra)", 570, 0, 560, 135);
            cmbMat = Cb(80, 26, 250);
            cmbTipoEnt = Cb(385, 26, 150); cmbTipoEnt.Items.AddRange(new object[] { "Inicial", "Compra" }); cmbTipoEnt.SelectedIndex = 0;
            nudCantEnt = Nu(80, 62, 90, 2); nudCostoEnt = Nu(255, 62, 100, 2);
            btnEnt = Bt("Registrar entrada", 385, 60, 150, Accent);
            gbEntrada.Controls.AddRange(new Control[] { Lb("Material:", 15, 26), cmbMat, Lb("Tipo:", 345, 26), cmbTipoEnt, Lb("Cantidad:", 15, 62), nudCantEnt, Lb("Costo unit.:", 185, 62), nudCostoEnt, btnEnt });
            p1.Controls.Add(gbMaterial); p1.Controls.Add(gbEntrada);

            dgvKardex = Dg(DockStyle.Fill, 0);
            dgvSaldos = Dg(DockStyle.Bottom, 150);
            tabAlmacen.Controls.Add(dgvKardex);
            tabAlmacen.Controls.Add(Tit("Saldos e inventario final de materia prima"));
            tabAlmacen.Controls.Add(dgvSaldos);
            tabAlmacen.Controls.Add(p1);

            // ===== TAB 2: ÓRDENES =====
            var p2 = Pn(165);
            gbOrden = Gb("Nueva orden de producción", 0, 0, 420, 150);
            nudNumOrden = Nu(90, 26, 80, 0); nudNumOrden.Minimum = 1; nudNumOrden.Value = 1050; nudNumOrden.ThousandsSeparator = false;
            txtDesc = Tx(90, 62, 310);
            btnAltaOP = Bt("Dar de alta orden", 90, 100, 160, Accent);
            gbOrden.Controls.AddRange(new Control[] { Lb("No. Orden:", 15, 26), nudNumOrden, Lb("Descripción:", 15, 62), txtDesc, btnAltaOP });

            gbReq = Gb("Requisición de materia prima a la orden", 430, 0, 400, 150);
            cmbOrdenMP = Cb(75, 26, 305); cmbMatReq = Cb(75, 58, 305); nudCantReq = Nu(75, 90, 90, 2);
            btnReq = Bt("Asignar MP", 180, 88, 200, Accent);
            lblCostoProm = new Label { Left = 15, Top = 126, AutoSize = true, ForeColor = Accent, Font = new Font("Segoe UI", 8.5F, FontStyle.Regular) };
            gbReq.Controls.AddRange(new Control[] { Lb("Orden:", 15, 26), cmbOrdenMP, Lb("Material:", 15, 58), cmbMatReq, Lb("Cantidad:", 15, 90), nudCantReq, btnReq, lblCostoProm });

            gbMO = Gb("Mano de obra (nómina de operarios)", 840, 0, 330, 150);
            cmbOrdenMO = Cb(75, 26, 235); txtOperario = Tx(75, 58, 235); nudMO = Nu(75, 90, 110, 2);
            btnMO = Bt("Asignar M.O.", 195, 88, 115, Accent);
            gbMO.Controls.AddRange(new Control[] { Lb("Orden:", 15, 26), cmbOrdenMO, Lb("Operario:", 15, 58), txtOperario, Lb("Monto:", 15, 90), nudMO, btnMO });
            p2.Controls.Add(gbOrden); p2.Controls.Add(gbReq); p2.Controls.Add(gbMO);

            dgvOrdenes = Dg(DockStyle.Fill, 0);
            dgvMO = Dg(DockStyle.Bottom, 150);
            tabOrdenes.Controls.Add(dgvOrdenes);
            tabOrdenes.Controls.Add(Tit("Registro de mano de obra"));
            tabOrdenes.Controls.Add(dgvMO);
            tabOrdenes.Controls.Add(p2);

            // ===== TAB 3: GASTOS INDIRECTOS =====
            var p3 = Pn(150);
            gbGasto = Gb("Captura de gasto indirecto", 0, 0, 620, 135);
            cmbTipoGI = Cb(80, 26, 200);
            cmbTipoGI.Items.AddRange(new object[] { "Materia prima indirecta", "Sueldos supervisores", "Luz", "Agua", "Servicios", "Renta", "Depreciación", "Otros" });
            cmbTipoGI.SelectedIndex = 0;
            nudGI = Nu(350, 26, 120, 2); txtDescGI = Tx(80, 62, 390);
            btnGI = Bt("Capturar gasto", 80, 98, 150, Accent);
            gbGasto.Controls.AddRange(new Control[] { Lb("Tipo:", 15, 26), cmbTipoGI, Lb("Monto:", 300, 26), nudGI, Lb("Descripción:", 15, 62), txtDescGI, btnGI });

            gbProrr = Gb("Prorrateo entre órdenes activas", 630, 0, 540, 135);
            cmbBase = Cb(110, 26, 200); cmbBase.Items.AddRange(new object[] { "Mano de obra", "Materia prima", "Partes iguales" }); cmbBase.SelectedIndex = 0;
            btnPror = Bt("PRORRATEAR G.I. A ÓRDENES ACTIVAS", 15, 62, 295, Naranja); btnPror.Height = 34;
            lblPend = new Label { Left = 15, Top = 106, AutoSize = true, ForeColor = Naranja, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            gbProrr.Controls.AddRange(new Control[] { Lb("Base de reparto:", 15, 26), cmbBase, btnPror, lblPend });
            p3.Controls.Add(gbGasto); p3.Controls.Add(gbProrr);

            dgvGI = Dg(DockStyle.Fill, 0);
            dgvProrrateo = Dg(DockStyle.Bottom, 170);
            tabGastos.Controls.Add(dgvGI);
            tabGastos.Controls.Add(Tit("Resultado del último prorrateo"));
            tabGastos.Controls.Add(dgvProrrateo);
            tabGastos.Controls.Add(p3);

            // ===== TAB 4: RESULTADOS =====
            var p4 = Pn(80);
            gbTerm = Gb("Cierre de orden (artículo terminado)", 0, 0, 800, 68);
            cmbOrdenTerm = Cb(65, 26, 250); nudUnidades = Nu(450, 26, 90, 2);
            btnTerm = Bt("Marcar como TERMINADA", 560, 24, 220, Verde);
            gbTerm.Controls.AddRange(new Control[] { Lb("Orden:", 15, 26), cmbOrdenTerm, Lb("Unidades producidas:", 330, 26), nudUnidades, btnTerm });
            p4.Controls.Add(gbTerm);

            dgvResumen = Dg(DockStyle.Fill, 0);
            lblTotales = new Label { Dock = DockStyle.Bottom, Height = 32, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.White, ForeColor = Navy, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            dgvSaldoFinal = Dg(DockStyle.Bottom, 150);
            tabResultados.Controls.Add(dgvResumen);
            tabResultados.Controls.Add(lblTotales);
            tabResultados.Controls.Add(Tit("Saldo final de materia prima en almacén"));
            tabResultados.Controls.Add(dgvSaldoFinal);
            tabResultados.Controls.Add(p4);

            // ----- Orden de acoplamiento (el último agregado se acopla primero) -----
            Controls.Add(tabs);
            Controls.Add(statusStrip1);
            Controls.Add(tlpKpi);
            Controls.Add(pnlHeader);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}