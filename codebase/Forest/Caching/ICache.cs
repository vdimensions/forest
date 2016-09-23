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


namespace Forest.Caching
{
    public interface ICache
    {
        bool Delete(object key);

        ICache Add(object key, object value);
        ICache Add<T>(object key, T value);

        object GetOrAdd(object key, object valueToAdd);
        object GetOrAdd(object key, Func<object> valueFactory);

        T GetOrAdd<T>(object key, T valueToAdd);
        T GetOrAdd<T>(object key, Func<T> valueFactory);

        object this[object key] { get; set; }
    }
}