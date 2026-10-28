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

        // ---------- Paleta oscura ----------
        static readonly Color Fondo = Color.FromArgb(18, 22, 33);
        static readonly Color Header = Color.FromArgb(10, 14, 23);
        static readonly Color Panel1 = Color.FromArgb(30, 36, 51);
        static readonly Color Panel2 = Color.FromArgb(38, 46, 66);
        static readonly Color Campo = Color.FromArgb(17, 22, 34);
        static readonly Color Borde = Color.FromArgb(55, 65, 85);
        static readonly Color Texto = Color.FromArgb(229, 231, 235);
        static readonly Color Gris = Color.FromArgb(148, 163, 184);
        static readonly Color Accent = Color.FromArgb(37, 99, 235);
        static readonly Color AccentClaro = Color.FromArgb(96, 165, 250);
        static readonly Color Naranja = Color.FromArgb(234, 88, 12);
        static readonly Color NaranjaClaro = Color.FromArgb(251, 146, 60);
        static readonly Color Verde = Color.FromArgb(22, 163, 74);
        static readonly Color VerdeClaro = Color.FromArgb(74, 222, 128);
        static readonly Color BtnGris = Color.FromArgb(71, 85, 105);

        // ---------- Controles (mismos nombres que usa Form1.cs) ----------
        Panel pnlHeader, pnlCuerpo; Label lblTitulo, lblSub;
        TableLayoutPanel tlpKpi; Label lblKpiInv, lblKpiOrd, lblKpiGI, lblKpiCosto;
        StatusStrip statusStrip1; ToolStripStatusLabel lblEstado;
        TabControl tabs; TabPage tabAlmacen, tabOrdenes, tabGastos, tabResultados;

        TextBox txtCod, txtNom, txtUni;
        Button btnAltaMat, btnEjemplo, btnEnt;
        ComboBox cmbMat, cmbTipoEnt;
        NumericUpDown nudCantEnt, nudCostoEnt;
        DataGridView dgvKardex, dgvSaldos;

        NumericUpDown nudNumOrden, nudCantReq, nudMO, nudTarifaMO;
        TextBox txtDesc, txtOperario;
        ComboBox cmbOrdenMP, cmbMatReq, cmbOrdenMO;
        Button btnAltaOP, btnReq, btnMO;
        Label lblCostoProm;
        DataGridView dgvOrdenes, dgvMO;

        ComboBox cmbTipoGI, cmbBase;
        TextBox txtDescGI;
        NumericUpDown nudGI;
        Button btnGI, btnPror;
        Label lblPend;
        DataGridView dgvGI, dgvProrrateo;

        ComboBox cmbOrdenTerm;
        NumericUpDown nudUnidades;
        Button btnTerm;
        Label lblTotales;
        DataGridView dgvResumen, dgvSaldoFinal;

        // =====================================================
        //  HELPERS DE CONSTRUCCIÓN
        // =====================================================
        static Label Etq(string t)
        {
            return new Label
            {
                Text = t,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 4, 8, 4),
                ForeColor = Gris,
                BackColor = Color.Transparent
            };
        }
        static TextBox Tx()
        {
            return new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Campo,
                ForeColor = Texto,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 4, 14, 4)
            };
        }
        static ComboBox Cb()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Campo,
                ForeColor = Texto,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 4, 14, 4)
            };
        }
        static NumericUpDown Nu(int dec)
        {
            return new NumericUpDown
            {
                DecimalPlaces = dec,
                Minimum = 0,
                Maximum = 1000000000,
                ThousandsSeparator = true,
                BackColor = Campo,
                ForeColor = Texto,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 4, 14, 4)
            };
        }
        static Button Bt(string t, int w, Color c)
        {
            var b = new Button
            {
                Text = t,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(w, 34),
                Padding = new Padding(14, 0, 14, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = c,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(c, 0.15f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(c, 0.1f);
            return b;
        }

        static TableLayoutPanel Frm()
        {
            var t = new TableLayoutPanel
            {
                ColumnCount = 4,
                BackColor = Panel1,
                Padding = new Padding(14, 10, 6, 10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0)
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            return t;
        }
        static void Fila(TableLayoutPanel t, string l1, Control c1, string l2 = null, Control c2 = null)
        {
            int r = t.RowStyles.Count;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowCount = r + 1;
            t.Controls.Add(Etq(l1), 0, r);
            t.Controls.Add(c1, 1, r);
            if (c2 == null) t.SetColumnSpan(c1, 3);
            else { t.Controls.Add(Etq(l2), 2, r); t.Controls.Add(c2, 3, r); }
        }
        static void FilaBtn(TableLayoutPanel t, params Control[] cs)
        {
            int r = t.RowStyles.Count;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowCount = r + 1;
            var f = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 2),
                BackColor = Color.Transparent
            };
            foreach (var c in cs) { c.Margin = new Padding(0, 0, 10, 0); f.Controls.Add(c); }
            t.Controls.Add(f, 0, r);
            t.SetColumnSpan(f, 4);
        }
        static void FilaSola(TableLayoutPanel t, Label l)
        {
            int r = t.RowStyles.Count;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowCount = r + 1;
            l.AutoSize = true; l.Anchor = AnchorStyles.Left; l.Margin = new Padding(0, 6, 0, 4);
            l.BackColor = Color.Transparent;
            t.Controls.Add(l, 0, r);
            t.SetColumnSpan(l, 4);
        }
        static Control Tarjeta(string titulo, Control contenido)
        {
            var p = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Panel1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 10, 0)
            };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 2F));
            p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var h = new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(14, 9, 0, 9),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Panel2
            };
            var linea = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Accent };
            contenido.Dock = DockStyle.Fill; contenido.Margin = new Padding(0);
            p.Controls.Add(h, 0, 0); p.Controls.Add(linea, 0, 1); p.Controls.Add(contenido, 0, 2);
            return p;
        }
        static TableLayoutPanel Barra(params Control[] cards)
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = cards.Length,
                RowCount = 1,
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            for (int i = 0; i < cards.Length; i++)
            {
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / cards.Length));
                if (i == cards.Length - 1) cards[i].Margin = new Padding(0);
                t.Controls.Add(cards[i], i, 0);
            }
            return t;
        }
        static Label Tit(string t, DockStyle d)
        {
            return new Label
            {
                Text = t,
                Dock = d,
                Height = 32,
                Padding = new Padding(2, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Texto,
                BackColor = Color.Transparent
            };
        }
        static Panel Esp() { return new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent }; }
        static TabPage Tab(string t)
        {
            return new TabPage
            {
                Text = t,
                Padding = new Padding(10),
                BackColor = Fondo,
                UseVisualStyleBackColor = false,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
        }
        static Panel Kpi(string titulo, Color acento, out Label valor)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Panel1, Margin = new Padding(4) };
            p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 5, BackColor = acento });
            p.Controls.Add(new Label
            {
                Text = titulo,
                Left = 16,
                Top = 9,
                AutoSize = true,
                ForeColor = Gris,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5F)
            });
            valor = new Label
            {
                Left = 16,
                Top = 31,
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "0.00"
            };
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
                BackgroundColor = Panel1,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Borde,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 36,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            if (h > 0) g.Height = h;
            g.RowTemplate.Height = 28;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.BackColor = Panel1;
            g.DefaultCellStyle.ForeColor = Texto;
            g.DefaultCellStyle.SelectionBackColor = Accent;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.BackColor = Header;
            g.ColumnHeadersDefaultCellStyle.ForeColor = AccentClaro;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Header;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = AccentClaro;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 42, 59);
            g.AlternatingRowsDefaultCellStyle.ForeColor = Texto;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Accent;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
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

        // =====================================================
        //  INITIALIZE COMPONENT
        // =====================================================
        private void InitializeComponent()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            SuspendLayout();
            Font = new Font("Segoe UI", 9F);
            Text = "Sistema de Costos por Órdenes de Producción";
            ClientSize = new Size(1280, 740);
            MinimumSize = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Fondo;
            ForeColor = Texto;

            // ----- Encabezado -----
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Header };
            pnlHeader.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = Accent });
            lblTitulo = new Label
            {
                Text = "SISTEMA DE COSTOS POR ÓRDENES DE PRODUCCIÓN",
                Left = 20,
                Top = 9,
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };
            lblSub = new Label
            {
                Text = "Materia prima  ·  Mano de obra  ·  Gastos indirectos  ·  Artículos terminados",
                Left = 22,
                Top = 40,
                AutoSize = true,
                ForeColor = Gris,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblTitulo); pnlHeader.Controls.Add(lblSub);

            // ----- KPIs -----
            tlpKpi = new TableLayoutPanel { Dock = DockStyle.Top, Height = 92, ColumnCount = 4, RowCount = 1, Padding = new Padding(6, 8, 6, 0), BackColor = Color.Transparent };
            for (int i = 0; i < 4; i++) tlpKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpKpi.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpKpi.Controls.Add(Kpi("INVENTARIO FINAL DE MP", AccentClaro, out lblKpiInv), 0, 0);
            tlpKpi.Controls.Add(Kpi("ÓRDENES ACTIVAS", VerdeClaro, out lblKpiOrd), 1, 0);
            tlpKpi.Controls.Add(Kpi("G.I. PENDIENTES DE PRORRATEAR", NaranjaClaro, out lblKpiGI), 2, 0);
            tlpKpi.Controls.Add(Kpi("COSTO ACUMULADO EN ÓRDENES", Color.FromArgb(167, 139, 250), out lblKpiCosto), 3, 0);

            // ----- Barra de estado -----
            statusStrip1 = new StatusStrip { BackColor = Header, ForeColor = Gris, SizingGrip = false };
            lblEstado = new ToolStripStatusLabel("Listo") { ForeColor = Gris };
            statusStrip1.Items.Add(lblEstado);

            // ----- Tabs -----
            tabs = new DarkTabControl { Dock = DockStyle.Fill, Padding = new Point(22, 8), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            tabAlmacen = Tab("1. Almacén (Materia Prima)");
            tabOrdenes = Tab("2. Órdenes de Producción");
            tabGastos = Tab("3. Gastos Indirectos y Prorrateo");
            tabResultados = Tab("4. Terminados / Resultados");
            tabs.TabPages.AddRange(new[] { tabAlmacen, tabOrdenes, tabGastos, tabResultados });

            pnlCuerpo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 4, 10, 8), BackColor = Color.Transparent };
            pnlCuerpo.Controls.Add(tabs);

            // =================== TAB 1: ALMACÉN ===================
            txtCod = Tx(); txtNom = Tx(); txtUni = Tx();
            btnAltaMat = Bt("Agregar material", 150, Accent);
            btnEjemplo = Bt("Cargar datos de ejemplo", 190, BtnGris);
            var f1 = Frm();
            Fila(f1, "Código:", txtCod, "Unidad:", txtUni);
            Fila(f1, "Nombre:", txtNom);
            FilaBtn(f1, btnAltaMat, btnEjemplo);

            cmbMat = Cb(); cmbTipoEnt = Cb();
            cmbTipoEnt.Items.AddRange(new object[] { "Inicial", "Compra" }); cmbTipoEnt.SelectedIndex = 0;
            nudCantEnt = Nu(2); nudCostoEnt = Nu(2);
            btnEnt = Bt("Registrar entrada", 160, Accent);
            var f2 = Frm();
            Fila(f2, "Material:", cmbMat, "Tipo:", cmbTipoEnt);
            Fila(f2, "Cantidad:", nudCantEnt, "Costo unit.:", nudCostoEnt);
            FilaBtn(f2, btnEnt);

            var barra1 = Barra(Tarjeta("Alta de material", f1), Tarjeta("Entrada de inventario (inicial / compra)", f2));
            dgvKardex = Dg(DockStyle.Fill, 0);
            dgvSaldos = Dg(DockStyle.Bottom, 120);
            tabAlmacen.Controls.Add(dgvKardex);
            tabAlmacen.Controls.Add(Tit("Saldos e inventario final de materia prima", DockStyle.Bottom));
            tabAlmacen.Controls.Add(dgvSaldos);
            tabAlmacen.Controls.Add(Tit("Tarjeta de almacén (kardex)", DockStyle.Top));
            tabAlmacen.Controls.Add(Esp());
            tabAlmacen.Controls.Add(barra1);

            // =================== TAB 2: ÓRDENES ===================
            nudNumOrden = Nu(0); nudNumOrden.Minimum = 1; nudNumOrden.Value = 1050; nudNumOrden.ThousandsSeparator = false;
            txtDesc = Tx();
            btnAltaOP = Bt("Dar de alta orden", 160, Accent);
            var g1 = Frm();
            Fila(g1, "No. Orden:", nudNumOrden);
            Fila(g1, "Descripción:", txtDesc);
            FilaBtn(g1, btnAltaOP);

            cmbOrdenMP = Cb(); cmbMatReq = Cb(); nudCantReq = Nu(2);
            lblCostoProm = new Label { ForeColor = AccentClaro, Font = new Font("Segoe UI", 8.5F) };
            btnReq = Bt("Asignar MP a la orden", 180, Accent);
            var g2 = Frm();
            Fila(g2, "Orden:", cmbOrdenMP);
            Fila(g2, "Material:", cmbMatReq);
            Fila(g2, "Cantidad:", nudCantReq);
            FilaSola(g2, lblCostoProm);
            FilaBtn(g2, btnReq);

            cmbOrdenMO = Cb(); txtOperario = Tx(); nudMO = Nu(2); nudTarifaMO = Nu(2);
            btnMO = Bt("Asignar M.O.", 140, Accent);
            var g3 = Frm();
            Fila(g3, "Orden:", cmbOrdenMO);
            Fila(g3, "Operario:", txtOperario);
            Fila(g3, "Horas:", nudMO, "Tarifa/hora:", nudTarifaMO);
            FilaBtn(g3, btnMO);

            var barra2 = Barra(Tarjeta("Nueva orden de producción", g1), Tarjeta("Requisición de materia prima", g2), Tarjeta("Mano de obra (nómina de operarios)", g3));
            dgvOrdenes = Dg(DockStyle.Fill, 0);
            dgvMO = Dg(DockStyle.Bottom, 105);
            tabOrdenes.Controls.Add(dgvOrdenes);
            tabOrdenes.Controls.Add(Tit("Registro de mano de obra", DockStyle.Bottom));
            tabOrdenes.Controls.Add(dgvMO);
            tabOrdenes.Controls.Add(Tit("Órdenes de producción", DockStyle.Top));
            tabOrdenes.Controls.Add(Esp());
            tabOrdenes.Controls.Add(barra2);

            // =================== TAB 3: GASTOS INDIRECTOS ===================
            cmbTipoGI = Cb();
            cmbTipoGI.Items.AddRange(new object[] { "Materia prima indirecta", "Sueldos supervisores", "Luz", "Agua", "Servicios", "Renta", "Depreciación", "Otros" });
            cmbTipoGI.SelectedIndex = 0;
            nudGI = Nu(2); txtDescGI = Tx();
            btnGI = Bt("Capturar gasto", 150, Accent);
            var h1 = Frm();
            Fila(h1, "Tipo:", cmbTipoGI, "Monto:", nudGI);
            Fila(h1, "Descripción:", txtDescGI);
            FilaBtn(h1, btnGI);

            cmbBase = Cb(); cmbBase.Items.AddRange(new object[] { "Mano de obra", "Materia prima", "Partes iguales" }); cmbBase.SelectedIndex = 0;
            btnPror = Bt("PRORRATEAR G.I. A ÓRDENES ACTIVAS", 320, Naranja);
            lblPend = new Label { ForeColor = NaranjaClaro, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            var h2 = Frm();
            Fila(h2, "Base de reparto:", cmbBase);
            FilaBtn(h2, btnPror);
            FilaSola(h2, lblPend);

            var barra3 = Barra(Tarjeta("Captura de gasto indirecto", h1), Tarjeta("Prorrateo entre órdenes activas", h2));
            dgvGI = Dg(DockStyle.Fill, 0);
            dgvProrrateo = Dg(DockStyle.Bottom, 130);
            tabGastos.Controls.Add(dgvGI);
            tabGastos.Controls.Add(Tit("Resultado del último prorrateo", DockStyle.Bottom));
            tabGastos.Controls.Add(dgvProrrateo);
            tabGastos.Controls.Add(Tit("Gastos indirectos capturados", DockStyle.Top));
            tabGastos.Controls.Add(Esp());
            tabGastos.Controls.Add(barra3);

            // =================== TAB 4: RESULTADOS ===================
            cmbOrdenTerm = Cb(); nudUnidades = Nu(2);
            btnTerm = Bt("Marcar como TERMINADA", 220, Verde);
            var k1 = Frm();
            Fila(k1, "Orden:", cmbOrdenTerm, "Unidades producidas:", nudUnidades);
            FilaBtn(k1, btnTerm);
            var barra4 = Barra(Tarjeta("Cierre de orden (artículo terminado)", k1),
                               new Panel { BackColor = Color.Transparent, Margin = new Padding(0) });

            dgvResumen = Dg(DockStyle.Fill, 0);
            lblTotales = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                Padding = new Padding(10, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Panel2,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            dgvSaldoFinal = Dg(DockStyle.Bottom, 110);
            tabResultados.Controls.Add(dgvResumen);
            tabResultados.Controls.Add(lblTotales);
            tabResultados.Controls.Add(Tit("Saldo final de materia prima en almacén", DockStyle.Bottom));
            tabResultados.Controls.Add(dgvSaldoFinal);
            tabResultados.Controls.Add(Tit("Costo por orden de producción", DockStyle.Top));
            tabResultados.Controls.Add(Esp());
            tabResultados.Controls.Add(barra4);

            // ----- Orden de acoplamiento del formulario -----
            Controls.Add(pnlCuerpo);
            Controls.Add(statusStrip1);
            Controls.Add(tlpKpi);
            Controls.Add(pnlHeader);
            ResumeLayout(false);
            PerformLayout();
        }
    }

    // =====================================================
    //  TabControl con dibujo propio (tema oscuro)
    // =====================================================
    public class DarkTabControl : TabControl
    {
        public DarkTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.FromArgb(18, 22, 33));
            for (int i = 0; i < TabCount; i++)
            {
                Rectangle r = GetTabRect(i);
                bool sel = i == SelectedIndex;
                using (var br = new SolidBrush(sel ? Color.FromArgb(30, 36, 51) : Color.FromArgb(24, 29, 43)))
                    g.FillRectangle(br, r);
                if (sel)
                    using (var br = new SolidBrush(Color.FromArgb(37, 99, 235)))
                        g.FillRectangle(br, r.Left, r.Top, r.Width, 3);
                TextRenderer.DrawText(g, TabPages[i].Text, Font, r,
                    sel ? Color.White : Color.FromArgb(148, 163, 184),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }
    }
}