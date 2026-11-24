# Sistema de Costos por Órdenes de Producción

Aplicación de escritorio en **C# y Windows Forms** que calcula el costo de cada orden de producción (materia prima + mano de obra + gastos indirectos) y su costo unitario. Sigue el flujo de un sistema de costos por órdenes: captura, tarjetas de almacén, órdenes de producción, gastos indirectos con prorrateo y resultados.

**Autor:** [Tu nombre]
**Materia:** [Nombre de la materia]
**Profesor:** [Nombre del profesor]
**Fecha:** [Fecha de entrega]

---

## Funcionalidades

| Módulo | Qué hace |
|---|---|
| **Tablero** | Resumen con mosaicos: inventario final de MP, órdenes activas, G.I. pendientes, costo acumulado, conteos y acciones rápidas. |
| **Almacén** | Alta de materiales, inventario inicial y compras. Tarjeta de almacén (kardex) con entradas, salidas y saldo. Salidas valuadas a **costo promedio**. |
| **Órdenes** | Alta de órdenes (1025, 1026, 1027...). Requisición de materia prima a cada orden y mano de obra por **horas × tarifa**. |
| **Gastos indirectos** | Captura de gastos (supervisores, luz, servicios, etc.) y **prorrateo** entre las órdenes en proceso. |
| **Resultados** | Marcar la orden como terminada, costo total (MP + MO + GI), costo unitario y saldo final de materia prima. |
| **Hoja de costos** | Detalle de una orden en el formato de la hoja "Orden de Producción": encabezado, saldo inicial, renglones y total acumulado. Con vista previa e impresión. |
| **Gráfica** | Barras apiladas con el costo de cada orden (MP, MO y GI). |
| **Correcciones** | Cancelar compras, requisiciones, nómina, gastos o prorrateos mal capturados, y reabrir órdenes terminadas. Todo se recalcula. |

Además:
- **Guardar / Abrir** los datos en un archivo `.json`.
- **Exportar a Excel** con las hojas ORDENES, TARJETA DE ALMACEN, PRORRATEO y RESUMEN.

---

## Requisitos

- Windows 10 u 11
- Visual Studio 2022
- Proyecto de **Windows Forms** en .NET 6/8 o .NET Framework 4.8
- Paquetes NuGet:
  - `ClosedXML` (exportar a Excel)
  - `System.Text.Json` (solo si usas .NET Framework 4.8; en .NET 6/8 ya viene incluido)

---

## Instalación y ejecución

1. Abre la solución en Visual Studio 2022.
2. Restaura los paquetes NuGet (clic derecho en la solución → *Restaurar paquetes NuGet*).
3. Presiona **F5** para compilar y ejecutar.

El ejecutable queda en `bin\Debug` (o `bin\Release`).

---

## Estructura del proyecto

```
CostosOP/
├── Modelos.cs           Clases y lógica de negocio (SistemaCostos)
├── Form1.cs             Eventos de los controles y refresco de pantalla
├── Form1.Designer.cs    Diseño de la interfaz (menú lateral, tablero, tarjetas)
├── Form1.Extras.cs      Hoja de costos, gráfica, correcciones, guardar/abrir/exportar
├── Exportador.cs        Generación del archivo de Excel
└── Program.cs           Punto de entrada
```

**Clases principales (`Modelos.cs`):** `Material`, `MovimientoAlmacen`, `OrdenProduccion`, `RegistroMO`, `GastoIndirecto`, `Lote` (cada prorrateo), `DetalleProrrateo` y `SistemaCostos`, que concentra todas las reglas de cálculo.

---

## Cómo se usa (paso a paso)

1. **Almacén:** da de alta un material y registra su inventario inicial y sus compras.
2. **Órdenes:** da de alta la orden. Asígnale materia prima (requisición) y mano de obra (horas y tarifa).
3. **Gastos indirectos:** captura los gastos. Quedan como *Pendiente*.
4. **Resultados:** termina las órdenes que ya acabaron, capturando las unidades producidas.
5. **Gastos indirectos:** elige la base de reparto y presiona **PRORRATEAR**. Solo reciben gastos las órdenes en proceso.
6. **Resultados / Hoja de costos / Gráfica:** consulta el costo total y unitario por orden y el saldo final de materia prima.

> Orden recomendado: termina primero las órdenes que ya acabaron y después prorratea, para que los gastos solo se repartan entre las órdenes en proceso.

### Prueba rápida con el ejercicio de ejemplo

1. En **Almacén**, presiona **Cargar datos de ejemplo** (carga el ejercicio del archivo `Ejercicio_Ordenes_3.xlsx`).
2. En **Resultados**, termina la **OP 1025** con las unidades que quieras.
3. En **Gastos indirectos**, elige la base **Horas de M.O.** y presiona **PRORRATEAR**.

Resultados esperados:

| Orden | Costo total |
|---|---|
| 1025 | 83,890.91 |
| 1026 | 36,440.97 |
| 1027 | 62,294.16 |

Prorrateo: 38,800 ÷ 625 horas = **62.08 por hora**, que da 13,968 para la 1026 y 24,832 para la 1027.

---

## Reglas de cálculo

- **Materia prima:** las salidas a producción se valúan a costo promedio ponderado = saldo ÷ existencia.
- **Mano de obra:** horas × tarifa por hora.
- **Gastos indirectos:** factor = total de G.I. ÷ total de la base. G.I. de la orden = base de la orden × factor. Bases disponibles: *Horas de M.O.*, *Mano de obra ($)*, *Materia prima* y *Partes iguales*. El último reparto absorbe el redondeo para que cuadre al centavo.
- **Costo de la orden:** MP + MO + GI (incluye el saldo inicial, si lo tiene).
- **Costo unitario:** costo total ÷ unidades producidas.

---

## Archivos de datos

- **Guardar / Abrir:** archivo `.json` con todos los materiales, movimientos, órdenes, nómina, gastos y prorrateos. Al abrirlo se recalculan saldos y costos.
- **Exportar a Excel:** archivo `.xlsx` con el formato de las hojas del ejercicio y los totales con fórmulas.

---

## Limitaciones conocidas

- No genera asientos de diario ni esquemas de mayor.
- No tiene almacén de productos terminados, ventas ni costo de ventas.
- Solo usa costo promedio (no PEPS ni UEPS).
- Un solo periodo continuo (sin cierre mensual).
- Sin usuarios ni permisos.

## Mejoras futuras

- Pólizas contables (Almacén → Producción en proceso → Artículos terminados).
- Inventario de artículos terminados y ventas con costo de ventas.
- Método PEPS además del costo promedio.
- Cierre por periodos.
- Base de datos (SQLite o SQL Server) en lugar de archivo JSON.
