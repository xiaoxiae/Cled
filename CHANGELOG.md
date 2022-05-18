# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).


## [2.3.2] - TODO

### Changed
- Prevent the editor from quitting when not saved.


## [2.3.1] - 2022-05-11

### Changed
- Improve marker positioning performance.
- Improve Ctrl+A performance in hold picker.
- Improve route view click behavior.
- Improve toolbar styling.
- Improve movement consistency across multiple platforms.
- Change initial window size.
- Slightly adjust icon and logo assets.
- Fix repeated project openings crashing.
- Fix cursor snapping to application after initialization.
- Fix marker position not updating when flipping hold.
- Fix save file being overwritten when export exception occurs.
- Fix escape not triggering popup ok actions.
- Fix UI bugging out on keyboard presses.


## [2.3.0] - 2022-05-05

### Changed
- Improve code quality and documentation.
- Improve application speed (lots of arrays/lists to IEnumerables).
- Allow switching holds in normal mode.
- Fix player position not being correctly reset when failing to load.
- Fix toolbar popping up when failing to load.
- Fix light intensity and shadow strength not being correctly updated when importing.
- Fix wall metadata not being properly initialized when the metadata file is not present.
- Fix hold starting/ending marker twitch when being added to a hold.
- Don't deinitialize when failing to import on the preferences step.


## [2.2.0] - 2022-05-05

### Added
- Add bottom toolbar with currently filtered holds.

### Changed
- Improve hold picker UI.
- Fix total hold counter being always set to 0.
- Fix general popup position being incorrect.
- Rename Total selected holds label to just Selected holds.

### Removed
- Remove filtered selected label.


## [2.1.0] - 2022-05-05

### Changed
- Make starting player position be (0, 0, 0) at the bottom of its model.
- Deselect non-filtered holds after filtering changes.
- Deselect all holds when Ctrl+A is pressed and all are selected.
- Fix editor not being de-initialize when failing to initialize.
- Fix holds not being removed from starting/ending when deleted.
- Fix hold position not being properly set when moving the mouse wheel.


## [2.0.0] - 2022-05-04

### Added
- Add horizontal flipping using the H button.
- Add toolbar View > Hold menu.

### Changed
- Improve popup UI.
- Include current version in help menu.
- Don't sort hold picker holds by type.
- Make Ctrl+any button work when toggling a hold in route mode.
- Fix hold state not being preserved when swapping using mouse wheel.
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
