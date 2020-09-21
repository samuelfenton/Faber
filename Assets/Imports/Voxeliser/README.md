Voxeliser and Voxelier_Burst are copyright (c) 2019 Samuel Fenton

// Getting Started //
- The voxeliser has two variants, Voxeliser and Voxeliser_Burst, the latter requiring the Burst, Mathematics and Collections packages as specified.
- Each perform the same operation however Burst allows for a 3-5 times increase in performance.
- The voxeliser script can only affect one model at a time.
- Once applied each voxeliser has six variables
	- Voxel Size: How large each voxel should be in units.
	- Voxeliser Type: The type of voxelising should occur
		-Solid: Works on any mesh that uses a mesh filter. Will snap voxels onto a world grid regardless of transformation.
		-Dynamic Solid: Works on any mesh that uses a mesh filter. Designed for a mesh filter that will undergo modifications, such as movement of vertices via animation or code. 
		-Animated: Works on models that use a skinned mesh renderer, typically for animated models. Will snap voxels onto a world grid regardless of transformation.
		-Static: Creates a static mesh intended to remain in a static position or simply as a conversion. At initialisation will make the correct model mesh, but after, any transformations wont cause voxels to snap to grid. However this leads to little or no overhead.  
	- Perform Over Frames: In assisting with optimisation, calculations can occur over several frames as needed. This can lead to smoother gameplay but add more stutter to any animation.  
	- Object With Mesh: This is the object that contains the model mesh. If not assigned it is assumed to be on the same object as the script.
	- Delayed Initialisation: For any particular reason a delay of a single frame is needed before initialisation. Typically used when other meshes are generated at runtime and order of execution is unknown.
	- Total Mesh Count: Due to the limitation  of max vertices per mesh, several meshes can be used at once to get extra large or complex models
	- Vert type: Change the behaviour of how the verts are created
		-Soft Edge: Voxels are made like a normal cube, 8 verts that are shared on each side. This can lead to some minor lighting artifacts due to the corner normal not being aligned with the face normal, but the average of the three connected faces.
		-Hard Edge: This increases the count of verts from 8 => 24, 4 for every face. This removes the lighting artifacts as each verts normal now follows the faces normal.
	- Save Static Mesh: When building a static mesh, the mesh can be saved for later use rather than using the voxeliser every time. This variable when toggled in the inspector will create a save prompt, save the mesh. This only occurs when using the unity editor.
	- Reset Save rotaion: When saving a static mesh, should the rotation be ignored?

// Known Issues //
Jobs inherently cannot run for longer than 4 frames, given Voxeliser Burst includes two jobs to complete, when performing over multiple frames it will still be limited to a max of 8 frames, whereas Voxeliser can perform indefinitely.
It currently works when using multiple submeshes, however will use only the first material on each individual submesh.
When using Unity 2017.4 version, you are unable to save a static mesh. This feature will be added at a later date.
When using Unity 2017.4 version, errors will occur when first installing. This is due to the assembly file looking for packages it can't find due to being out of date. Simply deleting the assembly file, or disabling the GUID checkbox will fix this issue.   