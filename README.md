<p align="center" width="100%">
<img width="50%" src="https://raw.githubusercontent.com/Climber-Apps/Cled/master/logo.svg">
</p>

<p align="center" width="100%">
The <strong>cli</strong>mber's <strong>ed</strong>itor – a 3D editor designed for efficient virtual routesetting.
</p>

This repository contains the source code to the Cled editor, along with various scripts used to import assets used from [Clis](github.com/climber-Apps/Clis).

## Running Cled
TODO

## Controls
The application has three modes (the current one being on the top right):

- **NORMAL** – the default mode you're in
- **EDITING** – the mode when you're in when you're placing a hold
- **ROUTE** – the mode you're in when you're editing a route

### Key bindings

| Key                                  | Action                                        |
| ---                                  | ---                                           |
| left click                           | pick up/place the hovered/held hold           |
| right click                          | select the hovered route                      |
|                                      |                                               |
| escape                               | opens the escape menu                         |
| `e`                                  | toggles between NORMAL and EDITING            |
| `tab` or `q`                         | opens holds menu                              |
|                                      |                                               |
| `d` or `delete`                      | delete the hovered hold                       |
| shift + left press + mouse (EDITING) | rotate the held hold                          |
| `t`/`b`                              | toggle the hovered hold as ending/starting    |
| control + left click (ROUTE)         | toggle the hovered hold as being in the route |
| wheel up/down                        | cycle the selected holds                      |

