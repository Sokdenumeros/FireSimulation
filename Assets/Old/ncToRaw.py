import netCDF4

f = netCDF4.Dataset("acetone_03circ_ronan_000010_00.nc")
d = f.variables['TEMPERATURE'][:,:,:].data.flatten()
shape = f.variables['TEMPERATURE'].shape
#shape[0] dimz
#shape[1] dimy
#shape[2] dimx
d.tofile('output.' + str(shape[0]) + '.' + str(shape[1]) + '.' + str(shape[2]) + '.raw')