# PIN Block DLL, español/english
Obtiene una DLL para crear un PIN Block, al estilo de C++ stdcall (Pero mas sencillo ;)). Muy util para integrarse con un HSM (como Thales) el resultado puedes enviarlo por Base24.

Create a DLL to get PIN Block like C++ stdcall (but easier ;) ). Usefull to integrate with an HSM (like Thales), the result can be send by Base24.

Esta DLL la use en un proyecto en Delphi, abajo un ejemplo de carga y ejecucion

This DLL was implemented in a Delphi Project, below a example and use.

>Por supuesto la DLL puede usarse con cualquier lenguaje/Of course you can use with any language 

### Delphi example

## vars and types
``` 
_TPINBlockDLL   = function(PIN, PAN, Llave3DES : PChar; out PINBlock : Pointer): Integer ; stdcall;

_F_PINBlockDLL : _TPINBlockDLL  ; 
_Handle_: THandle;
PIN, PAN, Llave3DES : string;
p : Pointer;
i : integer;

```
## Cargar DLL y metodo /Load DLL and function 
``` 
_Handle_:= LoadLibrary('PinBlockDLL.dll');
  if _Handle_ <> 0 then
    _F_PINBlockDLL:= GetProcAddress(_Handle_, 'PINBlock');
    
```
 
### Ejecutar metodo/ Gets PIN Block
``` 
if @_F_PINBlockDLL <> nil then begin
      i := _F_PINBlockDLL( PChar(PIN), PChar(PAN), PChar(Llave3DES), p);
      if i > 0 then begin
        PIN_BLOCK := PChar(p);
        SetLength(PIN_BLOCK, i);
      end;
      Result := Length(PIN_BLOCK) = 16;
```
