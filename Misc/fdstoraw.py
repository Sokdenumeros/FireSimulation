import numpy as np
import fdsreader

# Load FDS simulation
sim = fdsreader.Simulation('./NIST_PoolFire/')
slc = sim.slices[3]

# Get temperature data and grid
temp, grid = slc.to_global(return_coordinates=True, masked=True)

# Get the times
times = slc.times
#0 2 3 1 maybe?
temp= np.transpose(temp, (0, 3, 2, 1))

# Get the shape of the temperature array
shape = temp.shape
# Loop through each time step
for i, time in enumerate(times):
    # Flatten the temperature data
    flattened_temp = temp[i].flatten()

    # Write to a raw file for each time step, format of the filename is the following dimZ.dimY.dimX
    flattened_temp.astype(np.float32).tofile('temperature' + f'_T{time:.1f}_.' + str(shape[1]) + '.' + str(shape[2]) + '.' + str(shape[3]) + '.raw')

uVelocity, grid = sim.slices[0].to_global(return_coordinates=True, masked=True)
vVelocity, grid = sim.slices[1].to_global(return_coordinates=True, masked=True)
wVelocity, grid = sim.slices[2].to_global(return_coordinates=True, masked=True)

velocities = np.stack([uVelocity, vVelocity, wVelocity], axis=4)
#0 2 3 1 4 maybe?
velocities = np.transpose(velocities, (0, 3, 2, 1, 4))

shape = velocities.shape

for i, time in enumerate(times):

    flattened_velocity = velocities[i].flatten()
    flattened_velocity.astype(np.float32).tofile('velocity' + f'_T{time:.1f}_.' + str(shape[1]) + '.' + str(shape[2]) + '.' + str(shape[3]) + '.raw')