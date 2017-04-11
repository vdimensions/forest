- Add Support for ad-hoc adding of views in regions.

- Discard the server-side template layouts. Instead:
  - Each view will list the logical regions it supports by being annoated with a new `Region` attribute. 
  - Each view will also allow dynamic creation of regions at runtime. 

- Improve the forest app state protocol to allow incremental interface composition. This means that the client layer will be given power
  to inject views in already existing regions of teh server-side viewstate.  
  The bottomline of this is to allow views to be developed as standalone widgets and to be composed