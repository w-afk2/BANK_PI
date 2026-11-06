"use strict";

/*
 * Interfaz del operador.
 * Solo consume la API REST: no accede a SQLite ni repite reglas bancarias.
 * Los datos se envían tal como los escribe el operador; las validaciones (montos, decimales,
 * fondos, estado de la cuenta) las aplica el backend, y aquí solo se muestran sus respuestas
 * o sus errores (ProblemDetails).
 */

const ZONA_HORARIA = "America/Managua";
const MARCA_NUMERO = "@@NUM@@";
const TAMANO_PAGINA_AUDITORIA = 10;

const formatoFechaHora = new Intl.DateTimeFormat("es-NI", {
  dateStyle: "medium",
  timeStyle: "medium",
  timeZone: ZONA_HORARIA,
});
const formatoFecha = new Intl.DateTimeFormat("es-NI", { dateStyle: "long", timeZone: "UTC" });
const formatoNumero = new Intl.NumberFormat("es-NI", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

const TITULOS_HTTP = {
  0: "Sin conexión",
  400: "Datos inválidos",
  404: "No encontrado",
  409: "Operación rechazada",
  500: "Error interno",
};

const estado = {
  monedas: new Map(),
  auditoria: { filtros: "", pagina: 1, totalPaginas: 0 },
};

class ErrorApi extends Error {
  constructor(codigo, problema) {
    super(problema?.detail ?? problema?.title ?? `Error HTTP ${codigo}`);
    this.codigo = codigo;
    this.problema = problema ?? {};
  }
}

/* ---------------------------------------------------------------- DOM */

function el(etiqueta, atributos = {}, ...hijos) {
  const nodo = document.createElement(etiqueta);

  for (const [clave, valor] of Object.entries(atributos)) {
    if (valor === null || valor === undefined || valor === false) continue;
    if (clave === "class") nodo.className = valor;
    else if (clave.startsWith("on")) nodo.addEventListener(clave.slice(2), valor);
    else nodo.setAttribute(clave, valor === true ? "" : String(valor));
  }

  for (const hijo of hijos.flat()) {
    if (hijo === null || hijo === undefined || hijo === false) continue;
    nodo.append(hijo instanceof Node ? hijo : String(hijo));
  }

  return nodo;
}

function valor(formulario, nombre) {
  return formulario.elements.namedItem(nombre).value;
}

function ficha(pares) {
  return el("dl", { class: "ficha" }, pares.flatMap(([etiqueta, contenido]) => [
    el("dt", {}, etiqueta),
    el("dd", {}, contenido),
  ]));
}

function tabla(columnas, filas) {
  const encabezado = el("tr", {}, columnas.map((columna) => el("th", { class: columna.clase }, columna.titulo)));
  const cuerpo = filas.map((fila) =>
    el("tr", {}, fila.map((celda, indice) => el("td", { class: columnas[indice].clase }, celda))));

  return el("div", { class: "tabla-contenedor" }, el("table", {}, el("thead", {}, encabezado), el("tbody", {}, cuerpo)));
}

/* ---------------------------------------------------------------- Formatos (solo presentación) */

function dinero(importe, simbolo, codigo) {
  const partes = [];
  if (simbolo) partes.push(simbolo);
  partes.push(formatoNumero.format(importe));
  if (codigo) partes.push(codigo);
  return partes.join(" ");
}

function simboloDe(codigoMoneda) {
  return estado.monedas.get(codigoMoneda)?.simbolo ?? "";
}

function fechaHora(textoIso) {
  return formatoFechaHora.format(new Date(textoIso));
}

function fecha(textoFecha) {
  return formatoFecha.format(new Date(`${textoFecha}T00:00:00Z`));
}

function textoGenero(genero) {
  return genero === "PrefieroNoDecirlo" ? "Prefiero no decirlo" : genero;
}

function textoAccion(accion) {
  const separado = accion.replace(/([a-z])([A-Z])/g, "$1 $2").toLowerCase();
  return separado.charAt(0).toUpperCase() + separado.slice(1);
}

function insignia(texto) {
  const clase = String(texto ?? "")
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase();
  return el("span", { class: `insignia ${clase}` }, texto);
}

function numeroCuentaEl(numeroCuenta) {
  return el("span", { class: "numero-cuenta" }, numeroCuenta);
}

/* ---------------------------------------------------------------- API */

/**
 * Envía el número exactamente como lo escribió el operador, sin convertirlo a coma flotante,
 * para que el backend valide decimales y rangos con precisión decimal.
 * Un texto no numérico se envía como texto y el backend responde 400.
 */
function numero(texto) {
  const limpio = String(texto ?? "").trim().replace(",", ".");
  if (limpio === "") return null;
  return /^-?\d+(\.\d+)?$/.test(limpio) ? `${MARCA_NUMERO}${limpio}` : limpio;
}

function aJson(objeto) {
  const patron = new RegExp(`"${MARCA_NUMERO}(-?\\d+(?:\\.\\d+)?)"`, "g");
  return JSON.stringify(objeto).replace(patron, "$1");
}

async function api(metodo, ruta, cuerpo) {
  const opciones = { method: metodo, headers: { Accept: "application/json" } };

  if (cuerpo !== undefined) {
    opciones.headers["Content-Type"] = "application/json";
    opciones.body = aJson(cuerpo);
  }

  let respuesta;
  try {
    respuesta = await fetch(ruta, opciones);
  } catch {
    throw new ErrorApi(0, {
      title: TITULOS_HTTP[0],
      detail: "No se pudo contactar con la API. Verifique que esté en ejecución.",
    });
  }

  const texto = await respuesta.text();
  let datos = null;
  if (texto) {
    try {
      datos = JSON.parse(texto);
    } catch {
      datos = null;
    }
  }

  if (!respuesta.ok) {
    throw new ErrorApi(respuesta.status, datos);
  }

  return datos;
}

function requerido(texto, mensaje) {
  const limpio = String(texto ?? "").trim();
  if (!limpio) {
    throw new ErrorApi(0, { title: "Dato requerido", detail: mensaje });
  }
  return limpio;
}

/* ---------------------------------------------------------------- Mensajes */

function mostrarExito(contenedor, titulo, ...contenido) {
  contenedor.replaceChildren(el("div", { class: "mensaje exito" }, el("strong", {}, titulo)), ...contenido);
}

function mostrarInfo(contenedor, titulo, detalle) {
  contenedor.replaceChildren(el("div", { class: "mensaje info" }, el("strong", {}, titulo), detalle));
}

function mostrarError(contenedor, error) {
  const esErrorApi = error instanceof ErrorApi;
  const problema = esErrorApi ? error.problema : {};
  const codigo = esErrorApi ? error.codigo : null;

  const titulo = [
    codigo ? String(codigo) : null,
    problema.title ?? TITULOS_HTTP[codigo] ?? "Error",
  ].filter(Boolean).join(" · ");

  const detalles = [];
  if (problema.detail) detalles.push(problema.detail);
  if (problema.errors) {
    for (const mensajes of Object.values(problema.errors)) detalles.push(...mensajes);
  }
  if (!esErrorApi) detalles.push(error.message);

  const cuerpo = detalles.length > 1
    ? el("ul", {}, detalles.map((detalle) => el("li", {}, detalle)))
    : detalles[0] ?? null;

  contenedor.replaceChildren(el("div", { class: "mensaje error", role: "alert" }, el("strong", {}, titulo), cuerpo));
}

/* ---------------------------------------------------------------- Selección de cliente y cuenta */

function seleccionarCuenta(numeroCuenta) {
  document.querySelectorAll(".campo-cuenta").forEach((campo) => {
    campo.value = numeroCuenta;
  });

  const indicador = document.getElementById("cuenta-actual");
  indicador.textContent = `Cuenta: ${numeroCuenta}`;
  indicador.hidden = false;
}

function seleccionarCliente(clienteId) {
  document.querySelectorAll(".campo-cliente").forEach((campo) => {
    campo.value = clienteId;
  });
}

function botonSeleccionar(numeroCuenta) {
  return el("button", { type: "button", class: "enlace", onclick: () => seleccionarCuenta(numeroCuenta) }, "Seleccionar");
}

/* ---------------------------------------------------------------- Vistas */

function fichaCliente(cliente) {
  return ficha([
    ["Id", String(cliente.id)],
    ["Nombre completo", cliente.nombreCompleto],
    ["Fecha de nacimiento", fecha(cliente.fechaNacimiento)],
    ["Género", textoGenero(cliente.genero)],
    ["Ingresos mensuales", dinero(cliente.ingresosMensuales, simboloDe(cliente.monedaIngresos), cliente.monedaIngresos)],
  ]);
}

function fichaCuenta(cuenta) {
  return ficha([
    ["Número", numeroCuentaEl(cuenta.numeroCuenta)],
    ["Tipo", cuenta.tipoCuenta],
    ["Moneda", cuenta.moneda],
    ["Estado", insignia(cuenta.estado)],
    ["Saldo", dinero(cuenta.saldo, cuenta.simboloMoneda, cuenta.moneda)],
    ["Fecha de creación", fechaHora(cuenta.fechaCreacion)],
  ]);
}

function tablaCuentas(cuentas) {
  if (!cuentas.length) {
    return el("p", { class: "nota" }, "El cliente no tiene cuentas registradas.");
  }

  return tabla(
    [
      { titulo: "Número" },
      { titulo: "Tipo" },
      { titulo: "Moneda" },
      { titulo: "Estado" },
      { titulo: "Saldo", clase: "numero" },
      { titulo: "" },
    ],
    cuentas.map((cuenta) => [
      numeroCuentaEl(cuenta.numeroCuenta),
      cuenta.tipoCuenta,
      cuenta.moneda,
      insignia(cuenta.estado),
      dinero(cuenta.saldo, cuenta.simboloMoneda, cuenta.moneda),
      botonSeleccionar(cuenta.numeroCuenta),
    ]));
}

/* ---------------------------------------------------------------- Formularios */

function alEnviar(idFormulario, manejador) {
  const formulario = document.getElementById(idFormulario);
  const resultado = formulario.querySelector(".resultado");

  formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    const botones = formulario.querySelectorAll("button[type='submit']");
    botones.forEach((boton) => { boton.disabled = true; });

    try {
      await manejador(formulario, resultado, evento.submitter);
    } catch (error) {
      mostrarError(resultado, error);
    } finally {
      botones.forEach((boton) => { boton.disabled = false; });
    }
  });
}

// 1. Registrar cliente
alEnviar("form-registrar-cliente", async (formulario, resultado) => {
  const segundoApellido = valor(formulario, "segundoApellido").trim();

  const cliente = await api("POST", "/api/clientes", {
    nombres: valor(formulario, "nombres"),
    primerApellido: valor(formulario, "primerApellido"),
    segundoApellido: segundoApellido === "" ? null : segundoApellido,
    fechaNacimiento: valor(formulario, "fechaNacimiento") || null,
    genero: valor(formulario, "genero") || null,
    ingresosMensuales: numero(valor(formulario, "ingresosMensuales")),
  });

  formulario.reset();
  seleccionarCliente(cliente.id);
  mostrarExito(resultado, `Cliente registrado con id ${cliente.id}`, fichaCliente(cliente));
});

// 2. Consultar perfil
alEnviar("form-perfil", async (formulario, resultado) => {
  const id = requerido(valor(formulario, "id"), "Ingrese el id del cliente.");
  const cliente = await api("GET", `/api/clientes/${encodeURIComponent(id)}`);

  seleccionarCliente(cliente.id);
  mostrarExito(
    resultado,
    cliente.nombreCompleto,
    fichaCliente(cliente),
    el("h3", { class: "subtitulo" }, "Cuentas"),
    tablaCuentas(cliente.cuentas));
});

// 3. Crear cuenta
alEnviar("form-crear-cuenta", async (formulario, resultado) => {
  const cuenta = await api("POST", "/api/cuentas", {
    clienteId: numero(valor(formulario, "clienteId")),
    tipoCuentaId: numero(valor(formulario, "tipoCuentaId")),
    monedaId: numero(valor(formulario, "monedaId")),
    saldoInicial: numero(valor(formulario, "saldoInicial")),
  });

  formulario.elements.namedItem("saldoInicial").value = "";
  seleccionarCuenta(cuenta.numeroCuenta);
  mostrarExito(resultado, "Cuenta creada", fichaCuenta(cuenta));
});

// 4. Consultar saldo
alEnviar("form-saldo", async (formulario, resultado) => {
  const numeroCuenta = requerido(valor(formulario, "numeroCuenta"), "Ingrese el número de cuenta.");
  const saldo = await api("GET", `/api/cuentas/${encodeURIComponent(numeroCuenta)}/saldo`);

  seleccionarCuenta(saldo.numeroCuenta);
  resultado.replaceChildren(el("div", { class: "mensaje exito" },
    el("strong", {}, "Saldo actual"),
    el("div", { class: "saldo-grande" }, dinero(saldo.saldo, saldo.simboloMoneda, saldo.moneda)),
    el("div", {}, numeroCuentaEl(saldo.numeroCuenta), " · ", insignia(saldo.estado))));
});

// Funcionalidad adicional: cambiar estado
alEnviar("form-estado", async (formulario, resultado) => {
  const numeroCuenta = requerido(valor(formulario, "numeroCuenta"), "Ingrese el número de cuenta.");
  const ruta = `/api/cuentas/${encodeURIComponent(numeroCuenta)}`;

  // Solo para informar al operador si hubo cambio; la regla la aplica el backend.
  const anterior = await api("GET", `${ruta}/saldo`);
  const cuenta = await api("PATCH", `${ruta}/estado`, {
    estadoCuentaId: numero(valor(formulario, "estadoCuentaId")),
  });

  seleccionarCuenta(cuenta.numeroCuenta);

  if (anterior.estado === cuenta.estado) {
    mostrarInfo(resultado, "Sin cambios", `La cuenta ya estaba ${cuenta.estado}. No se modificó ni se registró auditoría.`);
    resultado.append(fichaCuenta(cuenta));
  } else {
    mostrarExito(resultado, `Estado actualizado: ${anterior.estado} → ${cuenta.estado}`, fichaCuenta(cuenta));
  }
});

// 5 y 6. Depositar o retirar
alEnviar("form-operacion", async (formulario, resultado, boton) => {
  const numeroCuenta = requerido(valor(formulario, "numeroCuenta"), "Ingrese el número de cuenta.");
  const operacion = boton?.value === "retiros" ? "retiros" : "depositos";

  const transaccion = await api("POST", `/api/cuentas/${encodeURIComponent(numeroCuenta)}/${operacion}`, {
    monto: numero(valor(formulario, "monto")),
  });

  formulario.elements.namedItem("monto").value = "";
  seleccionarCuenta(transaccion.numeroCuenta);

  const simbolo = simboloDe(transaccion.moneda);
  mostrarExito(resultado, `${transaccion.tipo} registrado`, ficha([
    ["Transacción", `#${transaccion.id}`],
    ["Cuenta", numeroCuentaEl(transaccion.numeroCuenta)],
    ["Monto", dinero(transaccion.monto, simbolo, transaccion.moneda)],
    ["Saldo resultante", dinero(transaccion.saldoResultante, simbolo, transaccion.moneda)],
    ["Fecha y hora", fechaHora(transaccion.fechaHora)],
  ]));
});

// 7. Historial de transacciones
alEnviar("form-historial", async (formulario, resultado) => {
  const numeroCuenta = requerido(valor(formulario, "numeroCuenta"), "Ingrese el número de cuenta.");
  const historial = await api("GET", `/api/cuentas/${encodeURIComponent(numeroCuenta)}/transacciones`);

  seleccionarCuenta(historial.numeroCuenta);

  const resumen = ficha([
    ["Cuenta", numeroCuentaEl(historial.numeroCuenta)],
    ["Saldo actual", dinero(historial.saldoActual, historial.simboloMoneda, historial.moneda)],
    ["Movimientos", String(historial.transacciones.length)],
  ]);

  if (!historial.transacciones.length) {
    resultado.replaceChildren(resumen, el("p", { class: "nota" }, "La cuenta no tiene movimientos."));
    return;
  }

  resultado.replaceChildren(resumen, tabla(
    [
      { titulo: "Id" },
      { titulo: "Fecha y hora" },
      { titulo: "Tipo" },
      { titulo: "Monto", clase: "numero" },
      { titulo: "Saldo resultante", clase: "numero" },
    ],
    historial.transacciones.map((transaccion) => [
      `#${transaccion.id}`,
      fechaHora(transaccion.fechaHora),
      insignia(transaccion.tipo),
      dinero(transaccion.monto, historial.simboloMoneda, historial.moneda),
      dinero(transaccion.saldoResultante, historial.simboloMoneda, historial.moneda),
    ])));
});

/* ---------------------------------------------------------------- Auditoría */

const formularioAuditoria = document.getElementById("form-auditoria");
const resultadoAuditoria = formularioAuditoria.querySelector(".resultado");
const paginacion = formularioAuditoria.querySelector(".paginacion");

/** Medianoche local del navegador para una fecha AAAA-MM-DD. */
function inicioDelDia(textoFecha) {
  const [anio, mes, dia] = textoFecha.split("-").map(Number);
  return new Date(anio, mes - 1, dia);
}

function filtrosAuditoria() {
  const parametros = new URLSearchParams();
  const desde = valor(formularioAuditoria, "desde");
  const hasta = valor(formularioAuditoria, "hasta");
  const resultado = valor(formularioAuditoria, "resultado");
  const numeroCuenta = valor(formularioAuditoria, "numeroCuenta").trim();

  if (desde) parametros.set("desde", inicioDelDia(desde).toISOString());
  if (hasta) {
    // "Hasta" incluye todo el día seleccionado: se envía la medianoche del día siguiente (exclusivo).
    const finExclusivo = inicioDelDia(hasta);
    finExclusivo.setDate(finExclusivo.getDate() + 1);
    parametros.set("hasta", finExclusivo.toISOString());
  }
  if (resultado) parametros.set("resultado", resultado);
  if (numeroCuenta) parametros.set("numeroCuenta", numeroCuenta);
  parametros.set("tamanoPagina", String(TAMANO_PAGINA_AUDITORIA));

  return parametros.toString();
}

function actualizarPaginacion(pagina, totalPaginas, totalElementos) {
  paginacion.hidden = totalElementos === 0;
  paginacion.querySelector(".paginacion-texto").textContent =
    `Página ${pagina} de ${Math.max(totalPaginas, 1)} · ${totalElementos} registros`;
  paginacion.querySelector("[data-pagina='anterior']").disabled = pagina <= 1;
  paginacion.querySelector("[data-pagina='siguiente']").disabled = pagina >= totalPaginas;
}

async function cargarAuditoria(pagina) {
  const parametros = new URLSearchParams(estado.auditoria.filtros);
  parametros.set("pagina", String(pagina));

  try {
    const datos = await api("GET", `/api/auditoria?${parametros}`);
    estado.auditoria.pagina = datos.pagina;
    estado.auditoria.totalPaginas = datos.totalPaginas;

    if (!datos.elementos.length) {
      mostrarInfo(resultadoAuditoria, "Sin resultados", "No hay eventos de auditoría con esos filtros.");
    } else {
      resultadoAuditoria.replaceChildren(tabla(
        [
          { titulo: "Fecha y hora" },
          { titulo: "Acción" },
          { titulo: "Resultado" },
          { titulo: "Descripción", clase: "texto-largo" },
          { titulo: "Cliente" },
          { titulo: "Cuenta" },
        ],
        datos.elementos.map((evento) => [
          fechaHora(evento.fechaHora),
          textoAccion(evento.accion),
          insignia(evento.resultado),
          evento.descripcion,
          evento.clienteId ?? "—",
          evento.numeroCuenta ? numeroCuentaEl(evento.numeroCuenta) : "—",
        ])));
    }

    actualizarPaginacion(datos.pagina, datos.totalPaginas, datos.totalElementos);
  } catch (error) {
    paginacion.hidden = true;
    if (error instanceof ErrorApi && error.codigo === 404) {
      mostrarInfo(resultadoAuditoria, "No disponible",
        "La auditoría solo está disponible en el entorno local de desarrollo o evaluación.");
    } else {
      mostrarError(resultadoAuditoria, error);
    }
  }
}

formularioAuditoria.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  estado.auditoria.filtros = filtrosAuditoria();
  await cargarAuditoria(1);
});

paginacion.addEventListener("click", async (evento) => {
  const boton = evento.target.closest("[data-pagina]");
  if (!boton || boton.disabled) return;
  const desplazamiento = boton.dataset.pagina === "siguiente" ? 1 : -1;
  await cargarAuditoria(estado.auditoria.pagina + desplazamiento);
});

/* ---------------------------------------------------------------- Pestañas y catálogos */

function activarSeccion(nombre) {
  document.querySelectorAll(".pestana").forEach((pestana) => {
    const activa = pestana.dataset.seccion === nombre;
    pestana.classList.toggle("activa", activa);
    pestana.setAttribute("aria-selected", String(activa));
  });

  document.querySelectorAll(".seccion").forEach((seccion) => {
    seccion.hidden = seccion.id !== `seccion-${nombre}`;
  });

  if (nombre === "auditoria" && !estado.auditoria.filtros) {
    estado.auditoria.filtros = filtrosAuditoria();
    cargarAuditoria(1);
  }
}

document.querySelectorAll(".pestana").forEach((pestana) => {
  pestana.addEventListener("click", () => activarSeccion(pestana.dataset.seccion));
});

function llenarSelect(catalogo, opciones) {
  document.querySelectorAll(`select[data-catalogo='${catalogo}']`).forEach((select) => {
    select.replaceChildren(...opciones.map(([id, texto]) => el("option", { value: id }, texto)));
  });
}

async function cargarCatalogos() {
  try {
    const [tipos, estados, monedas] = await Promise.all([
      api("GET", "/api/catalogos/tipos-cuenta"),
      api("GET", "/api/catalogos/estados-cuenta"),
      api("GET", "/api/catalogos/monedas"),
    ]);

    monedas.forEach((moneda) => estado.monedas.set(moneda.codigo, moneda));
    llenarSelect("tipos", tipos.map((tipo) => [tipo.id, tipo.nombre]));
    llenarSelect("estados", estados.map((estadoCuenta) => [estadoCuenta.id, estadoCuenta.nombre]));
    llenarSelect("monedas", monedas.map((moneda) => [moneda.id, `${moneda.codigo} · ${moneda.nombre} (${moneda.simbolo})`]));
  } catch {
    document.querySelectorAll("select[data-catalogo]").forEach((select) => {
      select.replaceChildren(el("option", { value: "" }, "No se pudo cargar el catálogo"));
    });
  }
}

cargarCatalogos();
