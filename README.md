# SEGYRead

A library created to read in .segy seismic files

# What does this program do?

While previously relying on other types of seismic file readers, it was made a priority to create a library of my own seismic file readers. In SEGYRead, it applies information provided from SEGY file documentation to correctly traverse through each SEGY file structure. It is then determined which format the data is in (for example, 4-byte IEEE floating-point or 2-byte two's complement integer). Finally, it would gather all the data needed for graphing and store that data in a class structure that I created. In doing so, it was much easier to access data with a class structure I implemented rather than having to learn a library's class structure.  
