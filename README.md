# Galio Flash Tool — Station/Monitor

Aplicación de campo para cargar firmware Intel HEX en ProtoLink Station/Monitor
(PIC18F67K40, bootloader `BOOT:1.2`, circuito de dos NPN cruzados).
Windows 10/11, .NET Framework 4.7.2. No requiere Python ni compilador de firmware.

## Uso

1. Seleccionar el COM en la pestaña **Program** y pulsar **Connect**. La herramienta ejecuta Auto Boot una sola vez y escucha la identificación del equipo. En placas con entrada manual, accionar BOOT y RESET mientras el puerto está abierto.
2. Seleccionar el HEX del equipo. Con el archivo válido y el bootloader identificado, pulsar **Cargar**. La transferencia utiliza el mismo puerto, que se libera al finalizar. Para el siguiente equipo, volver a conectar.
3. Para ver mensajes de depuración, pulsar **Connect** desde **Terminal avanzada**. Cambiar de pestaña conserva la conexión existente. Para cambiar su función, desconectar y conectar desde la pestaña deseada.

El resultado distingue carga aceptada de aplicación iniciada. Un firmware sin
la línea `INFO app=... fw=...` puede haberse iniciado aunque no se confirme.
El protocolo no ofrece lectura de flash: ACK no equivale a verificación por lectura.
Una desconexión, cancelación o respuesta inválida requiere repetir la carga desde
el inicio; no hay reintentos automáticos de registros individuales. Cerrar la
ventana cancela cooperativamente y libera el puerto.

## Perfil y protecciones

- Solo Intel HEX; BIN no está soportado.
- Checksum, longitud, tipos 00/01/04, EOF, solapamientos y vector en 0x0600.
- Aplicación: 0x0600..0x1FFFF. Se rechazan registros del bootloader y otras regiones.
- Fuses 0x300000..0x30000D se excluyen; se conservan los instalados por ICSP.
- Normalización ascendente: hasta 26 bytes de datos, sin cruzar filas de 128 bytes,
  huecos rellenados con FF hasta el último bloque de 16 bytes usado. Cada línea
  ocupa como máximo 64 bytes contando CR.
- Cada registro exige XOFF/ACK/XON (`13 06 11`); EOF exige además `06 11`.
- Se admite cualquier nombre y versión que anuncie el banner del bootloader. El protocolo de confirmación sigue siendo el mismo.

### Placas de cliente

Edite `placas.ini` junto al ejecutable y vuelva a conectar. La sección `[General]` se usa cuando no hay una entrada para la pareja nombre/versión. Para personalizar una placa, copie el ejemplo del INI y use `[nombre exacto | BOOT:versión]`. Indique el inicio y fin inclusivo de la aplicación en hexadecimal según su mapa de memoria. El nombre del micro es informativo. El archivo incluye el mapa original como valor general y no detecta la capacidad física del PIC.

El HEX por sí mismo no certifica el modelo funcional, cliente o versión de la
aplicación. El operador debe seleccionar el firmware de producción correcto.
El SHA-256 identifica el archivo cargado; no constituye una firma de autenticidad.

## Autoboot

Antes de abrir: DTR=False, RTS=False, sin handshake automático.
Reposo 50 ms; RTS=True (RESET) 200 ms; DTR=True y RTS=False consecutivos
(BOOT), mantener 200 ms; DTR=False. Capturar RX durante las esperas, sin purgar
tras liberar RESET. Esperar al menos 1050 ms desde el banner completo antes del
primer registro. Identificación: hasta 3 s; respuesta por registro: 3 s;
confirmación final: 2 s; identificación de aplicación: 5 s.

Los tiempos proceden de `protolink/docs/autoboot.md` y del cargador Python
`tools/station_upload.py`. La versión C# se validó en COM26 con station-dht22; ver la validación física abajo.

## Registros

Cada operación crea un archivo en `%LOCALAPPDATA%\Galio\FlashTool\logs`.
Incluye fecha con zona horaria, COM, perfil, ruta del HEX, SHA-256, bytes,
fuses excluidos, banner, resultado y errores. La ruta se muestra al terminar.
El archivo se valida y se guarda en memoria antes de abrir el puerto, de modo que
un cambio posterior en disco no altera los datos enviados ni su huella registrada.

## Desarrollo y pruebas

Abrir `Galio Flash Tool.sln` en Visual Studio con soporte .NET Framework 4.7.2.
Compilar Release mediante MSBuild. Ejecutar `powershell -File tests/run.ps1` para
pruebas sin hardware del parser, límites de memoria, empaquetado, autoboot,
respuestas serie, tiempos y cancelación. El transporte simulado entrega bytes
individuales y confirma inmediatamente las escrituras para detectar regresiones.

Antes de distribuir, probar en una placa de banco: carga correcta, segunda carga,
HEX inválido, puerto ocupado, desconexión durante transferencia y cierre de la
ventana. Comprobar la aplicación esperada tras reinicio. Se realizaron dos cargas físicas correctas; las pruebas de desconexión y cierre
durante transferencia quedan pendientes en hardware.

Validación automatizada actual: 23 comprobaciones del núcleo y una prueba de
construcción/render de WinForms (`tests/ui-smoke.ps1`, ejecutar desde la raíz en
Windows PowerShell STA después de compilar Release). El render se guarda en
`tests/bin/form.png`. Esta prueba no abre ningún puerto serie.

## Validación física — 2 de noviembre de 2026

Dos cargas consecutivas desde la interfaz en COM26 a 115200, seleccionando
`G:\Proyectos\PIC\protolink\build\station-dht22\main.hex`:

- SHA-256: `BE9CD47070FDBC3C84221E18CA924831A968E2DCB2371EB4E8A944BA159677BD`.
- 8034 bytes de aplicación, 12 bytes de fuses excluidos, 317 registros enviados.
- Bootloader `Protolink-Station-Monitor BOOT:1.2` identificado.
- Confirmaciones de transferencia y EOF completas.
- Reinicio observado: `INFO app=station-dht22 fw=0.1.0`, reset `0x0333`.
- Aproximadamente 5.0 s hasta carga aceptada y 6.0 s hasta identificación de arranque.

Los primeros dos intentos con propiedades `SerialPort.DtrEnable/RtsEnable` no
entraron al loader y no transmitieron HEX. La carga usa ahora `NativeLoaderPort`,
que configura una vez el puerto y modifica las líneas con `EscapeCommFunction`,
como pySerial. Mantiene su propio SafeFileHandle durante autoboot, carga y arranque;
no utiliza reflexión ni un proceso Python. La terminal conserva SerialPort y se
cierra antes de adquirir el puerto exclusivo de carga.

Referencia de la diferencia: [SerialStream de .NET Framework](https://github.com/microsoft/referencesource/blob/main/System/sys/system/IO/ports/SerialStream.cs)
y [EscapeCommFunction](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-escapecommfunction).
No se midieron señales con osciloscopio ni se verificó la lectura del sensor DHT22.
