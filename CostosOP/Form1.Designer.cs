using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        // ---------- Paleta clara ----------
        static readonly Color Fondo = Color.FromArgb(244, 246, 248);
        static readonly Color Header = Color.White;
        static readonly Color Panel1 = Color.White;
        static readonly Color Panel2 = Color.FromArgb(238, 242, 246);
        static readonly Color Campo = Color.White;
        static readonly Color Borde = Color.FromArgb(226, 232, 238);
        static readonly Color Texto = Color.FromArgb(31, 41, 51);
        static readonly Color Gris = Color.FromArgb(107, 119, 133);
        static readonly Color Accent = Color.FromArgb(59, 110, 150);
        static readonly Color AccentClaro = Color.FromArgb(70, 130, 169);
        static readonly Color Teal = Color.FromArgb(63, 168, 188);
        static readonly Color Naranja = Color.FromArgb(194, 87, 15);
        static readonly Color NaranjaClaro = Color.FromArgb(224, 123, 57);
        static readonly Color Verde = Color.FromArgb(58, 130, 80);
        static readonly Color VerdeClaro = Color.FromArgb(91, 169, 113);
        static readonly Color Ambar = Color.FromArgb(240, 176, 64);
        static readonly Color BtnGris = Color.FromArgb(100, 116, 139);

        // ---------- Controles (mismos nombres que usan Form1.cs y Form1.Extras.cs) ----------
        Panel pnlHeader, pnlMenu, pnlMain; Label lblTitulo, lblSub, lblPagina;
        TableLayoutPanel tlpMenu; List<Label> itemsMenu = new List<Label>();
        Label lblKpiInv, lblKpiOrd, lblKpiGI, lblKpiCosto;
        Label lblTMat, lblTTerm, lblTMov, lblTGastos;
        StatusStrip statusStrip1; ToolStripStatusLabel lblEstado;
        TabControl tabs; TabPage tabTablero, tabAlmacen, tabOrdenes, tabGastos, tabResultados;

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

        static readonly string[] MenuTextos = { "Tablero", "Almacén", "Órdenes", "Gastos indirectos", "Resultados", "Hoja de costos", "Gráfica", "Correcciones" };
        static readonly string[] MenuTitulos = { "Tablero", "Almacén (materia prima)", "Órdenes de producción", "Gastos indirectos y prorrateo",
            "Resultados y artículos terminados", "Hoja de costos por orden", "Gráfica de costos", "Correcciones" };
        static readonly Font FMenu = new Font("Segoe UI", 10F);
        static readonly Font FMenuSel = new Font("Segoe UI", 10F, FontStyle.Bold);

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
                Margin = new Padding(0, 6, 8, 6),
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
                Margin = new Padding(0, 5, 16, 5)
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
                Margin = new Padding(0, 5, 16, 5)
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
                Margin = new Padding(0, 5, 16, 5)
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
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(c, 0.12f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(c, 0.08f);
            return b;
        }

        // Línea de degradado al pie de las tarjetas y mosaicos
        static Panel Tira(int alto, Color a, Color b)
        {
            var t = new Panel { Dock = DockStyle.Bottom, Height = alto, BackColor = a };
            t.Paint += (s, e) =>
            {
                var r = t.ClientRectangle;
                if (r.Width < 1 || r.Height < 1) return;
                using (var br = new LinearGradientBrush(r, a, b, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(br, r);
            };
            t.Resize += (s, e) => t.Invalidate();
            return t;
        }

        static TableLayoutPanel Frm()
        {
            var t = new TableLayoutPanel
            {
                ColumnCount = 4,
                BackColor = Panel1,
                Padding = new Padding(16, 12, 10, 12),
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
                Margin = new Padding(0, 10, 0, 2),
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

        // Tarjeta estilo mosaico: borde fino, título, contenido y línea de degradado al pie
        static Control Tarjeta(string titulo, Control contenido)
        {
            var p = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Panel1,
                Padding = new Padding(1),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 12, 0)
            };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 4F));
            var h = new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(16, 12, 0, 10),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Texto,
                BackColor = Panel1
            };
            var linea = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Borde };
            var tira = Tira(4, Teal, Accent); tira.Dock = DockStyle.Fill; tira.Margin = new Padding(0);
            contenido.Dock = DockStyle.Fill; contenido.Margin = new Padding(0);
            p.Controls.Add(h, 0, 0); p.Controls.Add(linea, 0, 1); p.Controls.Add(contenido, 0, 2); p.Controls.Add(tira, 0, 3);
            p.Paint += (s, e) => { using (var pen = new Pen(Borde)) e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };
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
                Height = 34,
                Padding = new Padding(2, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Texto,
                BackColor = Color.Transparent
            };
        }
        static Panel Esp() { return new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent }; }
        static TabPage Tab(string t)
        {
            return new TabPage
            {
                Text = t,
                Padding = new Padding(4, 8, 4, 4),
                BackColor = Fondo,
                UseVisualStyleBackColor = false,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
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
                ColumnHeadersHeight = 38,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            if (h > 0) g.Height = h;
            g.RowTemplate.Height = 30;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.BackColor = Panel1;
            g.DefaultCellStyle.ForeColor = Texto;
            g.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 245);
            g.DefaultCellStyle.SelectionForeColor = Texto;
            g.ColumnHeadersDefaultCellStyle.BackColor = Panel2;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(59, 71, 84);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Panel2;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(59, 71, 84);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 252);
            g.AlternatingRowsDefaultCellStyle.ForeColor = Texto;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 245);
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Texto;
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

        // ---------- Mosaicos del tablero ----------
        static Panel Tile(string titulo, out Label valor, Color c1, Color c2)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Panel1, Margin = new Padding(8), Padding = new Padding(1) };
            valor = new Label
            {
                Dock = DockStyle.Fill,
                Text = "0",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 6, 0, 0),
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Texto,
                BackColor = Color.Transparent
            };
            var t = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                Text = titulo,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 6),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                BackColor = Color.Transparent
            };
            p.Controls.Add(valor); p.Controls.Add(t); p.Controls.Add(Tira(4, c1, c2));
            p.Paint += (s, e) => { using (var pen = new Pen(Borde)) e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };
            return p;
        }

        Panel Accion(string simbolo, string texto, int pagina)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Panel1, Margin = new Padding(8), Padding = new Padding(1), Cursor = Cursors.Hand };
            var s1 = new Label
            {
                Dock = DockStyle.Fill,
                Text = simbolo,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = Accent,
                BackColor = Color.Transparent
            };
            var t = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                Text = texto,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 6),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Accent,
                BackColor = Color.Transparent
            };
            var tira = Tira(4, Teal, Accent);
            p.Controls.Add(s1); p.Controls.Add(t); p.Controls.Add(tira);
            p.Paint += (s, e) => { using (var pen = new Pen(Borde)) e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };
            EventHandler ir = (a, b) => { tabs.SelectedIndex = pagina; };
            EventHandler entra = (a, b) => { p.BackColor = Color.FromArgb(238, 245, 250); };
            EventHandler sale = (a, b) => { p.BackColor = Panel1; };
            foreach (Control c in new Control[] { p, s1, t, tira })
            { c.Click += ir; c.MouseEnter += entra; c.MouseLeave += sale; c.Cursor = Cursors.Hand; }
            return p;
        }

        static Label Seccion(string t)
        {
            return new Label
            {
                Text = t,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(8, 0, 0, 6),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Texto,
                BackColor = Color.Transparent
            };
        }

        void ConstruirTablero()
        {
            var g = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, RowCount = 5, BackColor = Color.Transparent, Height = 490 };
            for (int i = 0; i < 4; i++) g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            g.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            g.RowStyles.Add(new RowStyle(SizeType.Absolute, 134F));
            g.RowStyles.Add(new RowStyle(SizeType.Absolute, 134F));
            g.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            g.RowStyles.Add(new RowStyle(SizeType.Absolute, 134F));

            var s1 = Seccion("Resumen general"); g.Controls.Add(s1, 0, 0); g.SetColumnSpan(s1, 4);
            g.Controls.Add(Tile("Inventario final de materia prima", out lblKpiInv, Teal, Accent), 0, 1);
            g.Controls.Add(Tile("Órdenes activas", out lblKpiOrd, VerdeClaro, Accent), 1, 1);
            g.Controls.Add(Tile("G.I. pendientes de prorratear", out lblKpiGI, Ambar, NaranjaClaro), 2, 1);
            g.Controls.Add(Tile("Costo acumulado en órdenes", out lblKpiCosto, Teal, Accent), 3, 1);

            g.Controls.Add(Tile("Materiales registrados", out lblTMat, Teal, Accent), 0, 2);
            g.Controls.Add(Tile("Órdenes terminadas", out lblTTerm, VerdeClaro, Accent), 1, 2);
            g.Controls.Add(Tile("Movimientos de almacén", out lblTMov, Teal, Accent), 2, 2);
            g.Controls.Add(Tile("Gastos sin prorratear", out lblTGastos, Ambar, NaranjaClaro), 3, 2);

            var s2 = Seccion("Acciones rápidas"); g.Controls.Add(s2, 0, 3); g.SetColumnSpan(s2, 4);
            g.Controls.Add(Accion("+", "Registrar compra de material", 1), 0, 4);
            g.Controls.Add(Accion("+", "Nueva orden de producción", 2), 1, 4);
            g.Controls.Add(Accion("+", "Capturar gasto indirecto", 3), 2, 4);
            g.Controls.Add(Accion("→", "Ver resultados", 4), 3, 4);

            tabTablero.AutoScroll = true;
            tabTablero.Controls.Add(g);

            // Los conteos se actualizan solos cuando se refrescan las tablas
            dgvSaldos.DataBindingComplete += (s, e) => { lblTMat.Text = dgvSaldos.Rows.Count.ToString(); };
            dgvKardex.DataBindingComplete += (s, e) => { lblTMov.Text = dgvKardex.Rows.Count.ToString(); };
            dgvResumen.DataBindingComplete += (s, e) =>
            {
                int n = 0;
                foreach (DataGridViewRow r in dgvResumen.Rows)
                    if (Convert.ToString(r.Cells["Estado"].Value) == "Terminada") n++;
                lblTTerm.Text = n.ToString();
            };
            dgvGI.DataBindingComplete += (s, e) =>
            {
                int n = 0;
                foreach (DataGridViewRow r in dgvGI.Rows)
                    if (Convert.ToString(r.Cells["Prorrateado"].Value) == "Pendiente") n++;
                lblTGastos.Text = n.ToString();
            };
        }

        // ---------- Menú lateral (se llama desde Form1.cs después de ConstruirExtras) ----------
        void FilaMenu(Control c)
        {
            int r = tlpMenu.RowStyles.Count;
            tlpMenu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMenu.RowCount = r + 1;
            c.Dock = DockStyle.Fill; c.Margin = new Padding(0);
            tlpMenu.Controls.Add(c, 0, r);
        }

        void ConstruirMenu()
        {
            var grupos = new Dictionary<int, string> { { 0, "PRINCIPAL" }, { 1, "OPERACIÓN" }, { 4, "REPORTES" }, { 7, "AJUSTES" } };
            tlpMenu.SuspendLayout();
            itemsMenu.Clear();
            for (int i = 0; i < tabs.TabPages.Count; i++)
            {
                if (grupos.ContainsKey(i))
                    FilaMenu(new Label
                    {
                        Text = grupos[i],
                        AutoSize = false,
                        Height = 38,
                        TextAlign = ContentAlignment.BottomLeft,
                        Padding = new Padding(24, 0, 0, 6),
                        Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                        ForeColor = Gris,
                        BackColor = Panel1
                    });

                int idx = i;
                var lbl = new Label
                {
                    Text = i < MenuTextos.Length ? MenuTextos[i] : tabs.TabPages[i].Text,
                    AutoSize = false,
                    Height = 42,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(26, 0, 0, 0),
                    Font = FMenu,
                    ForeColor = Texto,
                    BackColor = Panel1,
                    Cursor = Cursors.Hand,
                    Tag = false
                };
                lbl.Click += (s, e) => { tabs.SelectedIndex = idx; };
                lbl.MouseEnter += (s, e) => { if (!(bool)lbl.Tag) lbl.BackColor = Panel2; };
                lbl.MouseLeave += (s, e) => { if (!(bool)lbl.Tag) lbl.BackColor = Panel1; };
                lbl.Paint += (s, e) =>
                {
                    if ((bool)lbl.Tag)
                        using (var br = new SolidBrush(Accent)) e.Graphics.FillRectangle(br, 0, 0, 4, lbl.Height);
                };
                itemsMenu.Add(lbl);
                FilaMenu(lbl);
            }
            tlpMenu.ResumeLayout(true);
            tabs.SelectedIndexChanged += (s, e) => MarcarMenu();
            MarcarMenu();
        }

        void MarcarMenu()
        {
            for (int i = 0; i < itemsMenu.Count; i++)
            {
                bool sel = i == tabs.SelectedIndex;
                var l = itemsMenu[i];
                l.Tag = sel;
                l.BackColor = sel ? Color.FromArgb(225, 236, 246) : Panel1;
                l.ForeColor = sel ? Accent : Texto;
                l.Font = sel ? FMenuSel : FMenu;
                l.Invalidate();
            }
            int k = tabs.SelectedIndex;
            lblPagina.Text = k >= 0 && k < MenuTitulos.Length ? MenuTitulos[k] : (k >= 0 ? tabs.TabPages[k].Text : "");
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

            // ----- Barra superior -----
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Header };
            pnlHeader.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Borde });
            var logo = new Label
            {
                Text = "C",
                Left = 20,
                Top = 13,
                Width = 36,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            };
            lblTitulo = new Label
            {
                Text = "Costos por Órdenes de Producción",
                Left = 68,
                Top = 9,
                AutoSize = true,
                ForeColor = Texto,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            };
            lblSub = new Label
            {
                Text = "Materia prima · Mano de obra · Gastos indirectos",
                Left = 70,
                Top = 34,
                AutoSize = true,
                ForeColor = Gris,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5F)
            };
            pnlHeader.Controls.Add(logo); pnlHeader.Controls.Add(lblTitulo); pnlHeader.Controls.Add(lblSub);

            // ----- Barra de estado -----
            statusStrip1 = new StatusStrip { BackColor = Color.White, ForeColor = Gris, SizingGrip = false };
            lblEstado = new ToolStripStatusLabel("Listo") { ForeColor = Gris };
            statusStrip1.Items.Add(lblEstado);

            // ----- Menú lateral -----
            pnlMenu = new Panel { Dock = DockStyle.Left, Width = 236, BackColor = Panel1 };
            tlpMenu = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                BackColor = Panel1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 8, 0, 0)
            };
            tlpMenu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlMenu.Controls.Add(tlpMenu);
            pnlMenu.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 1, BackColor = Borde });

            // ----- Contenido: título de la sección + páginas -----
            tabs = new PaginasControl { Dock = DockStyle.Fill };
            tabTablero = Tab("Tablero");
            tabAlmacen = Tab("1. Almacén (Materia Prima)");
            tabOrdenes = Tab("2. Órdenes de Producción");
            tabGastos = Tab("3. Gastos Indirectos y Prorrateo");
            tabResultados = Tab("4. Terminados / Resultados");
            tabs.TabPages.AddRange(new[] { tabTablero, tabAlmacen, tabOrdenes, tabGastos, tabResultados });

            lblPagina = new Label
            {
                Dock = DockStyle.Top,
                Height = 50,
                Text = "Tablero",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Texto,
                BackColor = Color.Transparent
            };
            pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(26, 14, 26, 8), BackColor = Fondo };
            pnlMain.Controls.Add(tabs);
            pnlMain.Controls.Add(lblPagina);

            // =================== ALMACÉN ===================
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

            // =================== ÓRDENES ===================
            nudNumOrden = Nu(0); nudNumOrden.Minimum = 1; nudNumOrden.Value = 1050; nudNumOrden.ThousandsSeparator = false;
            txtDesc = Tx();
            btnAltaOP = Bt("Dar de alta orden", 160, Accent);
            var g1 = Frm();
            Fila(g1, "No. Orden:", nudNumOrden);
            Fila(g1, "Descripción:", txtDesc);
            FilaBtn(g1, btnAltaOP);

            cmbOrdenMP = Cb(); cmbMatReq = Cb(); nudCantReq = Nu(2);
            lblCostoProm = new Label { ForeColor = Accent, Font = new Font("Segoe UI", 8.5F) };
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

            // =================== GASTOS INDIRECTOS ===================
            cmbTipoGI = Cb();
            cmbTipoGI.Items.AddRange(new object[] { "Materia prima indirecta", "Sueldos supervisores", "Luz", "Agua", "Servicios", "Renta", "Depreciación", "Otros" });
            cmbTipoGI.SelectedIndex = 0;
            nudGI = Nu(2); txtDescGI = Tx();
            btnGI = Bt("Capturar gasto", 150, Accent);
            var h1 = Frm();
            Fila(h1, "Tipo:", cmbTipoGI, "Monto:", nudGI);
            Fila(h1, "Descripción:", txtDescGI);
            FilaBtn(h1, btnGI);

            cmbBase = Cb(); cmbBase.Items.AddRange(new object[] { "Horas de M.O.", "Mano de obra ($)", "Materia prima", "Partes iguales" }); cmbBase.SelectedIndex = 0;
            btnPror = Bt("PRORRATEAR G.I. A ÓRDENES ACTIVAS", 320, Naranja);
            lblPend = new Label { ForeColor = Naranja, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
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

            // =================== RESULTADOS ===================
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
                Height = 40,
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Panel2,
                ForeColor = Texto,
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

            // =================== TABLERO ===================
            ConstruirTablero();

            // ----- Orden de acoplamiento del formulario -----
            Controls.Add(pnlMain);
            Controls.Add(pnlMenu);
            Controls.Add(statusStrip1);
            Controls.Add(pnlHeader);
            ResumeLayout(false);
            PerformLayout();
        }
    }

    // =====================================================
    //  TabControl sin pestañas visibles (se navega con el menú lateral)
    // =====================================================
    public class PaginasControl : TabControl
    {
        const int TCM_ADJUSTRECT = 0x1328;

        public PaginasControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == TCM_ADJUSTRECT && !DesignMode) { m.Result = (IntPtr)1; return; }
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(244, 246, 248));
        }
    }
}