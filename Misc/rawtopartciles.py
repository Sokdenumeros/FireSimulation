import numpy as np
import os
import math

smokedir = input('Enter smoke directory\n')
heatdir = input('Enter heat directory\n')

xcoords = np.fromfile('gridX.raw', dtype=dtype)
ycoords = np.fromfile('gridY.raw', dtype=dtype)
zcoords = np.fromfile('gridZ.raw', dtype=dtype)

smokefiles = os.listdir(smokedir)[3:]
heatfiles = os.listdir(heatdir)[3:]

for i, val in enumerate(smokefiles):
	smokename = smokefiles[i]
	heatname = heatfiles[i]
	if smokename.split('_')[0] != heatname.split('_')[0]:
		print('----------------------------')
		print(smokename)
		print(heatname)

	shape = tuple([int(i) for i in smokename.split('.')[-4:-1]])
	dtype = np.float32

	smoke_data = np.fromfile(name, dtype=dtype).reshape(shape)
	smoke_data = np.nan_to_num(smoke_data,0)

	heat_data = np.fromfile(name2, dtype=dtype).reshape(shape)
	heat_data = np.nan_to_num(heat_data,0)

	particles = []
	smoke = []
	heat = []

	for z_index, z_value in enumerate(zcoords):
		for y_index, y_value in enumerate(ycoords):
			for x_index, x_value in enumerate(xcoords):
				if smoke_data[z_index,y_index,x_index] > 100:
					particles.append((z_value,y_value,x_value))
					smoke.append(smoke_data[z_index,y_index,x_index])
					heat.append(heat_data[z_index,y_index,x_index])

	particles.astype(np.uint32).tofile('particles/'+smokename.split('_')[0]+'.raw')
	smoke.astype(np.float32).tofile('smoke/'+smokename.split('_')[0]+'.raw')
	heat.astype(np.float32).tofile('heat/'+smokename.split('_')[0]+'.raw')