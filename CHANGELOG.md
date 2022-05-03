# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).


## [1.1.3] - 2022-05-04

### Changed
- Don't sort hold picker holds by type.


## [1.1.2] - 2022-05-03

### Changed
- Reset player position when under a certain treshold.
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
