"""A standalone script that generates a preview video and image of a hold.

https://docs.blender.org/api/current/bpy.types.Object.html
https://docs.blender.org/api/current/bpy.types.RenderSettings.html
"""

import os
import bpy, bmesh
import argparse
import tempfile
import shutil

from typing import *
from subprocess import Popen
from PIL import Image
from math import radians, sqrt

parser = argparse.ArgumentParser()

parser.add_argument("input", help="The path to the model to generate the preview for.")
parser.add_argument("output", help="The path of the resulting file (excluding extensions).")
parser.add_argument(
    "-c",
    "--color",
    help="The background color. Defaults to '0.5 0.5 0.5 1'.",
    type=lambda x: tuple(map(float, x.strip().split())),
    default=(0.5, 0.5, 0.5, 1),
)
parser.add_argument(
    "-q",
    "--quality",
    help="The resulting image quality. Defaults to 60.",
    type=int,
    default=60,
)
parser.add_argument(
    "-s",
    "--size",
    help="The resulting image size. Defaults to 1000 (by 1000).",
    type=int,
    default=1000,
)
parser.add_argument(
    "-n",
    "--number",
    help="The number of images to take. defaults to 120.",
    type=int,
    default=120,
)
parser.add_argument(
    "-f",
    "--framerate",
    help="The video framerate. Defaults to 60.",
    type=int,
    default=60,
)
parser.add_argument(
    "-l",
    "--light",
    help="The intensity of the distance of the light to the object. Defaults to 7.",
    type=int,
    default=7,
)

arguments = parser.parse_args()

# load the hold
bpy.ops.import_scene.obj(filepath=arguments.input)

camera = None
hold = None
light = None
for obj in bpy.data.objects:
    # remove cube
    if obj.name.lower() == "cube":
        bpy.data.objects.remove(obj, do_unlink=True)

    elif obj.name.lower() == "camera":
        camera = obj

    elif obj.name.lower() == "light":
        light = obj
        # move light somewhere better
        light.location = (arguments.light, -arguments.light, arguments.light)

    else:
        hold = obj


def fit_to_object(obj):
    """Zoom the camera to fit the entire object."""
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.ops.view3d.camera_to_view_selected()


def remove_files(files):
    """Remove all files in the list."""
    for file in files:
        os.remove(file)


tmp_name = next(tempfile._get_candidate_names())

# the sensor should be rectangular
bpy.data.cameras["Camera"].sensor_width = 36
bpy.data.cameras["Camera"].sensor_height = 36

# add a background color using the plane
bpy.ops.mesh.primitive_plane_add()

color = arguments.color
if len(arguments.color) == 3:
    color = tuple(list(arguments.color) + [1])

for obj in bpy.data.objects:
    if obj.name.lower() == "plane":
        mat = bpy.data.materials.new(name="Material")
        obj.data.materials.append(mat)
        bpy.context.object.active_material.diffuse_color = color

        obj.delta_scale = [10000] * 3

# this is not 0 for some reason
hold.rotation_euler[0] = 0

rotation_steps = arguments.number
rotation_angle = 360

def distance_to_origin(l: List):
    return sqrt(sum([x ** 2 for x in l]))

# determine the optimal camera distance so that the hold is always visible
max_dist = 0
max_dist_camera_location = None
for step in range(rotation_steps + 1):
    hold.rotation_euler[2] = radians(step * (rotation_angle / rotation_steps))

    fit_to_object(hold)

    if max_dist < distance_to_origin(camera.location):
        max_dist = distance_to_origin(camera.location)
        max_dist_camera_location = list(camera.location)
        print(camera.location, max_dist_camera_location)

# fit the view to the entire hold + scale it down a bit
camera.location = max_dist_camera_location

hold.delta_scale = (0.85, 0.85, 0.85)

bpy.context.view_layer.update()

bpy.context.scene.render.image_settings.file_format = "JPEG"
bpy.context.scene.render.image_settings.quality = arguments.quality

bpy.context.scene.render.resolution_x = arguments.size
bpy.context.scene.render.resolution_y = arguments.size

try:
    frames = []
    frames_glob = os.path.join("/tmp", f"{tmp_name}*.jpg")

    for step in range(rotation_steps):
        hold.rotation_euler[2] = radians(step * (rotation_angle / rotation_steps))

        frames.append(os.path.join("/tmp", f"{tmp_name}{step:03}.jpg"))

        bpy.context.scene.render.filepath = frames[-1]
        bpy.ops.render.render(write_still=True)

        if step == 0:
            shutil.copy(frames[-1], arguments.output + "-preview.jpg")

    # we're exporting to webm because the Linux editor doesn't support too many formats
    # https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
    Popen(
        [
            "ffmpeg",
            "-framerate",
            str(arguments.framerate),
            "-pattern_type",
            "glob",
            "-i",
            frames_glob,
            "-c:v",
            "libvpx",
            "-b:v",
            "1M",
            "-c:a",
            "libvorbis",
            arguments.output + "-preview.webm",
        ]
    ).communicate()
except KeyboardInterrupt:
    remove_files(frames)

remove_files(frames)
