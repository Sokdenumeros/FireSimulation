import numpy as np

f = input('Enter file\n')
t = np.float16
if f[-1] == '8':
  t = np.uint8
a = np.fromfile(f,t,-1)
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