// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes the configuration composed by a collection of <see cref="VirtualButtonBinding"/>.
    /// </summary>
    public class VirtualButtonConfig : TrackingCollection<VirtualButtonBinding>
    {
        private readonly Dictionary<object, List<VirtualButtonBinding>> mapBindings;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualButtonConfig" /> class.
        /// </summary>
        public VirtualButtonConfig()
        {
            mapBindings = new Dictionary<object, List<VirtualButtonBinding>>();
            CollectionChanged += bindings_CollectionChanged;
        }

        /// <summary>
        /// Gets the binding names.
        /// </summary>
        /// <value>The binding names.</value>
        public IEnumerable<object> BindingNames
        {
            get
            {
                return mapBindings.Keys;
            }
        }

        public virtual float GetValue(InputManager inputManager, object name)
        {
            float value = 0.0f;
            List<VirtualButtonBinding> bindingsPerName;
            if (mapBindings.TryGetValue(name, out bindingsPerName))
            {
                foreach (var virtualButtonBinding in bindingsPerName)
                {
                    float newValue = virtualButtonBinding.GetValue(inputManager);
                    if (Math.Abs(newValue) > Math.Abs(value))
                    {
                        value = newValue;
                    }
                }
            }

            return value;
        }

        void bindings_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var virtualButtonBinding = (VirtualButtonBinding)e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddBinding(virtualButtonBinding);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveBinding(virtualButtonBinding);
                    break;
            }
        }

        private void AddBinding(VirtualButtonBinding virtualButtonBinding)
        {
            List<VirtualButtonBinding> bindingsPerName;
            if (!mapBindings.TryGetValue(virtualButtonBinding.Name, out bindingsPerName))
            {
                bindingsPerName = new List<VirtualButtonBinding>();
                mapBindings.Add(virtualButtonBinding.Name, bindingsPerName);
            }
            bindingsPerName.Add(virtualButtonBinding);
        }

        private void RemoveBinding(VirtualButtonBinding virtualButtonBinding)
        {
            List<VirtualButtonBinding> bindingsPerName;
            if (mapBindings.TryGetValue(virtualButtonBinding.Name, out bindingsPerName))
            {
                bindingsPerName.Remove(virtualButtonBinding);
            }
        }
    }
}
