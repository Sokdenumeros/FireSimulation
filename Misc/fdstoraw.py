import numpy as np
import fdsreader

# Load FDS simulation
sim = fdsreader.Simulation('./NIST_PoolFire/')
slc = sim.slices[3]

# Get temperature data and grid
temp, grid = slc.to_global(return_coordinates=True, masked=True)

# Get the times
times = slc.times

temp= np.transpose(temp, (0, 3, 2, 1))

# Loop through each time step
for i, time in enumerate(times):
    # Flatten the temperature data
    flattened_temp = temp[i].flatten()

    # Get the shape of the temperature array
    shape = temp.shape

    # Write to a raw file for each time step, format of the filename is the following dimZ.dimY.dimX
    flattened_temp.astype(np.float32).tofile('outputFds' + f'_T{time:.1f}_.' + str(shape[1]) + '.' + str(shape[2]) + '.' + str(shape[3]) + '.raw')