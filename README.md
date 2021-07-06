# Forest

|Item|Status|  
|-|-|  
|CI Build (master)|[![Build status](https://ci.appveyor.com/api/projects/status/x6h5xcqbjn0nji0h/branch/master?svg=true)](https://ci.appveyor.com/project/ivaylo5ev/forest/branch/master)|  
|Forest.Core|[![#](https://img.shields.io/nuget/v/Forest.Core.svg)](https://www.nuget.org/packages/Forest.Core/)|  
|Forest.Forms|[![#](https://img.shields.io/nuget/v/Forest.Forms.svg)](https://www.nuget.org/packages/Forest.Forms/)|  
|Forest.Web.AspNetCore|[![#](https://img.shields.io/nuget/v/Forest.Web.AspNetCore.svg)](https://www.nuget.org/packages/Forest.Web.AspNetCore/)|  

----

This is the home of the Forest project. The name Forest stands for Front-end Over REST, and initially the purpose of this
project was to enable creation of operational backend applications usable entirely from a REST-ful endpoint.  
Inspired by the HATEOAS principles, the aim for Forest is to become a HATEOAS-like wrapper around entire applications.

## Forest.Core

This is the core library of the Forest framework

## Forest.Forms

This library serves as an abstraction to commonly used UI components found on many front-end frameworks targeting various platforms.  

Example components include:

- Repeater
- TabStripView
- DialogSystem
- Navigation and Breadcrumbs
- FormControls and FormValidation

## Forest.Web.AspNetCore

Contains modules to expose the Forest engine as a ready-to-use rest-ful controller.  
One may directly use the controller as it is capable of serving a working hateoas application, or extend the restful API with custom endpoints and abilities to handle specific data formats which need to be received by the restful endpoints and/or be pre-processed prior handing them to the Forest engine.
