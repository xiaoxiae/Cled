# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).


## [2.1.0] - TODO

### Changed
- Make starting player position be (0, 0, 0) at the bottom of its model.


## [2.0.0] - 2022-05-04

### Added
- Add horizontal flipping using the H button.
- Add toolbar View > Hold menu.

### Changed
- Improve popup UI.
- Include current version in help menu.
- Don't sort hold picker holds by type.
- Make Ctrl+any button work when toggling a hold in route mode.
- Fix hold state not being perserved when swapping using mouse wheel.
- Fix bug with holds not being properly selected with Ctrl+A.


## [1.1.2] - 2022-05-03

### Changed
- Reset player position when under a certain threshold.
- Disable capturing images while not initialized.
- Fix capturing images being broken in build.
- Fix player position not being reset after loading.
- Fix hold picker button growing.
- Fix Ctrl+Shift+P calling regular capture.


## [1.1.1] - 2022-04-30

### Changed
- Automatically pause uninitialized editor.
- Remove "Deselect filtered" button.
- Improve hold picker look and behavior on smaller screens.
- Improve Scripts/ClisImporter.py behavior.
- Increase route highlight thickness.
- Fix preferences being inaccessible behind the pause screen.
- Fix raycasting being applied to starting hold markers.


## [1.1.0] - 2022-04-29

### Added
- A route view window with the list of the project routes.
- Loading screen when opening or creating a new project.

### Changed
- Improve error messages.
- Improve hold picking behavior with the currently held holds.
- Allow setting top/bottom marks while holding.
- Enter pressing accepts popups and settings.
- Fix Ctrl+A shortcut not selecting all filtered holds.
- Fix a bugged exported state when deleting holds with markers.


## [1.0.0] - 2022-04-26

### Added
- Initial editor functionality.
