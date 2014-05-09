UnityTricks
===========

Utils Importer
-----------

add mesh to bones marked by name.

mesh must be in a separate blend file named xxxUtils.blend where xxx is the name of the referencing blend file.

bone must be named Utils_Mesh_nnn where nnn is the name of the linked object in xxxUtils.blend. Bone names with trailing _L _R _l _r .001 .002 .003 will also work

The bones must have the deform flag set, otherwise they might not get exported.

When changing names and reimporting check Unitys Mask option if you get errors with the animations and if you are using Mecanim (in Project window under Animations)


known issues:
  * the bone must have a length of 1 in edit mode for the it to appear scaled correctly.
  * the objects transforms in the xxxUtils.blend will not be imported correctly. theyre mesh is rotated and theyre transforms aren't. Doesn't really matter does it?!
  * if the Optimize Game Objects option is used the bones with mesh must be exposed.

