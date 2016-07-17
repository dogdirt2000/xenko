﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Design.Tests
{
    /// <summary>
    /// Testing <see cref="DataVisitorBase"/>
    /// </summary>
    [TestFixture]
    public class TestDataMemberVisitor
    {
        /// <summary>
        /// An primitive grabber just iterate through the whole object hierarchy
        /// and collect all primitives.
        /// </summary>
        public class PrimitiveGrabber : DataVisitorBase
        {
            public List<object> Collected { get; private set; }

            public PrimitiveGrabber()
            {
                Collected = new List<object>();
            }

            public override void Reset()
            {
                Collected.Clear();
                base.Reset();
            }

            public override void VisitNull()
            {
                Collected.Add(null);
            }

            public override void VisitPrimitive(object primitive, PrimitiveDescriptor descriptor)
            {
                Collected.Add(primitive);
            }
        }

        public class SimpleObject
        {
            public SimpleObject()
            {
            }

            public SimpleObject(int firstValue, int secondValue, int thirdValue, int fourthValue)
            {
                FirstValue = firstValue;
                SecondValue = secondValue;
                ThirdValue = thirdValue;
                FourthValue = fourthValue;
                Collection = new List<object>();
                Dictionary = new Dictionary<object, object>();
                ReadOnlyValue = 1234;
            }

            [DataMember(0)]
            public int FirstValue { get; set; }

            [DataMember(1)]
            public int SecondValue { get; set; }

            [DataMember(2)]
            public int ThirdValue { get; set; }

            [DataMember(3)]
            public int? FourthValue { get; set; }

            [DataMemberIgnore]
            public int MemberToIgnore { get; set; }

            [DataMember(4)]
            public string Name { get; set; }

            [DataMember(5)]
            public List<object> Collection { get; set; }

            [DataMember(6)]
            public Dictionary<object, object> Dictionary { get; set; }

            [DataMember(7, DataMemberMode.ReadOnly)]
            public int ReadOnlyValue { get; private set; }

            public SimpleObject SubObject { get; set; }
        }

        [Test]
        public void TestVisitPrimitive()
        {
            var simpleObject = new SimpleObject(1, 2, 3, 4) { Name = "Test", MemberToIgnore = int.MaxValue, SubObject = new SimpleObject(5, 6, 7, 8) };
            var primitiveGrabber = new PrimitiveGrabber();
            primitiveGrabber.Visit(simpleObject);
            Assert.AreEqual(new List<object>()
                {
                    1, 
                    2, 
                    3, 
                    4, 
                    "Test", 
                    1234, // SimpleObject.ReadOnlyValue
                    5, // simpleObject.SubObject
                    6, 
                    7, 
                    8,
                    null, // simpleObject.SubObject.Name
                    1234, // SimpleObject.ReadOnlyValue
                    null // simpleObject.SubObject.SubObject
                }, primitiveGrabber.Collected);

            simpleObject.Collection.Add("Item1");
            simpleObject.Collection.Add("Item2");

            simpleObject.Dictionary.Add("Key1", "Value1");
            simpleObject.Dictionary.Add("Key2", "Value2");

            primitiveGrabber.Reset();
            primitiveGrabber.Visit(simpleObject);
            Assert.AreEqual(new List<object>()
                {
                    1, 
                    2, 
                    3, 
                    4, 
                    "Test", 
                    "Item1", "Item2", // simpleObject.Collection
                    "Key1", "Value1", "Key2", "Value2", // simpleObject.Dictionary
                    1234, // SimpleObject.ReadOnlyValue
                    5, // simpleObject.SubObject
                    6, 
                    7, 
                    8, 
                    null, // simpleObject.SubObject.Name
                    1234, // SimpleObject.ReadOnlyValue
                    null // simpleObject.SubObject.SubObject
                }, primitiveGrabber.Collected);
        }

        public class CustomList : List<object>
        {
            [DataMember(0)]
            public int CustomId { get; set; }

            [DataMember(1)]
            public string Name { get; set; }
        }
        
        [Test]
        public void TestCollection()
        {
            var customList = new CustomList() {1, 2, 3, 4};
            customList.CustomId = 10;
            customList.Name = "Test";
            var primitiveGrabber = new PrimitiveGrabber();
            primitiveGrabber.Visit(customList);
            Assert.AreEqual(new List<object>()
                {
                    10, // customList.CustomId
                    "Test", // customList.Name
                    1, 
                    2, 
                    3, 
                    4, 
                }, primitiveGrabber.Collected);
        }


        public class CustomDictionary : Dictionary<object, object>
        {
            [DataMember(0)]
            public int CustomId { get; set; }

            [DataMember(1)]
            public string Name { get; set; }
        }

        [Test]
        public void TestDictionary()
        {
            var customDict = new CustomDictionary() { {"Key1", "Value1"}, {"Key2", "Value2"}};
            customDict.CustomId = 10;
            customDict.Name = "Test";
            var primitiveGrabber = new PrimitiveGrabber();
            primitiveGrabber.Visit(customDict);
            Assert.AreEqual(new List<object>()
                {
                    10, // customList.CustomId
                    "Test", // customList.Name
                    "Key1", "Value1",
                    "Key2", "Value2",
                }, primitiveGrabber.Collected);
        }

        [Test]
        public void TestArray()
        {
            var customArray = new int[] {1, 2, 3, 4};
            var primitiveGrabber = new PrimitiveGrabber();
            primitiveGrabber.Visit(customArray);
            Assert.AreEqual(new List<object>()
                {
                    1,2,3,4
                }, primitiveGrabber.Collected);
        }
    }
}