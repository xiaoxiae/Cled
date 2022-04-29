<p align="center" width="100%">
<img width="50%" src="https://raw.githubusercontent.com/Climber-Apps/Cled/master/logo.svg">
</p>

<p align="center" width="100%">
The <strong>cli</strong>mber's <strong>ed</strong>itor – a 3D editor designed for efficient virtual routesetting.
</p>

This repository contains the source code to the Cled editor, along with various scripts used to import assets used from [Clis](https://github.com/Climber-Apps/Cled/), released under the GPLv3 license.

## Running Cled
First, download the lastest version from the [Releases page](https://github.com/Climber-Apps/Cled/releases).

To use Cled, you need a wall and holds dataset.
A dataset example, along with the specification, can be found in `Models/Example`.
This dataset can then be imported to Cled either by `File > New`, or by pressing `Ctrl+N`.

For additional information, see the [thesis](https://github.com/Climber-Apps/Thesis).

### Key bindings
While most of the controls are accessible via the UI, here is a comprehensive list of key bindings for efficiently controlling the editor:

#### Movement
|         |                                  |
| ---     | ---                              |
| `WSAD`  | move forward/backward/left/right |
| `space` | fly upward                       |
| `shift` | fly downward                     |

#### UI
|                                        |                          |
| ---                                    | ---                      |
| `Esc`                                  | pause/cancel             |
| `Enter`                                | confirm                  |
| `Q` or `Tab`                           | open holds menu          |
| right button on selected route (ROUTE) | open route settings menu |

#### Editing
|                                 |                                                 |
| ---                             | ---                                             |
| left button                     | pick up/place the hovered/held hold             |
| right button                    | select the hovered route                        |
| `E`                             | toggle between NORMAL and EDITING               |
| `R` or `Delete`                 | delete hovered/held hold                        |
| `Ctrl+R` or `Ctrl+Delete`       | delete hovered route/route containing held hold |
| middle button (EDITING) + mouse | rotate held hold                                |
| `T`/`B`                         | toggle hovered hold as ending/starting          |
| control + left click (ROUTE)    | toggle hovered hold as being in the route       |
| wheel up/down                   | cycle filtered holds                            |

#### Import/Export
|                |                            |
| ---            | ---                        |
| `Ctrl+N`       | open new dataset           |
| `Ctrl+O`       | open existing Cled project |
| `Ctrl+S`       | save project               |
| `Ctrl+Shift+S` | save project as            |
| `Ctrl+Q`       | quit                       |

#### Images
|                |                  |
| ---            | ---              |
| `Ctrl+P`       | capture image    |
| `Ctrl+Shift+P` | capture image as |

#### Lighting
|          |                                    |
| ---      | ---                                |
| `F`      | toggle user light                  |
| `Ctrl+F` | add new light at the user position |
