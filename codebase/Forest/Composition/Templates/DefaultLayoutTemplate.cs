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

using Forest.Composition.Templates.Mutable;


namespace Forest.Composition.Templates
{
    [Serializable]
    internal sealed class DefaultLayoutTemplate : IMutableLayoutTemplate
    {
        private readonly string id;
        private readonly IMutableLayoutTemplate master;
        private readonly IMutablePlaceholderContainer placeholderContainer;
        private readonly IMutableRegionContainer regionContainer;

        public DefaultLayoutTemplate(string id, IMutableLayoutTemplate master) : this(id, master, true) { }
        private DefaultLayoutTemplate(string id, IMutableLayoutTemplate master, bool transferFromMaster)
        {
            this.id = id;
            this.placeholderContainer = new PlaceholderContainer();
            this.regionContainer = new RegionContainer(this);
            if (master != null)
            {
                this.master = master;
                if (transferFromMaster)
                {
                    foreach (IMutableRegionTemplate regionTemplate in master.Regions)
                    {
                        var rt = TransferViewsContent(regionTemplate, new RegionTemplate(regionTemplate.RegionName, regionTemplate.Layout, this), this);
                        this.Regions[rt.RegionName] = rt;
                    }    
                }
            }
            else
            {
                this.master = null;
            }
            
        }

        private static IMutablePlaceholder ClonePlaceholder(IMutablePlaceholder placeholder, IMutableLayoutTemplate owner)
        {
            return TransferViewsContent(placeholder, new Placeholder(placeholder.ID, owner), owner);
        }

        private static T TransferViewsContent<T>(IMutableViewContainer sourceContainer, T destinationContainer, IMutableLayoutTemplate owner) where T: IMutableViewContainer
        {
            foreach (var bucket in sourceContainer.Buckets)
            {
                if (bucket is Placeholder.PlaceholderBucket)
                {
                    var placeholderBucket = (Placeholder.PlaceholderBucket) bucket;
                    var bucketPlaceholder = ClonePlaceholder(placeholderBucket.Placeholder, owner);
                    destinationContainer.AddPlaceholder(bucketPlaceholder);
                }
                else 
                {
                    foreach (IMutableViewTemplate viewTemplate in (LayoutContainerBase<IMutableViewTemplate>.ContainerBucketBase)bucket)
                    {
                        destinationContainer[viewTemplate.ID] = CloneViewTemplate(viewTemplate, owner);
                    }
                }
            }
            return destinationContainer;
        }

        private static IMutableViewTemplate CloneViewTemplate(IMutableViewTemplate viewTemplate, IMutableLayoutTemplate owner)
        {
            if (viewTemplate is IProxy<IMutableViewTemplate>)
            {
                return new ViewTemplateProxy(CloneViewTemplate(((IProxy<IMutableViewTemplate>) viewTemplate).Value, owner));
            }
            if (viewTemplate is IMutableLayoutTemplate)
            {
                return CloneLayoutTemplate((IMutableLayoutTemplate) viewTemplate);
            }
            var result = new ViewTemplate(viewTemplate.ID, owner);
            
            foreach (IMutableRegionTemplate regionTemplate in viewTemplate.Regions)
            {
                var rt = TransferViewsContent(regionTemplate, new RegionTemplate(regionTemplate.RegionName, regionTemplate.Layout, owner), owner);
                result.Regions[rt.RegionName] = rt;
            }

            return result;
        }

        private static IMutableLayoutTemplate CloneLayoutTemplate(IMutableLayoutTemplate layoutTemplate)
        {
            return layoutTemplate;
        }


        public IMutableLayoutTemplate Clone()
        {
            var result = new DefaultLayoutTemplate(this.id, this.master, false);
            foreach (IMutableRegionTemplate regionTemplate in this.Regions)
            {
                var rt = TransferViewsContent(regionTemplate, new RegionTemplate(regionTemplate.RegionName, regionTemplate.Layout, result), result);
                result.Regions[rt.RegionName] = rt;
            }
            return result;
        }

        public string ID { get { return this.id; } }
        public string Master { get { return this.master == null ? null : this.master.ID; } }

        public IMutablePlaceholderContainer Placeholders { get { return this.placeholderContainer; } }
        IPlaceholderContainer ILayoutTemplate.Placeholders { get { return this.Placeholders; } }

        public IMutableRegionContainer Regions { get { return this.regionContainer; } }
        IRegionContainer IViewTemplate.Regions { get { return this.Regions; } }

        IMutableLayoutTemplate IMutableViewTemplate.Template { get { return this; } }
    }
}
