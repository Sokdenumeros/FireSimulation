# VR Fire Visualization

## Prerequisites
Newer or older versions of the listed prerequisites may also work.
- Unity 2022.3.15f1 
- Python 3.11.9
- [FDSReader](https://pypi.org/project/fdsreader/) 1.10.2
- [NumPy](https://numpy.org/) 1.26.2

## Installation
Clone the repository and open the Unity project from the Unity Hub

## Preprocessing a simulation
Three steps are required to visualize a simulation. Two preprocessing steps using python scripts and finally, the visualization.

### Extracting data
After cloning the repository, go into the Misc directory. From there, run the fdstoraw3.py script, it will then ask for a simulation directory. You can manually input the path to your simulation, but most consoles will also input the path to a file if you drag and drop it into the terminal window.
![Dragandrop1](https://github.com/Sokdenumeros/MIRI-SV-MEDVIZ/blob/main/2rays.png?raw=true)
![Dragandrop2](https://github.com/Sokdenumeros/MIRI-SV-MEDVIZ/blob/main/2rays.png?raw=true)

This should create two directories named SOOTDENSITY and HRPUV with many files in them. If the names of the directories do not match the information you wanted to extratct, make sure that the fields of the simulation being accessed in the last lines of the fdstoraw3.py script are the correct ones.

If you get the following error message:
numpy.core._exceptions._ArrayMemoryError: Unable to allocate 112. GiB for an array with shape (398, 641, 293, 201) and data type float16
This means that your system does not have enough RAM to store the entire simulation. Try [increasing the Virtual Memory](https://support.esri.com/en-us/knowledge-base/increase-virtual-memory-beyond-the-recommended-maximum--000011346) to around 1.5x the ammount requested by the error. 112. GiB -> 150000 MB

### Randomly selecting particles
If you had to increase the Virtual memory in the previous section, you can already go back to "Automatically manage paging file size for all drives".

Run rawtoparticles.py, it will ask for the smoke and heat directories. Similar to the previous section, you can manually write the path to each one or drag and drop the directories. The script will extract a random subsample of the data into the 'particles' and 'partData' directories.

In the first lines of the script, it is possible to adjust four parameters:
| Variable Name |  Description  |
|:---:|----------|
| nparticles  | Maximum number of particles that can be extracted from a single time instant.|
| opacityThreshold | Particles with a SootDensity below the threshold will be excluded from the list of potential particles to be selected.|
| heatThreshold | Particles with a HRPUV below the threshold will be excluded from the list of potential fire particles to be selected. |
| smokeparticlesratio | Max fraction from the total number of nparticles that can be smoke particles (soot > opacityThreshold & HRPUV < heatThreshold) |

## Visualization
In order to visualize a simulation, it first needs to be preprocessed as explained in the previous section.

Open the desired Unity scene, 2D_template or VR_template. In both you will see a SimulationManager object with one SimulationManager script, two ByteLoader scripts and a DisableFrustrumCulling script.

![Dragandrop1](https://github.com/Sokdenumeros/MIRI-SV-MEDVIZ/blob/main/2rays.png?raw=true)

### VR controls
In the VR_template following controls have been implemented for the Meta Quest 3, different headsets may have other input mappings.
| VR Controller Key |  Action  |
|:---:|----------|
| Left Joystick | Move horizontally |
| Left Front Trigger | Move upwards |
| Left Lateral Trigger | Move downwards |
