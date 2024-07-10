import numpy as np
import os
import math

nparticles = 1000000
opacityThreshold = 4
heatThreshold = 0
smokeparticlesratio = 0.8 #this means that this fraction of all particles will be smoke particles

particleDir = 'particles'
particleDataDir = 'partData'

smokedir = input('Enter smoke directory\n')
heatdir = input('Enter heat directory\n')

xcoords = np.fromfile(smokedir + '/gridX.raw', dtype=np.float32)
ycoords = np.fromfile(smokedir + '/gridY.raw', dtype=np.float32)
zcoords = np.fromfile(smokedir + '/gridZ.raw', dtype=np.float32)

smokefiles = os.listdir(smokedir)[3:]
heatfiles = os.listdir(heatdir)[3:]

try:
	os.mkdir(particleDir)
except:
	pass

try:
	os.mkdir(particleDataDir)
except:
	pass

for i, val in enumerate(smokefiles):
	smokename = smokefiles[i]
	heatname = heatfiles[i]
	print(val, end="\r")
		
	if smokename.split('_')[0] != heatname.split('_')[0]:
		print('----------------------------')
		print(smokename)
		print(heatname)

	shape = tuple([int(i) for i in smokename.split('.')[-4:-1]])

	datType = np.float16
	if smokename[-1] == '8':
		datType = np.uint8

	smoke_data = np.fromfile(smokedir + '/' + smokename, dtype=datType).reshape(shape)
	smoke_data = np.nan_to_num(smoke_data,0)

	heat_data = np.fromfile(heatdir + '/' + heatname, dtype=datType).reshape(shape)
	heat_data = np.nan_to_num(heat_data,0)

	if i+1 < len(smokefiles):
		next_smoke_data = np.fromfile(smokedir + '/' + smokefiles[i+1], dtype=datType).reshape(shape)
		next_smoke_data = np.nan_to_num(next_smoke_data,0)

		next_heat_data = np.fromfile(heatdir + '/' + heatfiles[i+1], dtype=datType).reshape(shape)
		next_heat_data = np.nan_to_num(next_heat_data,0)

	particles = []
	smoke = []
	heat = []
	zvals = []
	yvals = []
	xvals = []
	
	#Coordinates of cells that fulfill the condition
	aux = np.argwhere(smoke_data > opacityThreshold)
	fireaux = np.argwhere(heat_data > heatThreshold)

	#Choose one subsample of the coordiantes at random
	if len(fireaux) > 0:
		fireaux = fireaux[np.random.choice(len(fireaux),min(int(nparticles*(1-smokeparticlesratio)),len(fireaux)),replace = False)]

	if len(aux) > 0:
		aux = aux[np.random.choice(len(aux),  min( max( int(nparticles*smokeparticlesratio) ,nparticles-len(fireaux) ) , len(aux))  ,replace = False)]
	
	aux = np.concatenate((fireaux,aux))
	if len(aux) > 0:
		aux = aux[np.lexsort(aux.T)]

	particles = aux[:, 0] * len(ycoords) * len(xcoords) + aux[:, 1] * len(xcoords) + aux[:, 2]
	smoke = smoke_data[tuple(aux.T)]
	heat = heat_data[tuple(aux.T)]
	next_smoke = next_smoke_data[tuple(aux.T)]
	next_heat = next_heat_data[tuple(aux.T)]

	compressed_data = np.clip(np.array(smoke).astype(np.uint32),a_min = 0, a_max = 255) << 24
	compressed_data = (np.clip(np.array(heat).astype(np.uint32),a_min = 0, a_max = 255) << 16) | compressed_data
	compressed_data = (np.clip(np.array(next_smoke).astype(np.uint32),a_min = 0, a_max = 255) << 8) | compressed_data
	compressed_data = (np.clip(np.array(next_heat).astype(np.uint32),a_min = 0, a_max = 255))| compressed_data

	tim = float(smokename.split('_')[0][1:])
	np.array(particles).astype(np.uint32).tofile(particleDir+'/'+ f'T{tim:.1f}' +'.raw')
	compressed_data.tofile(particleDataDir+'/'+ f'T{tim:.1f}' +'.raw')