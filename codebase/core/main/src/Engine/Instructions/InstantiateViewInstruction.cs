﻿using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class InstantiateViewInstruction : TreeModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private object _model;
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly ViewHandle _viewHandle;
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly string _region;
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly string _owner;
        private readonly string _resourceBundle;

        public InstantiateViewInstruction(
            ViewHandle viewHandle, 
            string region, 
            string owner, 
            object model,
            string resourceBundle) : base(GuidGenerator.NewID().ToString())
        {
            _viewHandle = viewHandle;
            _region = region;
            _model = model;
            _owner = owner;
            _resourceBundle = resourceBundle ?? string.Empty;
        }

        protected override bool IsEqualTo(TreeModification other)
        {
            return other is InstantiateViewInstruction ivi 
                && base.IsEqualTo(other) 
                && Equals(ViewHandle, ivi.ViewHandle)
                && Equals(Region, ivi.Region)
                && Equals(Owner, ivi.Owner)
                && Equals(Model, ivi.Model)
                && Equals(ResourceBundle, ivi.ResourceBundle);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(NodeKey, ViewHandle, Region, Owner, Model, ResourceBundle);

        public ViewHandle ViewHandle => _viewHandle;

        public string Region => _region;

        public string Owner => _owner;

        public object Model => _model;
        
        public string ResourceBundle => _resourceBundle;
    }
}