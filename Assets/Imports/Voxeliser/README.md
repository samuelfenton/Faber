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
	-Separated Voxels: Voxels can be made up of completely unique verts or share verts with their neighbours. This will cause the texture to have a hard edge between or a smooth soft transition between them. Secondary effects is the increase in possible mesh size as less vertices are needed.
	- Object With Mesh: This is the object that contains the model mesh. If not assigned it is assumed to be on the same object as the script.
	- Delayed Initialisation: For any particular reason a delay of a single frame is needed before initialisation. Typically used when other meshes are generated at runtime and order of execution is unknown.
	- Save Static Mesh: When building a static mesh, the mesh can be saved for later use rather than using the voxeliser every time. This variable when toggled in the inspector will create a save prompt, save the mesh. This only occurs when using the unity editor.

// Known Issues //
Due to a limitation in unity meshes, there can be no more than 65535 vertices in a single mesh. As each voxel is 8 vertices, this leads to a hard limit of 8191 voxels per object. In the case of needing more, suggested that models are separated into several parts with each being voxelised.
Jobs inherently cannot run for longer than 4 frames, given Voxeliser Burst includes two jobs to complete, when performing over multiple frames it will still be limited to a max of 8 frames, whereas Voxeliser can perform indefinitely.
It currently does not work when using multiple submeshes, it can convert the mesh but will use only the first material.