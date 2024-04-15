import numpy as np
import fdsreader
import math

f = input('Enter file\n')
try:
    sim = fdsreader.Simulation(f[1:-1])
except:
    sim = fdsreader.Simulation(f)
for s in sim.smoke_3d:
    coord_min = {'x': math.inf, 'y': math.inf, 'z': math.inf}
    coord_max = {'x': -math.inf, 'y': -math.inf, 'z': -math.inf}
    for dim in ('x', 'y', 'z'):
        for subsmoke in s._subsmokes.values():
            co = subsmoke.mesh.coordinates[dim]
            coord_min[dim] = min(co[0], coord_min[dim])
            coord_max[dim] = max(co[-1], coord_max[dim])

    step_sizes_min = {'x': coord_max['x'] - coord_min['x'], 'y': coord_max['y'] - coord_min['y'], 'z': coord_max['z'] - coord_min['z']}
    step_sizes_max = {'x': 0, 'y': 0, 'z': 0}

    for dim in ('x', 'y', 'z'):
        for subsmoke in s._subsmokes.values():
            step_size = subsmoke.mesh.coordinates[dim][1] - subsmoke.mesh.coordinates[dim][0]
            step_sizes_min[dim] = min(step_size, step_sizes_min[dim])
            step_sizes_max[dim] = max(step_size, step_sizes_max[dim])


    print('---------------------------------')
    print(s.quantity)
    print('MinTime :'+str(s.times[0]))
    print('MaxTime :'+str(s.times[-1]))
    print('Timestep : '+str((s.times[-1] - s.times[0])/s.n_t))
    print('MinCoords: '+str(coord_min))
    print('MaxCoords: '+str(coord_max))
    print('MinSteps: '+str(step_sizes_min))
    print('MaxSteps: '+str(step_sizes_max))