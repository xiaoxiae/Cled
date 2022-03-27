"""A standalone script that generates preview images of the hold.

https://docs.blender.org/api/current/bpy.types.Object.html
https://docs.blender.org/api/current/bpy.types.RenderSettings.html
"""

import os
import bpy, bmesh
import argparse
import tempfile

from PIL import Image
from math import radians

parser = argparse.ArgumentParser()

parser.add_argument("input", help="The path to the model to generate the preview for.")
parser.add_argument("output", help="The path of the resulting folder containing the images.")
parser.add_argument("-c", "--color", help="The background color. Defaults to '0.5 0.5 0.5 1'.", type=lambda x: tuple(map(float, x.strip().split())), default=(0.5, 0.5, 0.5, 1))
parser.add_argument("-q", "--quality", help="The resulting image quality. Defaults to 40.", type=int, default=40)
parser.add_argument("-s", "--size", help="The resulting image size. Defaults to 1000 (by 1000).", type=int, default=1000)
parser.add_argument("-n", "--number", help="The number of images to take. defaults to 60.", type=int, default=60)

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


def fit_to_object(obj):
    """Zoom the camera to fit the entire object."""
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.ops.view3d.camera_to_view_selected()


def remove_files(files):
    """Remove all files in the list."""
    for file in files:
        os.remove(file)

bpy.data.cameras["Camera"].sensor_width=36
bpy.data.cameras["Camera"].sensor_height=36

# add a background color using the plane
bpy.ops.mesh.primitive_plane_add()

print(arguments.color)

color = arguments.color
if len(arguments.color) == 3:
    color = tuple(list(arguments.color) + [1])

for obj in bpy.data.objects:
    if obj.name.lower() == "plane":
        mat = bpy.data.materials.new(name="Material")
        obj.data.materials.append(mat)
        bpy.context.object.active_material.diffuse_color = color

# this is not 0 for some reason
hold.rotation_euler[0] = 0

# fit the view to the entire hold + scale it down a bit
fit_to_object(hold)
obj.scale = (0.85, 0.85, 0.85)

rotation_steps = arguments.number
rotation_angle = 360

bpy.context.view_layer.update()

bpy.context.scene.render.image_settings.file_format = "JPEG"
bpy.context.scene.render.image_settings.quality = arguments.quality

bpy.context.scene.render.resolution_x = arguments.size
bpy.context.scene.render.resolution_y = arguments.size

if not os.path.exists(arguments.output):
    os.mkdir(arguments.output)

for step in range(rotation_steps):
    hold.rotation_euler[2] = radians(step * (rotation_angle / rotation_steps))

    output_path = os.path.join(arguments.output, f"{step:03}")

    bpy.context.scene.render.filepath = output_path
    bpy.ops.render.render(write_still=True)
