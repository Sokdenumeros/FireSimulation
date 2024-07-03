import numpy as np
import fdsreader

def extract_f32_data(data_element,name,timestep):
    data = data_element.to_global(masked = True)
    times = data_element.times
    data = np.transpose(data,(0,2,3,1))

    previous = times[0] - timestep
    for i, time in enumerate(times):
        if time - previous >= timestep:
            flattened_data = data[i].flatten()
            flattened_data.astype(np.float32).tofile(name + f'_T{time:.1f}_.' + str(data.shape[1]) + '.' + str(data.shape[2]) + '.' + str(data.shape[3]) + '.raw')
            previous = round(time,2)

def extract_v3_data(de1, de2, de3, name,timestep):
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


# Load FDS simulation
sim = fdsreader.Simulation('./NIST_PoolFire/')

extract_f32_data(sim.slices[3],'temperature',0.5)

extract_v3_data(sim.slices[0],sim.slices[1],sim.slices[2],'velocity',0.5)

extract_f32_data(sim.smoke_3d[0],'smoke',0.5)

extract_f32_data(sim.smoke_3d[1],'hrpuv',0.5)