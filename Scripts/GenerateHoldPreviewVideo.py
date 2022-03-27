"""A standalone script that generates a preview video of the hold.

https://docs.blender.org/api/current/bpy.types.Object.html
https://docs.blender.org/api/current/bpy.types.RenderSettings.html
"""

import os
import bpy
import argparse
import tempfile

from math import radians
from subprocess import Popen

parser = argparse.ArgumentParser()

parser.add_argument("input", help="The path to the model to generate the preview for.")
parser.add_argument("output", help="The path of the resulting file.")

arguments = parser.parse_args()

# load the hold
bpy.ops.import_scene.obj(filepath=arguments.input)

LIGHT_DISTANCE = 5

camera = None
hold = None
light = None
for obj in bpy.data.objects:
    # remove cube
    if obj.name.lower() == "cube":
        bpy.data.objects.remove(obj, do_unlink=True)

    elif obj.name.lower() == "model":
        hold = obj

    elif obj.name.lower() == "camera":
        camera = obj

    # move light somewhere better
    elif obj.name.lower() == "light":
        light = obj
        light.location = (LIGHT_DISTANCE, -LIGHT_DISTANCE, LIGHT_DISTANCE)


def fit_to_object(camera, obj):
    """Zoom the camera to fit the entire object."""
    obj.select_set(True)
    bpy.ops.view3d.camera_to_view_selected()


def remove_files(files):
    """Remove all files in the list."""
    for file in files:
        os.remove(file)


tmp_name = next(tempfile._get_candidate_names())

# this is not 0 for some reason
hold.rotation_euler[0] = 0

# fit the view to the entire hold + scale it down a bit
fit_to_object(camera, hold)
obj.scale = (0.75, 0.75, 0.75)

rotation_steps = 120
rotation_angle = 360

bpy.context.view_layer.update()

bpy.context.scene.render.film_transparent = True
bpy.context.scene.render.image_settings.file_format = "JPEG"
bpy.context.scene.render.image_settings.quality = 80

bpy.context.scene.render.resolution_x = 1280
bpy.context.scene.render.resolution_y = 720

try:
    frames = []
    frames_format = os.path.join("/tmp", f"{tmp_name}*.jpg")
    for step in range(rotation_steps):
        hold.rotation_euler[2] = radians(step * (rotation_angle / rotation_steps))

        frames.append(os.path.join("/tmp", f"{tmp_name}{step:03}.jpg"))

        bpy.context.scene.render.filepath = frames[-1]
        bpy.ops.render.render(write_still=True)

    Popen(
        [
            "ffmpeg",
            "-framerate",
            "60",
            "-pattern_type",
            "glob",
            "-i",
            frames_format,
            "-c:v",
            "libx264",
            "-pix_fmt",
            "yuv420p",
            arguments.output,
        ]
    ).communicate()
except KeyboardInterrupt:
    remove_files(frames)

remove_files(frames)
