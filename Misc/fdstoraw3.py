import numpy as np
import fdsreader
import os
from typing import Dict, Tuple, Literal, Union
import math

#sim.smoke_3d[0].subsmokes[0].times
#sim.smoke_3d[0].subsmokes[0].data
#sim.smoke_3d[0].subsmokes[0].mesh.coordinates

def extract_f32_data(data_element,name):
    try:
        os.mkdir(name)
    except:
        print('Directory exists already')

    try:
        data, grid = to_global(data_element,return_coordinates=True,masked=True)
    except:
        data, grid = data_element.to_global(return_coordinates=True,masked=True)

    times = data_element.times
    data = np.transpose(data,(0,2,3,1))
    
    print('Writing to files')
    
    grid['x'].astype(np.float32).tofile(name + '/gridX.raw')
    grid['y'].astype(np.float32).tofile(name + '/gridZ.raw')
    grid['z'].astype(np.float32).tofile(name + '/gridY.raw')
    for i, time in enumerate(times):
        t = round(time,2)
        flattened_data = data[i].flatten()
        flattened_data.astype(np.float16).tofile(name + '/' + f'T{time:07.2f}_.' + str(data.shape[1]) + '.' + str(data.shape[2]) + '.' + str(data.shape[3]) + '.raw')
    print('FINISHED')

#DEPRECATED
def extract_v3_data(de1, de2, de3, name):
    print('THIS FUNCTION IS DEPRECATED')
    times = de1.times
    data1 = de1.to_global(masked = True)
    data2 = de2.to_global(masked = True)
    data3 = de3.to_global(masked = True)
    #in unity its xyz with y being the vertical component while in smokeview its xyz with z being the vertical component
    vectors = np.stack([data1, data3, data2], axis=4)
    vectors = np.transpose(vectors, (0, 2, 3, 1, 4))

    previous = times[0] - timestep
    for i, time in enumerate(times):
        if time - previous >= timestep:
            flattened_data = vectors[i].flatten()
            flattened_data.astype(np.float32).tofile(name + f'_T{time:.1f}_.' + str(vectors.shape[1]) + '.' + str(vectors.shape[2]) + '.' + str(vectors.shape[3]) + '.raw')
            previous = round(time,2)

def to_global(smk3d, masked: bool = False, fill: float = 0, return_coordinates: bool = False) -> Union[np.ndarray, Tuple[np.ndarray, Dict[Literal['x', 'y', 'z'], np.ndarray]]]:
    if len(smk3d._subsmokes) == 0:
        if return_coordinates:
            return np.array([]), {d: np.array([]) for d in ('x', 'y', 'z')}
        else:
            return np.array([])

    coord_min = {'x': math.inf, 'y': math.inf, 'z': math.inf}
    coord_max = {'x': -math.inf, 'y': -math.inf, 'z': -math.inf}
    for dim in ('x', 'y', 'z'):
        for subsmoke in smk3d._subsmokes.values():
            co = subsmoke.mesh.coordinates[dim]
            coord_min[dim] = min(co[0], coord_min[dim])
            coord_max[dim] = max(co[-1], coord_max[dim])

        # The global grid will use the finest mesh as base and duplicate values of the coarser
        # meshes. Therefore, we first find the finest mesh and calculate the step size in each
        # dimension.
    step_sizes_min = {'x': coord_max['x'] - coord_min['x'], 'y': coord_max['y'] - coord_min['y'], 'z': coord_max['z'] - coord_min['z']}
    step_sizes_max = {'x': 0, 'y': 0, 'z': 0}
    steps = dict()
    global_max = {'x': -math.inf, 'y': -math.inf, 'z': -math.inf}

    for dim in ('x', 'y', 'z'):
        for subsmoke in smk3d._subsmokes.values():
            step_size = subsmoke.mesh.coordinates[dim][1] - subsmoke.mesh.coordinates[dim][0]
            step_sizes_min[dim] = min(step_size, step_sizes_min[dim])
            step_sizes_max[dim] = max(step_size, step_sizes_max[dim])
            global_max[dim] = max(subsmoke.mesh.coordinates[dim][-1], global_max[dim])

    for dim in ('x', 'y', 'z'):
        if step_sizes_min[dim] == 0:
            step_sizes_min[dim] = math.inf
            steps[dim] = 1
        else:
            steps[dim] = max(int(round((coord_max[dim] - coord_min[dim]) / step_sizes_min[dim])),1) + 1  # + step_sizes_max[dim] / step_sizes_min[dim]

    grid = np.full((smk3d.n_t, steps['x'], steps['y'], steps['z']), np.nan, np.float16)

    print('TOTAL SUBSMOKES: '+str(len(smk3d._subsmokes.values())))
    for i, subsmoke in enumerate(smk3d._subsmokes.values()):
        #if subsmoke.mesh.coordinates['x'][1] - subsmoke.mesh.coordinates['x'][0] > 1.5*step_sizes_min['x']:
        #    continue
        #if subsmoke.mesh.coordinates['y'][1] - subsmoke.mesh.coordinates['y'][0] > 1.5*step_sizes_min['y']:
        #    continue
        #if subsmoke.mesh.coordinates['z'][1] - subsmoke.mesh.coordinates['z'][0] > 1.5*step_sizes_min['z']:
        #    continue
        subsmoke_data = subsmoke.data.astype(np.float16, order = 'C')
        print('SUBSMOKE ' + str(i))
        if masked:
            mask = subsmoke.mesh.get_obstruction_mask(smk3d.times).astype(np.float16)

        start_idx = {dim: int(round((subsmoke.mesh.coordinates[dim][0] - coord_min[dim]) / step_sizes_min[dim])) for dim in ('x', 'y', 'z')}
        end_idx = {dim: int(round((subsmoke.mesh.coordinates[dim][-1] - coord_min[dim]) / step_sizes_min[dim])) for dim in ('x', 'y', 'z')}

            # We ignore border points unless they are actually on the border of the simulation space as all
            # other border points actually appear twice, as the subslices overlap. This only
            # applies for face_centered slices, as cell_centered slices will not overlap.
        
        for axis in range(3):
            dim = ('x', 'y', 'z')[axis]
            n_repeat = max(int(round((subsmoke.mesh.coordinates[dim][1] - subsmoke.mesh.coordinates[dim][0]) /step_sizes_min[dim])), 1)
            if n_repeat > 1:
                subsmoke_data = np.repeat(subsmoke_data, n_repeat, axis=axis + 1)
                if masked:
                    mask = np.repeat(mask, n_repeat, axis=axis + 1)

            # If the slice should be masked, we set all cells at which an obstruction is in the
            # simulation space to the fill value set by the user
        if masked:
            subsmoke_data = np.where(mask, subsmoke_data, fill)

        grid[:, start_idx['x']: end_idx['x'], start_idx['y']: end_idx['y'],
        start_idx['z']: end_idx['z']] = subsmoke_data[:, : end_idx['x'] - start_idx['x'], :end_idx['y'] - start_idx['y'], :end_idx['z'] - start_idx['z']]
    if return_coordinates:
        coordinates = dict()
        for dim_index, dim in enumerate(('x', 'y', 'z')):
            coordinates[dim] = np.linspace(coord_min[dim], coord_max[dim], grid.shape[dim_index + 1])

    if return_coordinates:
        return grid, coordinates
    else:
        return grid

# Load FDS simulation
f = input('Enter file\n')
try:
    sim = fdsreader.Simulation(f[1:-1])
except:
    sim = fdsreader.Simulation(f)

#extract_f32_data(sim.slices[3],'temperature',0.5)

#extract_v3_data(sim.slices[0],sim.slices[1],sim.slices[2],'velocity',0.5)

extract_f32_data(sim.smoke_3d[0],'smokePinos')
extract_f32_data(sim.smoke_3d[1],'hrpuvPinos')