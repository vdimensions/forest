/*
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


namespace Forest.Stubs
{
    public interface IContainer
    {
        [Obsolete]
        IContainer CreateChildContainer();

        [Obsolete]
        IContainer RegisterInstance<T>(T obj);

        [Obsolete]
        IContainer RegisterPrototype<T>();
        [Obsolete]
        IContainer RegisterPrototype(Type t);

        [Obsolete]
        T Resolve<T>();

        [Obsolete]
        bool TryResolve<T>(Type vmType, out T viewModel);

        [Obsolete]
        IContainer Register(Type vmType);

        IView ResolveView(Type viewType, string id, object viewModel);
        IView ResolveView(Type viewType, string id, Type viewModelType);
    }
}