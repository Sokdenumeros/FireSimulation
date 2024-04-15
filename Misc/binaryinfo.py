import numpy as np

f = input('Enter file\n')
a = np.fromfile(f,np.float32,-1)
a = np.nan_to_num(a,0)
vals, cou = np.unique(a, return_counts= True)
print('VALUES')
print(vals)
print('COUNT')
print(cou)
print('NONZERO')
print(np.sum(cou[1:]))
print('MAXV')
print(vals[-1])