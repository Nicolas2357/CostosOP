using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CostosOP
{
    public partial class Form1
    {
        TabPage tabHoja, tabGrafica, tabCorr;
        ComboBox cmbHojaOrden, cmbReabrir;
        TextBox txtCliente, txtArticulo, txtCondiciones;
        NumericUpDown nudPrecioVenta, nudSIMP, nudSIMO, nudSIGI;
        DateTimePicker dtpPedido, dtpInicio, dtpEntrega;
        Button btnGuardarOrden, btnVista, btnImprimir, btnCancelarMov, btnReabrir;
        DataGridView dgvHoja, dgvBitacora;
        Label lblHojaTot;
        GraficaCostos grafica;
        int _filaImp;

        static DateTimePicker Dtp()
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 4, 14, 4)
            };
        }

        void ConstruirExtras()
        {
            // ---------- Botones Guardar / Abrir / Exportar en el encabezado ----------
            var bGuardar = Bt("Guardar", 100, Accent);
            var bAbrir = Bt("Abrir", 90, BtnGris);
            var bExcel = Bt("Exportar a Excel", 150, Verde);
            var tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 16, 14, 0)
            };
            foreach (var b in new[] { bGuardar, bAbrir, bExcel }) { b.Margin = new Padding(8, 0, 0, 0); tools.Controls.Add(b); }
            pnlHeader.Controls.Add(tools); tools.BringToFront();
            bGuardar.Click += (s, e) => GuardarArchivo();
            bAbrir.Click += (s, e) => AbrirArchivo();
            bExcel.Click += (s, e) => ExportarExcel();

            // Títulos más cortos para que quepan 7 pestañas
            tabAlmacen.Text = "1. Almacén"; tabOrdenes.Text = "2. Órdenes";
            tabGastos.Text = "3. Gastos Indirectos"; tabResultados.Text = "4. Resultados";
            dgvProrrateo.DataBindingComplete += (s, e) =>
            { var c = dgvProrrateo.Columns["Tasa"]; if (c != null) c.DefaultCellStyle.Format = "N4"; };

            // ---------- TAB 5: HOJA DE COSTOS ----------
            tabHoja = Tab("5. Hoja de costos");
            cmbHojaOrden = Cb(); txtCliente = Tx(); txtArticulo = Tx(); txtCondiciones = Tx();
            nudPrecioVenta = Nu(2); nudSIMP = Nu(2); nudSIMO = Nu(2); nudSIGI = Nu(2);
            dtpPedido = Dtp(); dtpInicio = Dtp(); dtpEntrega = Dtp();
            btnGuardarOrden = Bt("Guardar datos de la orden", 220, Accent);
            btnVista = Bt("Vista previa", 120, BtnGris);
            btnImprimir = Bt("Imprimir", 110, BtnGris);
            var fh = Frm();
            Fila(fh, "Orden:", cmbHojaOrden);
            Fila(fh, "Cliente:", txtCliente);
            Fila(fh, "Artículo:", txtArticulo);
            Fila(fh, "Precio de venta:", nudPrecioVenta);
            Fila(fh, "Condiciones:", txtCondiciones);
            Fila(fh, "Fecha de pedido:", dtpPedido);
            Fila(fh, "Fecha de inicio:", dtpInicio);
            Fila(fh, "Fecha de entrega:", dtpEntrega);
            Fila(fh, "S.I. materia prima:", nudSIMP);
            Fila(fh, "S.I. mano de obra:", nudSIMO);
            Fila(fh, "S.I. gastos indirectos:", nudSIGI);
            FilaBtn(fh, btnGuardarOrden);
            FilaBtn(fh, btnVista, btnImprimir);
            var tarjHoja = Tarjeta("Datos de la orden (encabezado y saldo inicial)", fh);
            tarjHoja.Dock = DockStyle.Top; tarjHoja.Margin = new Padding(0);
            var izq = new Panel { Dock = DockStyle.Left, Width = 430, AutoScroll = true, BackColor = Color.Transparent };
            izq.Controls.Add(tarjHoja);

            dgvHoja = Dg(DockStyle.Fill, 0);
            lblHojaTot = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                Padding = new Padding(10, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Panel2,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
        
            var der = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 0, 0, 0), BackColor = Color.Transparent };
            der.Controls.Add(dgvHoja); der.Controls.Add(lblHojaTot);
            der.Controls.Add(Tit("Hoja de costos de la orden", DockStyle.Top));
            tabHoja.Controls.Add(der); tabHoja.Controls.Add(izq);

            dgvHoja.DataBindingComplete += (s, e) =>
            {
                var enc = new Dictionary<string, string> {
                    {"MP_Cant","MP cant."},{"MP_Precio","MP precio"},{"MP_Total","MP total"},
                    {"MO_Cant","MO horas"},{"MO_Precio","MO tarifa"},{"MO_Total","MO total"},
                    {"GI_Cant","GI base"},{"GI_Precio","GI tasa"},{"GI_Total","GI total"} };
                foreach (var kv in enc) { var c = dgvHoja.Columns[kv.Key]; if (c != null) c.HeaderText = kv.Value; }
                foreach (var n in new[] { "MP_Precio", "MO_Precio", "GI_Precio" })
                { var c = dgvHoja.Columns[n]; if (c != null) c.DefaultCellStyle.Format = "N4"; }
            };

            cmbHojaOrden.SelectedIndexChanged += (s, e) => CargarHoja();
            btnVista.Click += (s, e) => Imprimir(true);
            btnImprimir.Click += (s, e) => Imprimir(false);
            btnGuardarOrden.Click += (s, e) => Ejecutar(() =>
            {
                var op = cmbHojaOrden.SelectedItem as OrdenProduccion;
                if (op == null) throw new InvalidOperationException("Seleccione una orden.");
                op.Cliente = txtCliente.Text.Trim(); op.Descripcion = txtArticulo.Text.Trim();
                op.Condiciones = txtCondiciones.Text.Trim(); op.PrecioVenta = nudPrecioVenta.Value;
                op.FechaPedido = dtpPedido.Checked ? dtpPedido.Value.Date : (DateTime?)null;
                op.FechaInicio = dtpInicio.Checked ? dtpInicio.Value.Date : (DateTime?)null;
                op.FechaEntrega = dtpEntrega.Checked ? dtpEntrega.Value.Date : (DateTime?)null;
                op.SI_MP = nudSIMP.Value; op.SI_MO = nudSIMO.Value; op.SI_GI = nudSIGI.Value;
                sis.Recalcular();
            }, "Datos de la orden guardados");

            // ---------- TAB 6: GRÁFICA ----------
            tabGrafica = Tab("6. Gráfica");
            grafica = new GraficaCostos { Dock = DockStyle.Fill };
            tabGrafica.Controls.Add(grafica);
            tabGrafica.Controls.Add(Tit("Costo por orden de producción (MP + MO + GI)", DockStyle.Top));

            // ---------- TAB 7: CORRECCIONES ----------
            tabCorr = Tab("7. Correcciones");
            dgvBitacora = Dg(DockStyle.Fill, 0);
            btnCancelarMov = Bt("Cancelar movimiento seleccionado", 260, Color.FromArgb(220, 38, 38));
            var lAyuda = new Label { ForeColor = Gris, Text = "Elija una fila de la tabla y presione el botón. Saldos y costos se recalculan." };
            var c1 = Frm(); FilaSola(c1, lAyuda); FilaBtn(c1, btnCancelarMov);
            cmbReabrir = Cb(); btnReabrir = Bt("Reabrir orden", 150, Naranja);
            var c2 = Frm(); Fila(c2, "Orden terminada:", cmbReabrir); FilaBtn(c2, btnReabrir);
            var barra = Barra(Tarjeta("Cancelar un movimiento", c1), Tarjeta("Reabrir una orden terminada", c2));
            tabCorr.Controls.Add(dgvBitacora);
            tabCorr.Controls.Add(Tit("Bitácora de movimientos (más recientes primero)", DockStyle.Top));
            tabCorr.Controls.Add(Esp());
            tabCorr.Controls.Add(barra);

            btnCancelarMov.Click += (s, e) =>
            {
                if (dgvBitacora.CurrentRow == null) { MessageBox.Show("Seleccione una fila.", "Aviso"); return; }
                int id = Convert.ToInt32(dgvBitacora.CurrentRow.Cells["Id"].Value);
                if (MessageBox.Show("¿Cancelar el movimiento #" + id + "?\nSe recalcularán saldos y costos.", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                Ejecutar(() => sis.Cancelar(id), "Movimiento #" + id + " cancelado");
            };
            btnReabrir.Click += (s, e) => Ejecutar(() => sis.Reabrir(cmbReabrir.SelectedItem as OrdenProduccion), "Orden reabierta");

            tabs.TabPages.AddRange(new[] { tabHoja, tabGrafica, tabCorr });
        }

        // ============ REFRESCO DE LAS PESTAÑAS NUEVAS ============
        static void PonerFecha(DateTimePicker d, DateTime? v)
        { if (v.HasValue) { d.Value = v.Value; d.Checked = true; } else d.Checked = false; }

        void RefreshExtras()
        {
            Bind(cmbHojaOrden, sis.Ordenes.ToList());
            Bind(cmbReabrir, sis.Ordenes.Where(o => o.Terminada).ToList());
            CargarHoja();
            grafica.Datos = sis.Ordenes.ToList(); grafica.Invalidate();
            dgvBitacora.DataSource = sis.Bitacora().Select(x => new { x.Id, x.Fecha, x.Tipo, x.Detalle, x.Orden, x.Importe }).ToList();
        }

        void CargarHoja()
        {
            var op = cmbHojaOrden.SelectedItem as OrdenProduccion;
            if (op == null) { dgvHoja.DataSource = null; lblHojaTot.Text = "   Aún no hay órdenes de producción."; return; }
            txtCliente.Text = op.Cliente; txtArticulo.Text = op.Descripcion; txtCondiciones.Text = op.Condiciones;
            nudPrecioVenta.Value = op.PrecioVenta; nudSIMP.Value = op.SI_MP; nudSIMO.Value = op.SI_MO; nudSIGI.Value = op.SI_GI;
            PonerFecha(dtpPedido, op.FechaPedido); PonerFecha(dtpInicio, op.FechaInicio); PonerFecha(dtpEntrega, op.FechaEntrega);
            dgvHoja.DataSource = sis.HojaCostos(op).Select(x => new {
                x.Fecha,
                x.Concepto,
                MP_Cant = x.MPCant,
                MP_Precio = x.MPPrecio,
                MP_Total = x.MPTotal,
                MO_Cant = x.MOCant,
                MO_Precio = x.MOPrecio,
                MO_Total = x.MOTotal,
                GI_Cant = x.GICant,
                GI_Precio = x.GIPrecio,
                GI_Total = x.GITotal,
                x.Total
            }).ToList();
            lblHojaTot.Text = string.Format("   Costo total: {0:N2}   |   Unidades: {1}   |   Costo unitario: {2}   |   Precio de venta: {3:N2}",
                op.CostoTotal, op.Terminada ? op.Unidades.ToString("N2") : "—", op.Terminada ? op.CostoUnitario.ToString("N4") : "—", op.PrecioVenta);
        }

        // ============ GUARDAR / ABRIR / EXPORTAR ============
        void GuardarArchivo()
        {
            using (var d = new SaveFileDialog { Filter = "Datos de costos (*.json)|*.json", FileName = "costos.json" })
                if (d.ShowDialog(this) == DialogResult.OK)
                    Ejecutar(() => sis.Guardar(d.FileName), "Guardado en " + Path.GetFileName(d.FileName));
        }

        void AbrirArchivo()
        {
            using (var d = new OpenFileDialog { Filter = "Datos de costos (*.json)|*.json" })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                if ((sis.Materiales.Any() || sis.Ordenes.Any()) &&
                    MessageBox.Show("Se reemplazarán los datos actuales. ¿Continuar?", "Abrir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                Ejecutar(() => sis = SistemaCostos.Abrir(d.FileName), "Abierto: " + Path.GetFileName(d.FileName));
            }
        }

        void ExportarExcel()
        {
            using (var d = new SaveFileDialog { Filter = "Libro de Excel (*.xlsx)|*.xlsx", FileName = "Costos_por_ordenes.xlsx" })
                if (d.ShowDialog(this) == DialogResult.OK)
                    Ejecutar(() => Exportador.Exportar(sis, d.FileName), "Exportado a " + Path.GetFileName(d.FileName));
        }

        // ============ IMPRESIÓN DE LA HOJA DE COSTOS ============
        void Imprimir(bool vista)
        {
            var op = cmbHojaOrden.SelectedItem as OrdenProduccion;
            if (op == null) { MessageBox.Show("Seleccione una orden.", "Aviso"); return; }
            var filas = sis.HojaCostos(op);
            var pd = new PrintDocument { DocumentName = "Orden de producción " + op.Numero };
            pd.DefaultPageSettings.Landscape = true;
            pd.BeginPrint += (s, e) => _filaImp = 0;
            pd.PrintPage += (s, e) => DibujarHoja(e, op, filas);
            if (vista)
            { using (var dlg = new PrintPreviewDialog { Document = pd, Width = 1100, Height = 750 }) dlg.ShowDialog(this); }
            else
            { using (var dlg = new PrintDialog { Document = pd, UseEXDialog = true }) if (dlg.ShowDialog(this) == DialogResult.OK) pd.Print(); }
        }

        static string Fch(DateTime? d) { return d.HasValue ? d.Value.ToString("dd/MM/yyyy") : ""; }
        static string F2(decimal? v) { return v.HasValue ? v.Value.ToString("N2") : ""; }
        static string F4(decimal? v) { return v.HasValue ? v.Value.ToString("N4") : ""; }

        static void Cad(Graphics g, string t, Font f, float x, float y, float w, float h, StringAlignment a)
        {
            using (var sf = new StringFormat
            {
                Alignment = a,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
                g.DrawString(t ?? "", f, Brushes.Black, new RectangleF(x, y, w, h), sf);
        }

        static void Celda(Graphics g, Pen pen, Font f, string t, float x, float y, float w, float h, StringAlignment a)
        {
            g.DrawRectangle(pen, x, y, w, h);
            Cad(g, t, f, x + 2, y, w - 4, h, a);
        }

        void DibujarHoja(PrintPageEventArgs e, OrdenProduccion op, List<FilaHoja> filas)
        {
            var g = e.Graphics; var r = e.MarginBounds;
            using (var fT = new Font("Segoe UI", 14F, FontStyle.Bold))
            using (var fB = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var fN = new Font("Segoe UI", 8.5F))
            using (var pen = new Pen(Color.Black))
            {
                const float hr = 20; float y = r.Top; float m = r.Width / 2f;
                Cad(g, "ORDEN DE PRODUCCIÓN No. " + op.Numero, fT, r.Left, y, r.Width, 28, StringAlignment.Near); y += 32;
                Cad(g, "Cliente: " + op.Cliente, fN, r.Left, y, m, hr, StringAlignment.Near);
                Cad(g, "Fecha de pedido: " + Fch(op.FechaPedido), fN, r.Left + m, y, m, hr, StringAlignment.Near); y += hr;
                Cad(g, "Artículo: " + op.Descripcion, fN, r.Left, y, m, hr, StringAlignment.Near);
                Cad(g, "Fecha de inicio: " + Fch(op.FechaInicio), fN, r.Left + m, y, m, hr, StringAlignment.Near); y += hr;
                Cad(g, "Precio de venta: " + op.PrecioVenta.ToString("N2"), fN, r.Left, y, m, hr, StringAlignment.Near);
                Cad(g, "Fecha de terminación: " + Fch(op.FechaTermino), fN, r.Left + m, y, m, hr, StringAlignment.Near); y += hr;
                Cad(g, "Condiciones: " + op.Condiciones, fN, r.Left, y, m, hr, StringAlignment.Near);
                Cad(g, "Fecha de entrega: " + Fch(op.FechaEntrega), fN, r.Left + m, y, m, hr, StringAlignment.Near); y += hr + 10;

                float[] w = new float[12]; w[0] = r.Width * 0.09f; w[1] = r.Width * 0.14f;
                for (int i = 2; i < 12; i++) w[i] = r.Width * 0.077f;
                float[] cx = new float[13]; cx[0] = r.Left;
                for (int i = 0; i < 12; i++) cx[i + 1] = cx[i] + w[i];

                var c = StringAlignment.Center;
                Celda(g, pen, fB, "FECHA", cx[0], y, w[0], hr * 2, c);
                Celda(g, pen, fB, "CONCEPTO", cx[1], y, w[1], hr * 2, c);
                Celda(g, pen, fB, "MATERIA PRIMA", cx[2], y, w[2] + w[3] + w[4], hr, c);
                Celda(g, pen, fB, "MANO DE OBRA", cx[5], y, w[5] + w[6] + w[7], hr, c);
                Celda(g, pen, fB, "GASTOS INDIRECTOS", cx[8], y, w[8] + w[9] + w[10], hr, c);
                Celda(g, pen, fB, "TOTAL", cx[11], y, w[11], hr * 2, c);
                string[] sub = { "CANTIDAD", "PRECIO", "TOTAL" };
                for (int k = 0; k < 9; k++) Celda(g, pen, fN, sub[k % 3], cx[2 + k], y + hr, w[2 + k], hr, c);
                y += hr * 2;

                while (_filaImp < filas.Count && y + hr <= r.Bottom - 70)
                {
                    var fl = filas[_filaImp++];
                    string[] t = { fl.Concepto == "SI" ? "SI" : fl.Fecha.ToString("dd/MM/yy"), fl.Concepto,
                        F2(fl.MPCant), F4(fl.MPPrecio), F2(fl.MPTotal), F2(fl.MOCant), F4(fl.MOPrecio), F2(fl.MOTotal),
                        F2(fl.GICant), F4(fl.GIPrecio), F2(fl.GITotal), fl.Total.ToString("N2") };
                    for (int i = 0; i < 12; i++)
                        Celda(g, pen, fN, t[i], cx[i], y, w[i], hr, i < 2 ? StringAlignment.Near : StringAlignment.Far);
                    y += hr;
                }
                if (_filaImp < filas.Count) { e.HasMorePages = true; return; }

                y += 10;
                Cad(g, "COSTO TOTAL: " + op.CostoTotal.ToString("N2"), fB, r.Left, y, r.Width, hr, StringAlignment.Near); y += hr;
                Cad(g, "UNIDADES PRODUCIDAS: " + (op.Terminada ? op.Unidades.ToString("N2") : "—"), fB, r.Left, y, r.Width, hr, StringAlignment.Near); y += hr;
                Cad(g, "COSTO UNITARIO: " + (op.Terminada ? op.CostoUnitario.ToString("N4") : "—"), fB, r.Left, y, r.Width, hr, StringAlignment.Near);
                e.HasMorePages = false;
            }
        }
    }

    // =====================================================
    //  Gráfica de barras apiladas (dibujo propio, tema oscuro)
    // =====================================================
    public class GraficaCostos : Control
    {
        public List<OrdenProduccion> Datos = new List<OrdenProduccion>();
        static readonly Color cTxt = Color.FromArgb(31, 41, 51), cGris = Color.FromArgb(107, 119, 133), cLin = Color.FromArgb(230, 235, 240);
        static readonly Color[] cols = { Color.FromArgb(70, 130, 169), Color.FromArgb(91, 169, 113), Color.FromArgb(240, 168, 60) };
        static readonly string[] nombres = { "Materia prima", "Mano de obra", "Gastos indirectos" };

        public GraficaCostos()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        static decimal Techo(decimal v)
        {
            double d = (double)v, p = Math.Pow(10, Math.Floor(Math.Log10(d))), n = d / p;
            double t = n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10;
            return (decimal)(t * p);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(BackColor);
            using (var fN = new Font("Segoe UI", 9F)) using (var fB = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                int lx = 20;
                for (int i = 0; i < 3; i++)
                {
                    using (var br = new SolidBrush(cols[i])) g.FillRectangle(br, lx, 18, 14, 14);
                    TextRenderer.DrawText(g, nombres[i], fN, new Point(lx + 20, 15), cTxt);
                    lx += 20 + TextRenderer.MeasureText(nombres[i], fN).Width + 26;
                }
                if (Datos.Count == 0)
                {
                    TextRenderer.DrawText(g, "Aún no hay órdenes para graficar.", fN, ClientRectangle, cGris,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    return;
                }
                var area = new Rectangle(90, 55, Width - 120, Height - 120);
                if (area.Width < 60 || area.Height < 60) return;
                decimal max = Datos.Max(o => o.CostoTotal); if (max <= 0) max = 1;
                decimal tope = Techo(max);
                using (var pen = new Pen(cLin))
                    for (int i = 0; i <= 5; i++)
                    {
                        float yy = area.Bottom - area.Height * i / 5f;
                        g.DrawLine(pen, area.Left, yy, area.Right, yy);
                        TextRenderer.DrawText(g, (tope * i / 5).ToString("N0"), fN, new Rectangle(0, (int)yy - 9, 84, 18), cGris,
                            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                    }
                float slot = (float)area.Width / Datos.Count, bw = Math.Min(110f, slot * 0.55f);
                for (int i = 0; i < Datos.Count; i++)
                {
                    var o = Datos[i];
                    float x = area.Left + slot * i + (slot - bw) / 2f, y0 = area.Bottom;
                    decimal[] vals = { o.MateriaPrima, o.ManoObra, o.GastosIndirectos };
                    for (int k = 0; k < 3; k++)
                    {
                        float h = (float)(vals[k] / tope * area.Height);
                        if (h <= 0) continue;
                        y0 -= h;
                        using (var br = new SolidBrush(cols[k])) g.FillRectangle(br, x, y0, bw, h);
                    }
                    TextRenderer.DrawText(g, o.CostoTotal.ToString("N2"), fB, new Rectangle((int)x - 30, (int)y0 - 22, (int)bw + 60, 20),
                        Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    var rx = new Rectangle((int)(area.Left + slot * i), area.Bottom + 8, (int)slot, 20);
                    TextRenderer.DrawText(g, "OP " + o.Numero, fB, rx, cTxt, TextFormatFlags.HorizontalCenter);
                    rx.Y += 20;
                    TextRenderer.DrawText(g, o.Terminada ? "Terminada" : "En proceso", fN, rx, cGris, TextFormatFlags.HorizontalCenter);
                }
            }
        }
    }
}