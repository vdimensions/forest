﻿/**
 * Copyright 2014 vdimensions.net.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;


namespace Forest.Composition
{
    public interface IViewRegistry
    {
        //IViewEntry Lookup(string id);
        //IViewEntry Lookup(Type viewModelType);
        IViewRegistry Register(Type viewType);
        IViewRegistry Register(params Type[] viewTypes);
        IViewRegistry Register<T>() where T: IView;
        IViewRegistry Unregister(Type viewType);
        IViewRegistry Unregister<T>() where T: IView;
    }
}