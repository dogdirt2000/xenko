﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Extensions;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Tests
{
    [TestFixture]
    class TestCore
    {
        public class Dummy : ViewModelBase, IComparable
        {
            private string name;

            public string Name { get { return name; } set { SetValue(ref name, value); } }

            public Dummy(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return Name;
            }

            public int CompareTo(object obj)
            {
                return obj == null ? 1 : String.Compare(Name, ((Dummy)obj).Name, StringComparison.Ordinal);
            }
        }

        [Test]
        public void TestSortedObservableCollection()
        {
            var collection = new SortedObservableCollection<int> { 5, 13, 2, 9, 0, 8, 5, 11, 1, 7, 14, 12, 4, 10, 3, 6 };
            collection.Remove(5);

            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i] == i);
                Assert.That(collection.BinarySearch(i) == i);
            }

            Assert.Throws<InvalidOperationException>(() => collection[4] = 10);
            Assert.Throws<InvalidOperationException>(() => collection.Move(4, 5));
        }

        [Test]
        public void TestAutoUpdatingSortedObservableCollection()
        {
            var collection = new AutoUpdatingSortedObservableCollection<Dummy> { new Dummy("sss"), new Dummy("eee") };

            var dummy = new Dummy("ggg");
            collection.Add(dummy);

            var sorted = new[] { "eee", "ggg", "sss" };

            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i].Name == sorted[i]);
                Assert.That(collection.BinarySearch(sorted[i], (d, s) => String.Compare(d.Name, s, StringComparison.Ordinal)) == i);
            }

            dummy.Name = "aaa";
            sorted = new[] { "aaa", "eee", "sss" };
            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i].Name == sorted[i]);
                Assert.That(collection.BinarySearch(sorted[i], (d, s) => String.Compare(d.Name, s, StringComparison.Ordinal)) == i);
            }

            dummy.Name = "zzz";
            sorted = new[] { "eee", "sss", "zzz" };
            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i].Name == sorted[i]);
                Assert.That(collection.BinarySearch(sorted[i], (d, s) => String.Compare(d.Name, s, StringComparison.Ordinal)) == i);
            }
        }

        [Test]
        public void TestCamelCaseSplit()
        {
            var inputStrings = new[]
            {
                "ThisIsOneTestString",
                "ThisOneABCContainsAbreviation",
                "ThisOneContainsASingleCharacterWord",
                "ThisOneEndsWithAbbreviationABC",
                "ThisOneEndsWithASingleCharacterWordZ",
                "  This OneContains   SpacesBetweenSome OfThe Words  ",
            };
            var expectedResult = new[]
            {
                new[] { "This", "Is", "One", "Test", "String" },
                new[] { "This", "One", "ABC", "Contains", "Abreviation" },
                new[] { "This", "One", "Contains", "A", "Single", "Character", "Word" },
                new[] { "This", "One", "Ends", "With", "Abbreviation", "ABC" },
                new[] { "This", "One", "Ends", "With", "A", "Single", "Character", "Word", "Z" },
                new[] { "This", "One", "Contains", "Spaces", "Between", "Some", "Of", "The", "Words" },
            };

            foreach (var testCase in inputStrings.Zip(expectedResult))
            {
                var split = testCase.Item1.CamelCaseSplit();
                Assert.AreEqual(testCase.Item2.Length, split.Count);
                for (var i = 0; i < testCase.Item2.Length; ++i)
                {
                    Assert.AreEqual(testCase.Item2[i], split[i]);
                }
            }
        }
    }
}
