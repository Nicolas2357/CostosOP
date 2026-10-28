using System;
using System.Linq;
using ClosedXML.Excel;

namespace CostosOP
{
    public static class Exportador
    {
        const string M2 = "#,##0.00", M4 = "#,##0.0000";

        static void Caja(IXLRange r) { r.Style.Border.OutsideBorder = XLBorderStyleValues.Thin; r.Style.Border.InsideBorder = XLBorderStyleValues.Thin; }
        static void Enc(IXLRange r)
        {
            r.Style.Font.Bold = true; r.Style.Font.FontColor = XLColor.White;
            r.Style.Fill.BackgroundColor = XLColor.FromHtml("#111827");
            r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            r.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            Caja(r);
        }
        static void Tx(IXLWorksheet ws, int r, int c, string v) { ws.Cell(r, c).Value = v ?? ""; }
        static void Nm(IXLWorksheet ws, int r, int c, decimal? v, string f = M2)
        { if (!v.HasValue) return; var ce = ws.Cell(r, c); ce.Value = (double)v.Value; ce.Style.NumberFormat.Format = f; }
        static void Fe(IXLWorksheet ws, int r, int c, DateTime? v)
        { if (!v.HasValue) return; var ce = ws.Cell(r, c); ce.Value = v.Value; ce.Style.DateFormat.Format = "dd/MM/yyyy"; }

        public static void Exportar(SistemaCostos sis, string ruta)
        {
            using (var wb = new XLWorkbook())
            {
                HojaOrdenes(wb.Worksheets.Add("ORDENES"), sis);
                HojaTarjetas(wb.Worksheets.Add("TARJETA DE ALMACEN"), sis);
                HojaProrrateo(wb.Worksheets.Add("PRORRATEO"), sis);
                HojaResumen(wb.Worksheets.Add("RESUMEN"), sis);
                wb.SaveAs(ruta);
            }
        }

        static void HojaOrdenes(IXLWorksheet ws, SistemaCostos sis)
        {
            int r = 1;
            foreach (var op in sis.Ordenes)
            {
                Tx(ws, r, 1, "Orden de Producción"); ws.Cell(r, 1).Style.Font.Bold = true; ws.Cell(r, 1).Style.Font.FontSize = 13;
                Tx(ws, r, 7, "Num."); Nm(ws, r, 8, op.Numero, "0");
                Tx(ws, r + 1, 1, "Fecha de pedido"); Fe(ws, r + 1, 2, op.FechaPedido);
                Tx(ws, r + 2, 1, "Fecha de inicio"); Fe(ws, r + 2, 2, op.FechaInicio);
                Tx(ws, r + 3, 1, "Fecha de terminación"); Fe(ws, r + 3, 2, op.FechaTermino);
                Tx(ws, r + 4, 1, "Fecha de entrega"); Fe(ws, r + 4, 2, op.FechaEntrega);
                Tx(ws, r + 1, 7, "Nombre del cliente"); Tx(ws, r + 1, 8, op.Cliente);
                Tx(ws, r + 2, 7, "Artículo"); Tx(ws, r + 2, 8, op.Descripcion);
                Tx(ws, r + 3, 7, "Precio de venta"); Nm(ws, r + 3, 8, op.PrecioVenta);
                Tx(ws, r + 4, 7, "Condiciones"); Tx(ws, r + 4, 8, op.Condiciones);

                int h = r + 6;
                Tx(ws, h, 1, "FECHA"); ws.Range(h, 1, h + 1, 1).Merge();
                Tx(ws, h, 2, "MATERIA PRIMA"); ws.Range(h, 2, h, 4).Merge();
                Tx(ws, h, 5, "MANO DE OBRA"); ws.Range(h, 5, h, 7).Merge();
                Tx(ws, h, 8, "GASTOS INDIRECTOS"); ws.Range(h, 8, h, 10).Merge();
                Tx(ws, h, 11, "TOTAL"); ws.Range(h, 11, h + 1, 11).Merge();
                string[] sub = { "CANTIDAD", "PRECIO", "TOTAL" };
                for (int k = 0; k < 9; k++) Tx(ws, h + 1, 2 + k, sub[k % 3]);
                Enc(ws.Range(h, 1, h + 1, 11));

                int row = h + 2, ini = row;
                foreach (var f in sis.HojaCostos(op))
                {
                    Tx(ws, row, 1, f.Concepto == "SI" ? "SI" : f.Fecha.ToString("dd/MM/yyyy") + "  " + f.Concepto);
                    Nm(ws, row, 2, f.MPCant); Nm(ws, row, 3, f.MPPrecio, M4); Nm(ws, row, 4, f.MPTotal);
                    Nm(ws, row, 5, f.MOCant); Nm(ws, row, 6, f.MOPrecio); Nm(ws, row, 7, f.MOTotal);
                    Nm(ws, row, 8, f.GICant); Nm(ws, row, 9, f.GIPrecio, M4); Nm(ws, row, 10, f.GITotal);
                    string suma = "SUM(D" + row + ",G" + row + ",J" + row + ")";
                    ws.Cell(row, 11).FormulaA1 = row == ini ? "=" + suma : "=K" + (row - 1) + "+" + suma;
                    ws.Cell(row, 11).Style.NumberFormat.Format = M2;
                    row++;
                }
                if (row > ini)
                {
                    Caja(ws.Range(ini, 1, row - 1, 11));
                    Tx(ws, row + 1, 9, "COSTO TOTAL"); ws.Cell(row + 1, 11).FormulaA1 = "=K" + (row - 1);
                    ws.Cell(row + 1, 11).Style.NumberFormat.Format = M2;
                    Tx(ws, row + 2, 9, "UNIDADES"); Nm(ws, row + 2, 11, op.Terminada ? op.Unidades : (decimal?)null);
                    Tx(ws, row + 3, 9, "COSTO UNITARIO"); Nm(ws, row + 3, 11, op.Terminada ? op.CostoUnitario : (decimal?)null, M4);
                    ws.Range(row + 1, 9, row + 3, 11).Style.Font.Bold = true;
                }
                r = row + 6;
            }
            ws.Columns().AdjustToContents();
        }

        static void HojaTarjetas(IXLWorksheet ws, SistemaCostos sis)
        {
            int r = 2;
            foreach (var m in sis.Materiales)
            {
                Tx(ws, r, 2, "ARTÍCULO:"); Tx(ws, r, 3, m.Nombre); Tx(ws, r, 7, "CLAVE:"); Tx(ws, r, 8, m.Codigo);
                Tx(ws, r + 1, 2, "LOCALIZACIÓN"); Tx(ws, r + 1, 7, "UNIDAD DE MEDIDA:"); Tx(ws, r + 1, 8, m.Unidad);
                int h = r + 3;
                Tx(ws, h, 2, "FECHA"); ws.Range(h, 2, h + 1, 2).Merge();
                Tx(ws, h, 3, "REFERENCIA"); ws.Range(h, 3, h + 1, 3).Merge();
                Tx(ws, h, 4, "UNIDADES"); ws.Range(h, 4, h, 6).Merge();
                Tx(ws, h, 7, "PRECIO UNITARIO"); ws.Range(h, 7, h + 1, 7).Merge();
                Tx(ws, h, 8, "PRECIO NETO"); ws.Range(h, 8, h + 1, 8).Merge();
                Tx(ws, h, 9, "VALORES"); ws.Range(h, 9, h, 11).Merge();
                string[] s1 = { "ENTRADAS", "SALIDAS", "EXISTENCIA" }, s2 = { "DEBE", "HABER", "SALDO" };
                for (int k = 0; k < 3; k++) { Tx(ws, h + 1, 4 + k, s1[k]); Tx(ws, h + 1, 9 + k, s2[k]); }
                Enc(ws.Range(h, 2, h + 1, 11));
                int row = h + 2;
                foreach (var f in sis.TarjetaAlmacen(m))
                {
                    Tx(ws, row, 2, f.Fecha.ToString("dd/MM/yyyy")); Tx(ws, row, 3, f.Ref);
                    Nm(ws, row, 4, f.Entradas); Nm(ws, row, 5, f.Salidas); Nm(ws, row, 6, f.Existencia);
                    Nm(ws, row, 7, f.PrecioUnit, M4); Nm(ws, row, 8, f.PrecioNeto, M4);
                    Nm(ws, row, 9, f.Debe); Nm(ws, row, 10, f.Haber); Nm(ws, row, 11, f.Saldo);
                    row++;
                }
                if (row > h + 2) Caja(ws.Range(h + 2, 2, row - 1, 11));
                r = row + 3;
            }
            ws.Columns().AdjustToContents();
        }

        static void HojaProrrateo(IXLWorksheet ws, SistemaCostos sis)
        {
            int r = 2;
            if (sis.Lotes.Count == 0) Tx(ws, r, 2, "Aún no se ha realizado ningún prorrateo.");
            foreach (var l in sis.Lotes)
            {
                Tx(ws, r, 2, "Prorrateo #" + l.Id + "  |  Base: " + l.BaseNombre + "  |  " + l.Fecha.ToString("dd/MM/yyyy"));
                ws.Cell(r, 2).Style.Font.Bold = true;
                Tx(ws, r + 1, 2, "ORDEN"); Tx(ws, r + 1, 3, "BASE"); Tx(ws, r + 1, 4, "FACTOR"); Tx(ws, r + 1, 5, "G.I. ASIGNADO");
                Enc(ws.Range(r + 1, 2, r + 1, 5));
                int row = r + 2, ini = row;
                foreach (var d in l.Detalle)
                { Nm(ws, row, 2, d.Orden, "0"); Nm(ws, row, 3, d.Base); Nm(ws, row, 4, d.Tasa, M4); Nm(ws, row, 5, d.Importe); row++; }
                Caja(ws.Range(ini, 2, row - 1, 5));
                Tx(ws, row, 2, "TOTAL");
                ws.Cell(row, 3).FormulaA1 = "=SUM(C" + ini + ":C" + (row - 1) + ")";
                ws.Cell(row, 5).FormulaA1 = "=SUM(E" + ini + ":E" + (row - 1) + ")";
                ws.Cell(row, 3).Style.NumberFormat.Format = M2; ws.Cell(row, 5).Style.NumberFormat.Format = M2;
                ws.Range(row, 2, row, 5).Style.Font.Bold = true;
                Tx(ws, row + 1, 2, "Factor = total G.I. ÷ total de la base = " + l.TotalGI.ToString("N2") + " ÷ " + l.SumaBase.ToString("N2") + " = " + l.Tasa.ToString("N4"));
                r = row + 4;
            }
            for (int c = 2; c <= 5; c++) ws.Column(c).Width = 18;
        }

        static void HojaResumen(IXLWorksheet ws, SistemaCostos sis)
        {
            string[] h = { "ORDEN", "CLIENTE", "ARTÍCULO", "ESTADO", "MATERIA PRIMA", "MANO DE OBRA", "GASTOS INDIRECTOS", "COSTO TOTAL", "UNIDADES", "COSTO UNITARIO" };
            for (int i = 0; i < h.Length; i++) Tx(ws, 2, 2 + i, h[i]);
            Enc(ws.Range(2, 2, 2, 11));
            int row = 3, ini = 3;
            foreach (var o in sis.Ordenes)
            {
                Nm(ws, row, 2, o.Numero, "0"); Tx(ws, row, 3, o.Cliente); Tx(ws, row, 4, o.Descripcion);
                Tx(ws, row, 5, o.Terminada ? "Terminada" : "En proceso");
                Nm(ws, row, 6, o.MateriaPrima); Nm(ws, row, 7, o.ManoObra); Nm(ws, row, 8, o.GastosIndirectos);
                ws.Cell(row, 9).FormulaA1 = "=SUM(F" + row + ":H" + row + ")"; ws.Cell(row, 9).Style.NumberFormat.Format = M2;
                if (o.Terminada) { Nm(ws, row, 10, o.Unidades); Nm(ws, row, 11, o.CostoUnitario, M4); }
                row++;
            }
            if (row > ini)
            {
                Caja(ws.Range(ini, 2, row - 1, 11));
                Tx(ws, row, 5, "TOTALES");
                foreach (var col in new[] { "F", "G", "H", "I" })
                { var ce = ws.Cell(col + row); ce.FormulaA1 = "=SUM(" + col + ini + ":" + col + (row - 1) + ")"; ce.Style.NumberFormat.Format = M2; }
                ws.Range(row, 5, row, 9).Style.Font.Bold = true;
                row++;
            }
            row += 2;
            Tx(ws, row, 2, "SALDO FINAL DE MATERIA PRIMA"); ws.Cell(row, 2).Style.Font.Bold = true; row++;
            string[] h2 = { "CÓDIGO", "MATERIAL", "EXISTENCIA", "COSTO PROMEDIO", "SALDO" };
            for (int i = 0; i < h2.Length; i++) Tx(ws, row, 2 + i, h2[i]);
            Enc(ws.Range(row, 2, row, 6)); row++;
            int ini2 = row;
            foreach (var m in sis.Materiales)
            {
                Tx(ws, row, 2, m.Codigo); Tx(ws, row, 3, m.Nombre);
                Nm(ws, row, 4, sis.Existencia(m)); Nm(ws, row, 5, sis.CostoPromedio(m), M4); Nm(ws, row, 6, sis.Saldo(m));
                row++;
            }
            if (row > ini2)
            {
                Caja(ws.Range(ini2, 2, row - 1, 6));
                Tx(ws, row, 5, "TOTAL");
                ws.Cell(row, 6).FormulaA1 = "=SUM(F" + ini2 + ":F" + (row - 1) + ")"; ws.Cell(row, 6).Style.NumberFormat.Format = M2;
                ws.Range(row, 5, row, 6).Style.Font.Bold = true;
            }
            double[] anchos = { 10, 28, 24, 14, 18, 18, 20, 16, 12, 16 };
            for (int i = 0; i < anchos.Length; i++) ws.Column(2 + i).Width = anchos[i];
        }
    }
}